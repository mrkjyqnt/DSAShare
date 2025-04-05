Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Security.Principal
Imports System.Diagnostics

#Disable Warning
Module ConfigurationModule
    ' Debug mode flag
#If DEBUG Then
    Private Const DebugMode As Boolean = True
#Else
    Private Const DebugMode As Boolean = False
#End If

    ' File paths
    Private ReadOnly DebugConfigPath As String = "AppConfig.debug.json"
    Private ReadOnly RuntimeConfigPath As String = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DSAShare",
        "AppConfig.secure")

    ' Secret key
    Private ReadOnly SecretKey As String = "DSAShare"

    ' Default configuration
    Private ReadOnly DefaultConfig As New AppSettings With {
        .Database = New DatabaseSettings With {
            .Server = "192.168.8.10\SQLEXPRESS",
            .Name = "dsa_share_database",
            .Username = "member",
            .Password = "member"
        },
        .Network = New NetworkSettings With {
            .FolderPath = "\\192.168.8.10\ServerStorage\",
            .Username = "user",
            .Password = "user"
        }
    }

    ' Configuration classes
    Public Class AppSettings
        Public Property Database As DatabaseSettings
        Public Property Network As NetworkSettings
    End Class

    Public Class DatabaseSettings
        Public Property Server As String
        Public Property Name As String
        Public Property Username As String
        Public Property Password As String
    End Class

    Public Class NetworkSettings
        Public Property FolderPath As String
        Public Property Username As String
        Public Property Password As String
    End Class

    ' JSON serialization options
    Private ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True,
        .PropertyNameCaseInsensitive = True,
        .Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }

    ' Public interface
    Public Function GetSettings() As AppSettings
        Try
            Debug.WriteLine($"Loading configuration in {(If(DebugMode, "DEBUG", "RELEASE"))} mode")
            Return If(DebugMode, LoadDebugConfig(), LoadRuntimeConfig())
        Catch ex As Exception
            ErrorHandler.SetError($"Failed to load configuration: {ex.Message}")
            Debug.WriteLine($"GetSettings error: {ex.ToString()}")
            Return DefaultConfig
        End Try
    End Function

    Public Sub SaveSettings(settings As AppSettings)
        Try
            Debug.WriteLine($"Saving configuration in {(If(DebugMode, "DEBUG", "RELEASE"))} mode")
            If DebugMode Then
                SaveDebugConfig(settings)
            Else
                SaveRuntimeConfig(settings)
            End If
            VerifyFileCreation()
        Catch ex As Exception
            ErrorHandler.SetError($"Failed to save configuration: {ex.Message}")
            Debug.WriteLine($"SaveSettings error: {ex.ToString()}")
            Throw
        End Try
    End Sub

    Public Sub ResetToDefaults()
        Try
            Debug.WriteLine("Resetting configuration to defaults")
            If DebugMode Then
                SaveDebugConfig(DefaultConfig)
            Else
                SaveRuntimeConfig(DefaultConfig)
            End If
        Catch ex As Exception
            ErrorHandler.SetError($"Failed to reset configuration: {ex.Message}")
            Debug.WriteLine($"ResetToDefaults error: {ex.ToString()}")
            Throw
        End Try
    End Sub

#Region "Debug Mode Implementation"
    Private Function LoadDebugConfig() As AppSettings
        Try
            If File.Exists(DebugConfigPath) Then
                Dim json As String = File.ReadAllText(DebugConfigPath)
                Debug.WriteLine($"Loaded debug config from: {Path.GetFullPath(DebugConfigPath)}")
                Return JsonSerializer.Deserialize(Of AppSettings)(json, JsonOptions)
            Else
                Debug.WriteLine("Debug config not found, creating with defaults")
                SaveDebugConfig(DefaultConfig)
                Return DefaultConfig
            End If
        Catch ex As Exception
            Debug.WriteLine($"Debug config load failed: {ex.Message}")
            Throw New ApplicationException("Failed to load debug configuration", ex)
        End Try
    End Function

    Private Sub SaveDebugConfig(config As AppSettings)
        Try
            Dim json As String = JsonSerializer.Serialize(config, JsonOptions)
            File.WriteAllText(DebugConfigPath, json)
            Debug.WriteLine($"Debug config saved to: {Path.GetFullPath(DebugConfigPath)}")
        Catch ex As Exception
            Debug.WriteLine($"Debug config save failed: {ex.Message}")
            Throw New ApplicationException("Failed to save debug configuration", ex)
        End Try
    End Sub
#End Region

#Region "Runtime (Secure) Mode Implementation"
    Private Function LoadRuntimeConfig() As AppSettings
        Try
            If File.Exists(RuntimeConfigPath) Then
                Dim encryptedBytes As Byte() = File.ReadAllBytes(RuntimeConfigPath)
                If encryptedBytes.Length = 0 Then
                    Debug.WriteLine("Empty config file detected, using defaults")
                    Return DefaultConfig
                End If

                Dim json As String = DecryptStringFromBytes(encryptedBytes)
                Debug.WriteLine($"Loaded secure config from: {RuntimeConfigPath}")
                Return JsonSerializer.Deserialize(Of AppSettings)(json, JsonOptions)
            End If
        Catch cryptoEx As CryptographicException
            Debug.WriteLine($"Invalid config encryption: {cryptoEx.Message}")
            File.Delete(RuntimeConfigPath)
            Throw New ApplicationException("Configuration corrupted - reset to defaults", cryptoEx)
        Catch ex As Exception
            Debug.WriteLine($"Runtime config load failed: {ex.Message}")
            Throw New ApplicationException("Failed to load runtime configuration", ex)
        End Try

        Debug.WriteLine("No config file found, using defaults")
        Return DefaultConfig
    End Function

    Private Sub SaveRuntimeConfig(config As AppSettings)
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(RuntimeConfigPath))
            
            Dim json As String = JsonSerializer.Serialize(config, JsonOptions)
            Dim encryptedBytes As Byte() = EncryptStringToBytes(json)
            
            File.WriteAllBytes(RuntimeConfigPath, encryptedBytes)
            File.SetAttributes(RuntimeConfigPath, FileAttributes.Hidden)
            
            Debug.WriteLine($"Secure config saved to: {RuntimeConfigPath}")
        Catch ex As Exception
            Debug.WriteLine($"Runtime config save failed: {ex.Message}")
            Throw New ApplicationException("Failed to save runtime configuration", ex)
        End Try
    End Sub

    Private Function EncryptStringToBytes(plainText As String) As Byte()
        Using aes As Aes = Aes.Create()
            Dim keyMaterial = $"{SecretKey}-{Environment.MachineName}-{Environment.UserName}"
            Using rfc As New Rfc2898DeriveBytes(keyMaterial, Encoding.UTF8.GetBytes("DSAShareSalt"), 10000)
                aes.Key = rfc.GetBytes(32)
                aes.IV = rfc.GetBytes(16)
            End Using

            Using encryptor = aes.CreateEncryptor()
                Using ms = New MemoryStream()
                    ms.Write(aes.IV, 0, aes.IV.Length)
                    
                    Using cs = New CryptoStream(ms, encryptor, CryptoStreamMode.Write)
                        Using sw = New StreamWriter(cs)
                            sw.Write(plainText)
                        End Using
                    End Using
                    Return ms.ToArray()
                End Using
            End Using
        End Using
    End Function

    Private Function DecryptStringFromBytes(cipherText As Byte()) As String
        If cipherText Is Nothing OrElse cipherText.Length < 17 Then
            Throw New CryptographicException("Invalid cipher text length")
        End If

        Using aes As Aes = Aes.Create()
            Dim iv(15) As Byte
            Array.Copy(cipherText, 0, iv, 0, iv.Length)
            aes.IV = iv
            
            Dim keyMaterial = $"{SecretKey}-{Environment.MachineName}-{Environment.UserName}"
            Using rfc As New Rfc2898DeriveBytes(keyMaterial, Encoding.UTF8.GetBytes("DSAShareSalt"), 10000)
                aes.Key = rfc.GetBytes(32)
            End Using

            Using decryptor = aes.CreateDecryptor()
                Using ms = New MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length)
                    Using cs = New CryptoStream(ms, decryptor, CryptoStreamMode.Read)
                        Using sr = New StreamReader(cs)
                            Return sr.ReadToEnd()
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Function

    Private Sub VerifyFileCreation()
        Try
            Dim targetPath = If(DebugMode, DebugConfigPath, RuntimeConfigPath)
            If File.Exists(targetPath) Then
                Debug.WriteLine($"Config verification: File exists at {targetPath}")
                If Not DebugMode Then
                    Debug.WriteLine($"Hidden attribute: {(File.GetAttributes(targetPath) And FileAttributes.Hidden) = FileAttributes.Hidden}")
                End If
            Else
                Debug.WriteLine($"Config verification: FILE NOT FOUND at {targetPath}")
                Debug.WriteLine($"Possible causes:")
                Debug.WriteLine($"1. Missing write permissions")
                Debug.WriteLine($"2. Anti-virus blocking")
                Debug.WriteLine($"3. Path: {Path.GetDirectoryName(targetPath)}")
            End If
        Catch ex As Exception
            Debug.WriteLine($"Verification error: {ex.Message}")
        End Try
    End Sub
#End Region
End Module