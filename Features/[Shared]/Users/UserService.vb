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

            If _sessionManager.CurrentUser.Role = "Admin" Then
                Return True
            End If

            If _sessionManager.CurrentUser.Role = "Member" Then
                Dim user = _userRepository.GetByUsername(_sessionManager.CurrentUser)

                If user IsNot Nothing AndAlso
                   users.PasswordHash IsNot Nothing AndAlso
                   user.PasswordHash IsNot Nothing AndAlso
                   users.PasswordHash = user.PasswordHash Then
                    Return True
                End If
            End If

            Return False
        Catch ex As Exception
            Debug.WriteLine($"[UserService] Error checking permission: {ex.Message}")
            Return False
        End Try
    End Function

    Public Function CheckStatus() As Boolean Implements IUserService.CheckStatus
        Try
            If _sessionManager.CurrentUser.Role = "Guest" Then
                Return True
            End If
            
            Dim user = _userRepository.GetByUsername(_sessionManager.CurrentUser)
            If user IsNot Nothing Then
                If user.Status = "Active" Then
                    Return True
                ElseIf user.Status = "Banned" Then
                    Debug.WriteLine($"[UserService] User is banned: {user.Username}")
                    Return False
                End If
            End If
            Return False
        Catch ex As Exception
            Debug.WriteLine($"[UserService] Error checking status: {ex.Message}")
            Return False
        End Try
    End Function

    Public Function GetAllUsers() As List(Of Users) Implements IUserService.GetAllUsers
        Try
            Dim users = _userRepository.Read()
            If users IsNot Nothing Then
                Return users.Where(Function(u) u.Role <> "Admin").ToList()
            End If
            Return New List(Of Users)()
        Catch ex As Exception
            Debug.WriteLine($"[UserService] Error getting all users: {ex.Message}")
            Return New List(Of Users)()
        End Try
    End Function

    Public Function GetUserById(userId As Users) As Users Implements IUserService.GetUserById
        Try
            Dim user = _userRepository.GetById(userId)
            If user IsNot Nothing Then
                Return user
            End If
            Return Nothing
        Catch ex As Exception
            Debug.WriteLine($"[UserService] Error getting user by ID: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Function UpdateUser(user As Users) As Boolean Implements IUserService.UpdateUser
        Try
            If user.Id Is Nothing Then
                Debug.WriteLine($"[UserService] UpdateUser Error: User is nothing")
                Return False
            End If

            Return _userRepository.Update(user)
        Catch ex As Exception
            Debug.WriteLine($"[UserService] Error updating user: {ex.Message}")
            Return False
        End Try
    End Function

    Public Function DeleteUser(user As Users) As Boolean Implements IUserService.DeleteUser
        Try
            If user.Id Is Nothing Then
                Debug.WriteLine($"[UserService] DeleteUser Error: User is nothing")
                Return False
            End If

            Return _userRepository.Delete(user)
        Catch ex As Exception
            Debug.WriteLine($"[UserService] DeleteUser Error deleting user: {ex.Message}")
            Return False
        End Try
    End Function

End Class
