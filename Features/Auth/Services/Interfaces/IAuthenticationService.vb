Public Interface IAuthenticationService
    Function Authenticate(username As String, password As String) As Boolean
    Function IsUsernameExist(username As String) As Boolean
End Interface