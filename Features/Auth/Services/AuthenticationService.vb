Public Class AuthenticationService
    Implements IAuthenticationService

    Private ReadOnly _usersRepository As UsersRepository
    Private ReadOnly _sessionManager As ISessionManager

    Public Sub New(usersRepository As UsersRepository, sessionManager As ISessionManager)
        _usersRepository = usersRepository
        _sessionManager = sessionManager
    End Sub

    Public Function Authenticate(username As String, password As String) As Boolean Implements IAuthenticationService.Authenticate
        Dim user = New Users With {
            .Username = username,
            .PasswordHash = HashPassword(password)
        }

        If _usersRepository.Auth(user) Then
            Dim loggedUser = _usersRepository.GetByUsername(user)
            SetSession(loggedUser)
            Return True
        End If

        Return False
    End Function

    Private Async Sub SetSession(user As Users)
        Try
            If user IsNot Nothing Then
                _sessionManager.Login(user)
                Await Task.Delay(1000).ConfigureAwait(True)
                _sessionManager.LoadSession()
            Else
                Throw New ArgumentNullException(NameOf(user), "User cannot be null.")
            End If
        Catch ex As Exception

        End Try
    End Sub
End Class