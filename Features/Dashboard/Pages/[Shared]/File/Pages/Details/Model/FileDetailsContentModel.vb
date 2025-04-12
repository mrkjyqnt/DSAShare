Public Class FileDetailsContentModel
    Public Property Author As String
    Public Property FileName As String
    Public Property AccessLevel As String
    Public Property DownloadCount As Integer
    Public Property PublishDate As DateTime
    Public Property ExpirationDate As DateTime? = Nothing
    Public Property Availability As String
    Public Property FileType As String
End Class