Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Security.Principal

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

    Private ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True,
        .PropertyNameCaseInsensitive = True
    }

    Public Function GetSettings() As AppSettings
        Return If(DebugMode, LoadDebugConfig(), LoadRuntimeConfig())
    End Function

    Public Sub SaveSettings(settings As AppSettings)
        If DebugMode Then
            SaveDebugConfig(settings)
        Else
            SaveRuntimeConfig(settings)
        End If
    End Sub

    Public Sub ResetToDefaults()
        If DebugMode Then
            SaveDebugConfig(DefaultConfig)
        Else
            SaveRuntimeConfig(DefaultConfig)
        End If
    End Sub

#Region "Debug Mode Implementation"
    Private Function LoadDebugConfig() As AppSettings
        Try
            If File.Exists(DebugConfigPath) Then
                Dim json As String = File.ReadAllText(DebugConfigPath)
                Return JsonSerializer.Deserialize(Of AppSettings)(json, JsonOptions)
            Else
                SaveDebugConfig(DefaultConfig)
                Return DefaultConfig
            End If
        Catch ex As Exception
            Debug.WriteLine($"Debug config load error: {ex.Message}")
            Return DefaultConfig
        End Try
    End Function

    Private Sub SaveDebugConfig(config As AppSettings)
        Try
            Dim json As String = JsonSerializer.Serialize(config, JsonOptions)
            File.WriteAllText(DebugConfigPath, json)
            Debug.WriteLine($"Debug config saved to: {Path.GetFullPath(DebugConfigPath)}")
        Catch ex As Exception
            Debug.WriteLine($"Debug config save error: {ex.Message}")
            Throw
        End Try
    End Sub
#End Region

#Region "Runtime (Secure) Mode Implementation"
    Private Function LoadRuntimeConfig() As AppSettings
        Try
            If File.Exists(RuntimeConfigPath) Then
                Dim encryptedBytes As Byte() = File.ReadAllBytes(RuntimeConfigPath)
                Dim json As String = DecryptStringFromBytes(encryptedBytes)
                Return JsonSerializer.Deserialize(Of AppSettings)(json, JsonOptions)
            End If
        Catch ex As Exception
            Debug.WriteLine($"Runtime config load error: {ex.Message}")
        End Try

        Return DefaultConfig
    End Function

    Private Sub SaveRuntimeConfig(config As AppSettings)
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(RuntimeConfigPath))
            Dim json As String = JsonSerializer.Serialize(config, JsonOptions)
            Dim encryptedBytes As Byte() = EncryptStringToBytes(json)
            File.WriteAllBytes(RuntimeConfigPath, encryptedBytes)
            File.SetAttributes(RuntimeConfigPath, FileAttributes.Hidden)
        Catch ex As Exception
            Debug.WriteLine($"Runtime config save error: {ex.Message}")
            Throw
        End Try
    End Sub

    Private Function EncryptStringToBytes(plainText As String) As Byte()
        Using aes As Aes = Aes.Create()
            Dim entropy As Byte() = Encoding.UTF8.GetBytes(
                $"{Environment.MachineName}-{WindowsIdentity.GetCurrent().User.Value}")

            Dim protectedKey As Byte() = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(SecretKey),
                entropy,
                DataProtectionScope.CurrentUser)

            aes.Key = protectedKey
            aes.GenerateIV()

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
        Using aes As Aes = Aes.Create()
            Dim iv(15) As Byte
            Array.Copy(cipherText, 0, iv, 0, iv.Length)
            aes.IV = iv

            Dim entropy As Byte() = Encoding.UTF8.GetBytes(
                $"{Environment.MachineName}-{WindowsIdentity.GetCurrent().User.Value}")

            Dim protectedKey As Byte() = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(SecretKey),
                entropy,
                DataProtectionScope.CurrentUser)

            aes.Key = protectedKey

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
#End Region
End Module