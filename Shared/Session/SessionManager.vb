Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Diagnostics
Imports System.Security.Principal

Public Class SessionManager
    Implements ISessionManager

#If DEBUG Then
    Private Const DebugMode As Boolean = True
#Else
    Private Const DebugMode As Boolean = False
#End If

    ' File paths (unchanged from your original)
    Private Const SessionFilePath As String = "UserSession.debug.json"
    Private ReadOnly SecureSessionPath As String = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DSAShare",
        "UserSession.secure")

    ' Use the same secret key as ConfigurationModule
    Private ReadOnly SecretKey As String = "DSAShare"

    Private _currentUser As Users

    ' All your original public methods remain exactly the same
    Sub New()
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
        Return IsLoggedIn() AndAlso _currentUser.Role.Equals(role, StringComparison.OrdinalIgnoreCase)
    End Function

    ' Private methods with identical encryption to ConfigurationModule
    Private Sub SaveSession()
        If DebugMode Then
            SaveDebugSession()
        Else
            SaveSecureSession()
        End If
    End Sub

    Private Sub ClearSession()
        Try
            If DebugMode Then
                If File.Exists(SessionFilePath) Then
                    File.Delete(SessionFilePath)
                End If
            Else
                If File.Exists(SecureSessionPath) Then
                    File.Delete(SecureSessionPath)
                End If
            End If
        Catch ex As Exception
            ErrorHandler.SetError(ex.Message)
        End Try
    End Sub

    Public Sub LoadSession() Implements ISessionManager.LoadSession
        If DebugMode Then
            LoadDebugSession()
        Else
            LoadSecureSession()
        End If
    End Sub

#Region "Debug Mode Implementation"
    Private Sub SaveDebugSession()
        Try
            Dim jsonString As String = JsonSerializer.Serialize(_currentUser)
            File.WriteAllText(SessionFilePath, jsonString)
        Catch ex As Exception
            ErrorHandler.SetError(ex.Message)
        End Try
    End Sub

    Private Sub LoadDebugSession()
        Try
            If File.Exists(SessionFilePath) Then
                Dim jsonString As String = File.ReadAllText(SessionFilePath)
                _currentUser = JsonSerializer.Deserialize(Of Users)(jsonString)
            End If
        Catch ex As Exception
            ErrorHandler.SetError(ex.Message)
        End Try
    End Sub
#End Region

#Region "Secure Mode Implementation - Matches ConfigurationModule"
    Private Sub SaveSecureSession()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(SecureSessionPath))
            Dim jsonString As String = JsonSerializer.Serialize(_currentUser)
            Dim encryptedData As Byte() = EncryptStringToBytes(jsonString)
            File.WriteAllBytes(SecureSessionPath, encryptedData)
            File.SetAttributes(SecureSessionPath, FileAttributes.Hidden)
        Catch ex As Exception
            ErrorHandler.SetError(ex.Message)
        End Try
    End Sub

    Private Sub LoadSecureSession()
        Try
            If File.Exists(SecureSessionPath) Then
                Dim encryptedData As Byte() = File.ReadAllBytes(SecureSessionPath)
                Dim jsonString As String = DecryptStringFromBytes(encryptedData)
                _currentUser = JsonSerializer.Deserialize(Of Users)(jsonString)
            End If
        Catch ex As Exception
            ErrorHandler.SetError(ex.Message)
        End Try
    End Sub

    ' Identical encryption methods to ConfigurationModule
    Private Function EncryptStringToBytes(plainText As String) As Byte()
        Using aes As Aes = Aes.Create()
            ' Use DPAPI with your SecretKey
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
End Class