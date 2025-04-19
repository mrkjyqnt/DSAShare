Public Interface ISecurityService
    Sub RecordAttempts()
    Sub ResetAttempts()
    Function SecurityCheck() As Task(Of Boolean)
    Function GetLockUntil() As Date?
End Interface