Public Class AuthenticationService
    Implements IAuthenticationService

    Private ReadOnly _usersRepository As UsersRepository
    Private ReadOnly _sessionManager As SessionManager

    Private _user As New Users

    Public Sub New(usersRepository As UsersRepository, sessionManager As SessionManager)
        _usersRepository = usersRepository
        _sessionManager = sessionManager
    End Sub

    ''' <summary>
    ''' Authenticates a user and logs them in.
    ''' </summary>
    ''' <param name="username">The username.</param>
    ''' <param name="password">The password.</param>
    ''' <returns>True if authentication is successful; otherwise, False.</returns>
    Public Function Authenticate(username As String, password As String) As Boolean Implements IAuthenticationService.Authenticate
        
        _user = New Users With {
            .Username = username,
            .PasswordHash = password
        }
        ' Authenticate the user
        Dim user As Users = _usersRepository.GetByUsername(_user)

        If user IsNot Nothing Then

            If user.PasswordHash Is _user.PasswordHash then
                ' Log the user in
                _sessionManager.Login(user)
                Return True
            End If

            Return False
        Else
            ' Authentication failed
            Return False
        End If
    End Function

    ''' <summary>
    ''' Logs out the current user.
    ''' </summary>
    Public Sub Logout() Implements IAuthenticationService.Logout
        _sessionManager.Logout()
    End Sub

    ''' <summary>
    ''' Checks if a user is logged in.
    ''' </summary>
    ''' <returns>True if a user is logged in; otherwise, False.</returns>
    Public Function IsAuthenticated() As Boolean Implements IAuthenticationService.IsAuthenticated
        Return _sessionManager.IsLoggedIn()
    End Function
End Class