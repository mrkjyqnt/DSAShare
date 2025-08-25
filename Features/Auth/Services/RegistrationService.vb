Public Class RegistrationService
    Implements IRegistrationService

    Private ReadOnly _usersRepository As UsersRepository

    Private _users As New Users

    Public Sub New(usersRepository As UsersRepository)
        _usersRepository = usersRepository
    End Sub

    Public Function Register(user As Users) As Boolean Implements IRegistrationService.Register
        Return _usersRepository.Insert(user)
    End Function

    Public Function CheckUsername(username As String) As Boolean Implements IRegistrationService.CheckUsername
        _users = New Users() With {
            .Username = username
        }

        Return _usersRepository.GetByUsername(_users) IsNot Nothing
    End Function
End Class
