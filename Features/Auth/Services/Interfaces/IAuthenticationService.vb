Public Interface IAuthenticationService
    Function Authenticate(username As String, password As String) As Boolean
    Sub Logout()
    Function IsAuthenticated() As Boolean
End Interface