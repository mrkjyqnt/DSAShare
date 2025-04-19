Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Linq

#Disable Warning
Module ConfigurationModule
    ' File paths
    Private ReadOnly ConfigPath As String = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DSAShare",
        "AppConfig.secure")

    ' Secret key
    Private ReadOnly SecretKey As String = "DSAShare"

    ' Default configuration
    Private DefaultConfig As AppSettings = Nothing

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

    ' Initialize default config with dynamic IP
    Private Sub InitializeDefaultConfig()
        If DefaultConfig IsNot Nothing Then Return

        Dim ipBase As String = GetLocalIpBase()
        If String.IsNullOrEmpty(ipBase) Then
            ipBase = "192.168.8"
        End If

        DefaultConfig = New AppSettings With {
            .Database = New DatabaseSettings With {
                .Server = $"{ipBase}.10\SQLEXPRESS",
                .Name = "dsa_share_database",
                .Username = "member",
                .Password = "member"
            },
            .Network = New NetworkSettings With {
                .FolderPath = $"\\{ipBase}.10\ServerStorage",
                .Username = "user",
                .Password = "user"
            }
        }
    End Sub

    ' Gets the first 3 octets of the active IPv4 address
    Private Function GetLocalIpBase() As String
        Try
            Dim interfaces = NetworkInterface.GetAllNetworkInterfaces().
                Where(Function(ni) ni.OperationalStatus = OperationalStatus.Up AndAlso
                                  ni.NetworkInterfaceType <> NetworkInterfaceType.Loopback)

            For Each ni In interfaces
                Dim ipProps = ni.GetIPProperties()
                If ipProps.GatewayAddresses.Count = 0 Then Continue For

                Dim ipv4Addresses = ipProps.UnicastAddresses.
                    Where(Function(addr) addr.Address.AddressFamily = AddressFamily.InterNetwork)

                For Each addr In ipv4Addresses
                    Dim parts = addr.Address.ToString().Split("."c)
                    If parts.Length = 4 Then
                        Return $"{parts(0)}.{parts(1)}.{parts(2)}"
                    End If
                Next
            Next

            Dim host = Dns.GetHostEntry(Dns.GetHostName())
            For Each ip In host.AddressList
                If ip.AddressFamily = AddressFamily.InterNetwork Then
                    Dim parts = ip.ToString().Split("."c)
                    If parts.Length = 4 Then
                        Return $"{parts(0)}.{parts(1)}.{parts(2)}"
                    End If
                End If
            Next

            Return "192.168.8"
        Catch
            Return "192.168.8"
        End Try
    End Function

    ' Public interface
    Public Function GetSettings() As AppSettings
        Try
            InitializeDefaultConfig()
            Dim currentIpBase = GetLocalIpBase()

            ' Load existing config (or use defaults if file doesn't exist)
            Dim settings = LoadConfig()

            ' If no config file exists, use defaults and save them
            If settings Is Nothing OrElse Not File.Exists(ConfigPath) Then
                settings = DefaultConfig
                SaveConfig(settings) ' Save defaults immediately
            End If

            Dim newDbServer = $"{currentIpBase}.10\SQLEXPRESS"
            Dim newFolderPath = $"\\{currentIpBase}.10\ServerStorage"

            ' Check if IP needs updating
            If settings.Database.Server <> newDbServer OrElse
               settings.Network.FolderPath <> newFolderPath Then

                ' Update IP in settings
                settings.Database.Server = newDbServer
                settings.Network.FolderPath = newFolderPath

                ' Save the updated config
                SaveConfig(settings)
            End If

            Debug.WriteLine("=== Updated Config ===")
            Debug.WriteLine($"Database Server: {settings.Database.Server}")
            Debug.WriteLine($"Database Name: {settings.Database.Name}")
            Debug.WriteLine($"Network FolderPath: {settings.Network.FolderPath}")
            Debug.WriteLine("=======================")

            Return settings
        Catch ex As Exception
            Debug.WriteLine($"Error in GetSettings: {ex.Message}")
            Return DefaultConfig
        End Try
    End Function


    Public Sub SaveSettings(settings As AppSettings)
        Try
            InitializeDefaultConfig()
            SaveConfig(settings)
            VerifyConfigFileExistence()
        Catch ex As Exception
            Throw New ApplicationException("Failed to save configuration", ex)
        End Try
    End Sub

    Public Sub ResetToDefaults()
        Try
            InitializeDefaultConfig()
            SaveConfig(DefaultConfig)
        Catch ex As Exception
            Throw New ApplicationException("Failed to reset configuration", ex)
        End Try
    End Sub

#Region "Config File Operations"
    Private Function LoadConfig() As AppSettings
        Try
            Dim configDir = Path.GetDirectoryName(ConfigPath)
            If Not Directory.Exists(configDir) Then
                Directory.CreateDirectory(configDir)
            End If

            If File.Exists(ConfigPath) Then
                Dim encryptedBytes = File.ReadAllBytes(ConfigPath)
                If encryptedBytes.Length > 0 Then
                    Dim json = DecryptStringFromBytes(encryptedBytes)
                    Return JsonSerializer.Deserialize(Of AppSettings)(json, JsonOptions)
                End If
            End If
            Return DefaultConfig
        Catch cryptoEx As CryptographicException
            File.Delete(ConfigPath)
            Return DefaultConfig
        Catch
            Return DefaultConfig
        End Try
    End Function

    Private Sub SaveConfig(config As AppSettings)
        Try
            Dim configDir = Path.GetDirectoryName(ConfigPath)
            If Not Directory.Exists(configDir) Then
                Directory.CreateDirectory(configDir)
            End If

            ' Test directory permissions
            TestDirectoryPermissions(configDir)

            ' Log current config before saving
            Dim debugJson = JsonSerializer.Serialize(config, JsonOptions)
            Debug.WriteLine("Saving the following configuration:")
            Debug.WriteLine(debugJson)

            Dim encryptedBytes = EncryptStringToBytes(debugJson)

            ' Atomic write operation
            Dim tempFile = Path.Combine(configDir, Path.GetRandomFileName())
            File.WriteAllBytes(tempFile, encryptedBytes)

            If Not File.Exists(tempFile) Then
                Throw New IOException("Failed to create temporary config file")
            End If

            If File.Exists(ConfigPath) Then
                File.Delete(ConfigPath)
            End If
            File.Move(tempFile, ConfigPath)

            If Not File.Exists(ConfigPath) Then
                Throw New IOException("Final config file not created after move")
            End If

            File.SetAttributes(ConfigPath, FileAttributes.Hidden)
        Catch ex As Exception
            Throw
        End Try
    End Sub


    Private Sub TestDirectoryPermissions(dir As String)
        Dim testFile = Path.Combine(dir, "perm_test.tmp")
        Try
            File.WriteAllText(testFile, "test")
            If Not File.Exists(testFile) Then
                Throw New IOException("Test file not created")
            End If
            File.Delete(testFile)
        Catch ex As Exception
            Throw New IOException($"Permission test failed in {dir}", ex)
        End Try
    End Sub

    Public Sub VerifyConfigFileExistence()
        Try
            If Not File.Exists(ConfigPath) Then
                Throw New FileNotFoundException($"Config file missing at: {ConfigPath}")
            End If

            Dim fileInfo As New FileInfo(ConfigPath)
            Dim attrs = fileInfo.Attributes
            Dim isHidden = (attrs And FileAttributes.Hidden) = FileAttributes.Hidden

            Debug.WriteLine($"Config file verification:")
            Debug.WriteLine($"- Path: {ConfigPath}")
            Debug.WriteLine($"- Size: {fileInfo.Length} bytes")
            Debug.WriteLine($"- Hidden: {isHidden}")
            Debug.WriteLine($"- Last Write: {fileInfo.LastWriteTime}")

            Dim bytes = File.ReadAllBytes(ConfigPath)
            Debug.WriteLine($"- Read {bytes.Length} bytes successfully")

        Catch ex As Exception
            Debug.WriteLine($"VERIFICATION FAILED: {ex.Message}")
            Throw
        End Try

        Debug.WriteLine("=== Default Config Initialized ===")
        Debug.WriteLine("Database Server: " & DefaultConfig.Database.Server)
        Debug.WriteLine("Database Name: " & DefaultConfig.Database.Name)
        Debug.WriteLine("Database Username: " & DefaultConfig.Database.Username)
        Debug.WriteLine("Network FolderPath: " & DefaultConfig.Network.FolderPath)
        Debug.WriteLine("===================================")
    End Sub
#End Region

#Region "Encryption"
    Private Function EncryptStringToBytes(plainText As String) As Byte()
        Using aes As Aes = Aes.Create()
            Dim keyMaterial = $"{SecretKey}-{Environment.MachineName}-{Environment.UserName}"
            Using rfc = New Rfc2898DeriveBytes(keyMaterial, Encoding.UTF8.GetBytes("DSAShareSalt"), 10000)
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
            Using rfc = New Rfc2898DeriveBytes(keyMaterial, Encoding.UTF8.GetBytes("DSAShareSalt"), 10000)
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
#End Region

    Public Sub CheckAppDataFolder()
        Dim dir = Path.GetDirectoryName(ConfigPath)
        Debug.WriteLine($"Checking folder: {dir}")

        If Not Directory.Exists(dir) Then
            Debug.WriteLine("Folder does not exist!")
            Return
        End If

        Debug.WriteLine("Folder contents:")
        For Each file In Directory.GetFiles(dir)
            Dim fileInfo As New FileInfo(file)
            Debug.WriteLine($"- {fileInfo.Name} (Size: {fileInfo.Length} bytes, Hidden: {(fileInfo.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden})")
        Next
    End Sub
End Module