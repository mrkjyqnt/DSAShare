Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Diagnostics
Imports System.Security.Principal

#Disable Warning
Public Class SessionManager
    Implements ISessionManager

#If DEBUG Then
    Private Const DebugMode As Boolean = True
#Else
    Private Const DebugMode As Boolean = False
#End If

    ' File paths
    Private Const SessionFilePath As String = "UserSession.debug.json"
    Private ReadOnly SecureSessionPath As String = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DSAShare",
        "UserSession.secure")

    ' Secret key
    Private ReadOnly SecretKey As String = "DSAShare"

    Private _currentUser As Users

    Sub New()
        Debug.WriteLine($"SessionManager initialized in {(If(DebugMode, "DEBUG", "RELEASE"))} mode")
        Debug.WriteLine($"Secure storage path: {SecureSessionPath}")
        LoadSession()
    End Sub

    Public ReadOnly Property CurrentUser As Users Implements ISessionManager.CurrentUser
        Get
            Debug.WriteLine($"[DEBUG] Retrieving CurrentUser: {_currentUser?.Username}")
            Return _currentUser
        End Get
    End Property

    Public Sub Login(user As Users) Implements ISessionManager.Login
        Debug.WriteLine($"[DEBUG] Logging in user: {user?.Username}")
        _currentUser = user
        SaveSession()
        Debug.WriteLine($"[DEBUG] Session saved for user: {_currentUser?.Username}")
        VerifyFileCreation()
    End Sub

    Public Sub Logout() Implements ISessionManager.Logout
        Debug.WriteLine($"[DEBUG] Logging out user: {_currentUser?.Username}")
        _currentUser = Nothing
        ClearSession()
    End Sub

    Public Function IsLoggedIn() As Boolean Implements ISessionManager.IsLoggedIn
        Return _currentUser IsNot Nothing
    End Function

    Public Function HasRole(role As String) As Boolean Implements ISessionManager.HasRole
        Return IsLoggedIn() AndAlso _currentUser?.Role?.Equals(role, StringComparison.OrdinalIgnoreCase) = True
    End Function

    Private Sub SaveSession()
        Try
            If DebugMode Then
                SaveDebugSession()
            Else
                SaveSecureSession()
            End If
        Catch ex As Exception
            ErrorHandler.SetError($"SaveSession failed: {ex.Message}")
            Debug.WriteLine($"SaveSession error: {ex.ToString()}")
        End Try
    End Sub

    Private Sub ClearSession()
        Try
            Dim targetPath = If(DebugMode, SessionFilePath, SecureSessionPath)
            If File.Exists(targetPath) Then
                File.Delete(targetPath)
                Debug.WriteLine($"Cleared session file: {targetPath}")
            End If
        Catch ex As Exception
            ErrorHandler.SetError($"ClearSession failed: {ex.Message}")
            Debug.WriteLine($"ClearSession error: {ex.ToString()}")
        End Try
    End Sub

    Public Sub LoadSession() Implements ISessionManager.LoadSession
        Try
            If DebugMode Then
                LoadDebugSession()
            Else
                LoadSecureSession()
            End If
        Catch ex As Exception
            ErrorHandler.SetError($"LoadSession failed: {ex.Message}")
            Debug.WriteLine($"LoadSession error: {ex.ToString()}")
        End Try
    End Sub

#Region "Debug Mode Implementation"
    Private Sub SaveDebugSession()
        Try
            Dim options As New JsonSerializerOptions With {
                .WriteIndented = True,
                .PropertyNameCaseInsensitive = True
            }
            Dim jsonString As String = JsonSerializer.Serialize(_currentUser, options)
            File.WriteAllText(SessionFilePath, jsonString)
            Debug.WriteLine($"Debug session saved to: {Path.GetFullPath(SessionFilePath)}")
        Catch ex As Exception
            Throw New ApplicationException("Failed to save debug session", ex)
        End Try
    End Sub

    Private Sub LoadDebugSession()
        Try
            If File.Exists(SessionFilePath) Then
                Dim jsonString As String = File.ReadAllText(SessionFilePath)
                _currentUser = JsonSerializer.Deserialize(Of Users)(jsonString)
                Debug.WriteLine($"Debug session loaded for: {_currentUser?.Username}")
            End If
        Catch ex As Exception
            Throw New ApplicationException("Failed to load debug session", ex)
        End Try
    End Sub
#End Region

#Region "Secure Mode Implementation"
    Private Sub SaveSecureSession()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(SecureSessionPath))
            
            Dim options As New JsonSerializerOptions With {
                .PropertyNameCaseInsensitive = True
            }
            Dim jsonString As String = JsonSerializer.Serialize(_currentUser, options)
            
            Dim encryptedData As Byte() = EncryptStringToBytes(jsonString)
            File.WriteAllBytes(SecureSessionPath, encryptedData)
            File.SetAttributes(SecureSessionPath, FileAttributes.Hidden)
            
            Debug.WriteLine($"Secure session saved to: {SecureSessionPath}")
        Catch ex As Exception
            Throw New ApplicationException("Failed to save secure session", ex)
        End Try
    End Sub

    Private Sub LoadSecureSession()
        Try
            If File.Exists(SecureSessionPath) Then
                Dim encryptedData As Byte() = File.ReadAllBytes(SecureSessionPath)
                If encryptedData.Length > 0 Then
                    Dim jsonString As String = DecryptStringFromBytes(encryptedData)
                    _currentUser = JsonSerializer.Deserialize(Of Users)(jsonString)
                    Debug.WriteLine($"Secure session loaded for: {_currentUser?.Username}")
                End If
            End If
        Catch cryptoEx As CryptographicException
            File.Delete(SecureSessionPath)
            Throw New ApplicationException("Invalid session - please login again", cryptoEx)
        Catch ex As Exception
            Throw New ApplicationException("Failed to load secure session", ex)
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
            Dim targetPath = If(DebugMode, SessionFilePath, SecureSessionPath)
            If File.Exists(targetPath) Then
                Debug.WriteLine($"Session verification: File exists at {targetPath}")
                If Not DebugMode Then
                    Debug.WriteLine($"Hidden attribute: {(File.GetAttributes(targetPath) And FileAttributes.Hidden) = FileAttributes.Hidden}")
                End If
            Else
                Debug.WriteLine($"Session verification: FILE NOT FOUND at {targetPath}")
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
End Class