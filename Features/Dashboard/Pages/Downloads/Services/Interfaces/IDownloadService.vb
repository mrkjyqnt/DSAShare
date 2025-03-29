Imports System.Threading

Public Interface IDownloadService
    Function StartDownloadAsync(url As String, Optional destinationPath As String = Nothing) As Task(Of Guid)
    Sub CancelDownload(downloadId As Guid)
    Sub Remove(item As DownloadHistoryItem)
    ReadOnly Property DownloadHistory As IEnumerable(Of DownloadHistoryItem)
    ReadOnly Property ActiveDownloads As IEnumerable(Of ActiveDownloadInfo)
    Event DownloadProgressChanged As EventHandler(Of DownloadProgress)
    Event DownloadCompleted As EventHandler(Of DownloadHistoryItem)
End Interface

Public Class ActiveDownloadInfo
    Public Property DownloadId As Guid
    Public Property Url As String
    Public Property DestinationPath As String
    Public Property Status As DownloadStatus
    Public Property CancellationTokenSource As CancellationTokenSource
End Class