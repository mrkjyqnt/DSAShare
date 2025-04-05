Public Interface IUserService
    Function CheckPermission(users As Users) As Boolean
    Function GetUserById(userId As Users) As Users
    Function UpdateUser(user As Users) As Boolean
End Interface
