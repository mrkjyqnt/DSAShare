Imports System.Text.Json
Imports System.IO
Imports System.Diagnostics

Public Class SessionManager
    Implements ISessionManager

    Private _currentUser As Users
    Private Const SessionFilePath As String = "UserSession.json"

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

    Private Sub SaveSession()
        Try
            Dim jsonString As String = JsonSerializer.Serialize(_currentUser)
            File.WriteAllText(SessionFilePath, jsonString)
            Debug.WriteLine($"[DEBUG] Session saved to file: {SessionFilePath}")
        Catch ex As Exception
            ErrorHandler.SetError(ex.Message)
        End Try
    End Sub

    Private Sub ClearSession()
        Try
            If File.Exists(SessionFilePath) Then
                File.Delete(SessionFilePath)
                Debug.WriteLine($"[DEBUG] Session file deleted: {SessionFilePath}")
            End If
        Catch ex As Exception
            ErrorHandler.SetError(ex.Message)
        End Try
    End Sub

    Public Sub LoadSession() Implements ISessionManager.LoadSession
        Try
            If File.Exists(SessionFilePath) Then
                Dim jsonString As String = File.ReadAllText(SessionFilePath)
                _currentUser = JsonSerializer.Deserialize(Of Users)(jsonString)
                Debug.WriteLine($"[DEBUG] Session loaded for user: {_currentUser?.Username}")
            End If
        Catch ex As Exception
            ErrorHandler.SetError(ex.Message)
        End Try
    End Sub
End Class