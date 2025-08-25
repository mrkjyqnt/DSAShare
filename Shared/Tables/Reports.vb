''' <summary>
''' Represents the Reports table.
''' </summary>
Public Class Reports
    Public Property Id As Integer? = 0
    Public Property FileId As Integer? = 0
    Public Property ReporterId As Integer? = 0
    Public Property ReporterDescription As String
    Public Property AdminId As Integer? = Nothing
    Public Property AdminDescription As String
    Public Property Status As String
    Public Property ReportedAt As DateTime? = Nothing
End Class
