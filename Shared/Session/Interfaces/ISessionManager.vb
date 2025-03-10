Public Interface ISessionManager
    ReadOnly Property CurrentUser As Users
    Sub Login(user As Users)
    Sub Logout()
    Function IsLoggedIn() As Boolean
    Function HasRole(role As String) As Boolean
    Sub LoadSession()

End Interface
