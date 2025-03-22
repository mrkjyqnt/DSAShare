Public Interface IAuthenticationService
    Sub SetSession(user As Users)
    Function Authenticate(username As String, password As String) As Boolean
End Interface