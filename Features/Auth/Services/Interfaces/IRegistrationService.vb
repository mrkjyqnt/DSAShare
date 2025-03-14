Public Interface IRegistrationService
    Function Register(fullname As String, username As String, password As String) As Boolean
    Function CheckUsername(username As String) As Boolean

End Interface
