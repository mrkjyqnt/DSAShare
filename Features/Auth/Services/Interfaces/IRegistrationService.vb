Public Interface IRegistrationService
    Function Register(user As Users) As Boolean
    Function CheckUsername(username As String) As Boolean

End Interface
