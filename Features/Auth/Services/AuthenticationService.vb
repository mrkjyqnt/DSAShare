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
            Return True
        End If

        Return False
    End Function

    Public Function IsUsernameExist(username As String) As Boolean Implements IAuthenticationService.IsUsernameExist
        Dim user = New Users With {
            .Username = username
        }

        If _usersRepository.GetByUsername(user) Is Nothing Then
            Return False
        Else
            Return True
        End If
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