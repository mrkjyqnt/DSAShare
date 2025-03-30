Public Class DownloadProgress
    Public Property Url As String
    Public Property FilePath As String
    Public Property BytesReceived As Long
    Public Property TotalBytes As Long
    Public Property Percentage As Integer
    Public Property Status As DownloadStatus
End Class

Public Enum DownloadStatus
    Downloading
    Completed
    Failed
    Canceled
    Removed
End Enum