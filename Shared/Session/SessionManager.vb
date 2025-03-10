Imports System.Text.Json
Imports System.IO

Public Class SessionManager
    Implements ISessionManager

    Private _currentUser As Users
    Private Const SessionFilePath As String = "UserSession.json"

    Public ReadOnly Property CurrentUser As Users Implements ISessionManager.CurrentUser
        Get
            Return _currentUser
        End Get
    End Property

    Public Sub Login(user As Users) Implements ISessionManager.Login
        _currentUser = user
        SaveSession()
    End Sub

    Public Sub Logout() Implements ISessionManager.Logout
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
        Catch ex As Exception
            ' Handle exceptions as needed
        End Try
    End Sub

    Private Sub ClearSession()
        Try
            If File.Exists(SessionFilePath) Then
                File.Delete(SessionFilePath)
            End If
        Catch ex As Exception
            ' Handle exceptions as needed
        End Try
    End Sub

    Public Sub LoadSession() Implements ISessionManager.LoadSession
        Try
            If File.Exists(SessionFilePath) Then
                Dim jsonString As String = File.ReadAllText(SessionFilePath)
                _currentUser = JsonSerializer.Deserialize(Of Users)(jsonString)
            End If
        Catch ex As Exception
            ' Handle exceptions as needed
        End Try
    End Sub
End Class
