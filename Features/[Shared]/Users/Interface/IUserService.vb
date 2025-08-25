Public Interface IUserService
    Function CheckPermission(users As Users) As Boolean
    Function GetUserById(userId As Users) As Users
    Function UpdateUser(user As Users) As Boolean
    Function DeleteUser(user As Users) As Boolean
    Function GetAllUsers() As List(Of Users)
    Function CheckStatus() As Boolean
    Function GetUserByUsername(user As Users) As Users
    Function CheckSecurityAnswers(user As Users) As Boolean
End Interface
