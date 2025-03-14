Public Class RegistrationService
    Implements IRegistrationService

    Private ReadOnly _usersRepository As UsersRepository

    Private _users As New Users

    Public Sub New(usersRepository As UsersRepository)
        _usersRepository = usersRepository
    End Sub

    Public Function Register(fullname As String, username As String, password As String) As Boolean Implements IRegistrationService.Register
        ' Create a new user
        _users = New Users() With {
            .Name = fullname,
            .Username = username,
            .PasswordHash = HashPassword(password),
            .Role = "Member",
            .Status = "Active",
            .CreatedAt = DateTime.Now
        }

        Return _usersRepository.Insert(_users)
    End Function

    Public Function CheckUsername(username As String) As Boolean Implements IRegistrationService.CheckUsername
        _users = New Users() With {
            .Username = username
        }

        Return _usersRepository.GetByUsername(_users) IsNot Nothing
    End Function
End Class
