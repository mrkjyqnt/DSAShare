Public Class FilesBlocked
    Public Property Id As Integer? = Nothing
    Public Property FileHash As String
    Public Property Reason As String
    Public Property BlockedBy As Integer? = Nothing
    Public Property BlockedAt As DateTime? = Nothing
End Class
