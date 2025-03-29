Public Class UserService
    Implements IUserService

    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _userRepository As UsersRepository

    Public Sub New(sessionManager As ISessionManager, userRepository As UsersRepository)
        _sessionManager = sessionManager
        _userRepository = userRepository
    End Sub

    Public Function CheckPermission(users As Users) As Boolean Implements IUserService.CheckPermission
        Try
            If _sessionManager.CurrentUser.Role = "Guest" Then
                Return False
            End If

            If _sessionManager.CurrentUser.Role = "Member" OrElse _sessionManager.CurrentUser.Role = "Admin" Then
                Dim user = _userRepository.GetByUsername(_sessionManager.CurrentUser)

                If user IsNot Nothing Then

                    If users.PasswordHash = user.PasswordHash Then
                        Return True
                    End If

                    Return False
                End If

                Return False
            End If

            Return False
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Theres an error checking the user permission")
            Debug.WriteLine($"[DEBUG] Message: {ex.Message}")
            Return False
        End Try
    End Function

End Class
