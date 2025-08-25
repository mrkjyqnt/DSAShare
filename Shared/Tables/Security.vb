''' <summary>
''' Represents the Security table.
''' </summary>
Public Class Security
    Public Property Id As Integer? = Nothing
    Public Property UserId As Integer
    Public Property MacAddress As String
    Public Property AttemptCount As Integer? 
    Public Property LockUntil As DateTime?
End Class