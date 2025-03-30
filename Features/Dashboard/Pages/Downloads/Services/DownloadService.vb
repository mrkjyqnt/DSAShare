Imports Prism.Events
Imports System.Collections.Concurrent
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.Json
Imports System.Threading

Public Class DownloadService
    Implements IDownloadService

    Private Shared ReadOnly HistoryFilePath As String = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YourAppName",
        "download_history.json")

    Private ReadOnly _activeDownloads As New ConcurrentDictionary(Of Guid, ActiveDownloadInfo)
    Private ReadOnly _history As New ObservableCollection(Of DownloadHistoryItem)
    Private ReadOnly _eventAggregator As IEventAggregator
    Private ReadOnly _mutex As New Object()

    Public Event DownloadProgressChanged As EventHandler(Of DownloadProgress) Implements IDownloadService.DownloadProgressChanged
    Public Event DownloadCompleted As EventHandler(Of DownloadHistoryItem) Implements IDownloadService.DownloadCompleted

    Public Sub New(eventAggregator As IEventAggregator)
        _eventAggregator = eventAggregator
        LoadHistory()
    End Sub

    Public ReadOnly Property DownloadHistory As IEnumerable(Of DownloadHistoryItem) Implements IDownloadService.DownloadHistory
        Get
            Return _history
        End Get
    End Property

    Public ReadOnly Property ActiveDownloads As IEnumerable(Of ActiveDownloadInfo) Implements IDownloadService.ActiveDownloads
        Get
            Return _activeDownloads.Values
        End Get
    End Property

    Public Function StartDownloadAsync(sourcePath As String, Optional destinationPath As String = Nothing) As Task(Of Guid) Implements IDownloadService.StartDownloadAsync
        If String.IsNullOrEmpty(destinationPath) Then
            destinationPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                Path.GetFileName(sourcePath))
        End If

        Dim downloadId = Guid.NewGuid()
        Dim cts = New CancellationTokenSource()
        Dim activeDownload As New ActiveDownloadInfo With {
            .DownloadId = downloadId,
            .Url = sourcePath,
            .DestinationPath = destinationPath,
            .Status = DownloadStatus.Downloading,
            .CancellationTokenSource = cts
        }

        _activeDownloads.TryAdd(downloadId, activeDownload)

        ' Start the transfer in the background
        Task.Run(Async Function()
                     Await TransferFileInternalAsync(sourcePath, destinationPath, downloadId, cts.Token).ConfigureAwait(True)
                 End Function)

        Return Task.FromResult(downloadId)
    End Function

    Public Sub CancelDownload(downloadId As Guid) Implements IDownloadService.CancelDownload
        Dim activeDownload As ActiveDownloadInfo = Nothing
        If _activeDownloads.TryGetValue(downloadId, activeDownload) Then
            activeDownload.CancellationTokenSource.Cancel()
            activeDownload.Status = DownloadStatus.Canceled
            _activeDownloads.TryRemove(downloadId, activeDownload)
        End If
    End Sub

    Private Async Function TransferFileInternalAsync(sourcePath As String, destinationPath As String,
                                              downloadId As Guid, cancellationToken As CancellationToken) As Task
        Dim progress = New DownloadProgress With {
            .Url = sourcePath,
            .FilePath = destinationPath,
            .Status = DownloadStatus.Downloading
        }

        Try
            ' Verify source file exists and get its size
            If Not File.Exists(sourcePath) Then
                Throw New FileNotFoundException("Source file not found", sourcePath)
            End If

            Dim fileInfo = New FileInfo(sourcePath)
            progress.TotalBytes = fileInfo.Length

            ' Create directory structure if needed
            Dim directoryPath = Path.GetDirectoryName(destinationPath)
            If Not Directory.Exists(directoryPath) Then
                Directory.CreateDirectory(directoryPath)
            End If

            ' Use FileOptions to ensure proper file handling
            Dim bufferSize As Integer = 81920 ' 80KB buffer
            Using sourceStream = New FileStream(
                sourcePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize,
                FileOptions.SequentialScan Or FileOptions.Asynchronous)

                Using destinationStream = New FileStream(
                    destinationPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize,
                    FileOptions.Asynchronous Or FileOptions.WriteThrough)

                    Dim buffer As Byte() = New Byte(bufferSize - 1) {}
                    Dim bytesRead As Integer
                    Dim totalBytesRead As Long = 0

                    ' Read until all bytes are read or cancellation is requested
                    While totalBytesRead < progress.TotalBytes AndAlso Not cancellationToken.IsCancellationRequested
                        bytesRead = Await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(True)
                        If bytesRead = 0 Then Exit While ' End of file

                        Await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(True)
                        totalBytesRead += bytesRead

                        ' Update progress
                        progress.BytesReceived = totalBytesRead
                        progress.Percentage = CInt((totalBytesRead * 100) / progress.TotalBytes)

                        ' Raise progress events
                        RaiseEvent DownloadProgressChanged(Me, progress)
                        _eventAggregator.GetEvent(Of DownloadProgressEvent).Publish(progress)
                    End While

                    ' Ensure all data is written to disk
                    destinationStream.Flush(True)
                End Using
            End Using

            ' Verify the copied file
            Dim destFileInfo = New FileInfo(destinationPath)
            If destFileInfo.Length <> fileInfo.Length Then
                Throw New IOException($"File copy incomplete. Expected {fileInfo.Length} bytes, got {destFileInfo.Length}")
            End If

            progress.Status = DownloadStatus.Completed
            AddToHistory(progress)

        Catch ex As OperationCanceledException
            progress.Status = DownloadStatus.Canceled
            ' Clean up partially transferred file
            If File.Exists(destinationPath) Then
                File.Delete(destinationPath)
            End If
            _eventAggregator.GetEvent(Of DownloadProgressEvent).Publish(progress)

        Catch ex As Exception
            progress.Status = DownloadStatus.Failed
            ' Clean up failed transfer
            If File.Exists(destinationPath) Then
                File.Delete(destinationPath)
            End If
            _eventAggregator.GetEvent(Of DownloadProgressEvent).Publish(progress)
            Throw
        Finally
            _activeDownloads.TryRemove(downloadId, Nothing)
        End Try
    End Function
    Private Sub AddToHistory(progress As DownloadProgress)
        SyncLock _mutex
            Dim historyItem = New DownloadHistoryItem With {
                .Url = progress.Url,
                .FilePath = progress.FilePath,
                .FileName = Path.GetFileName(progress.FilePath),
                .DownloadDate = DateTime.Now,
                .Status = progress.Status
            }

            ' Dispatch to UI thread if needed
            Application.Current.Dispatcher.Invoke(Sub()
                                                      _history.Insert(0, historyItem)
                                                  End Sub)

            SaveHistory()
            _eventAggregator.GetEvent(Of DownloadCompletedEvent).Publish(historyItem)
        End SyncLock
    End Sub

    Private Sub LoadHistory()
        If File.Exists(HistoryFilePath) Then
            Try
                Dim json = File.ReadAllText(HistoryFilePath)
                Dim items = JsonSerializer.Deserialize(Of List(Of DownloadHistoryItem))(json)
                
                Application.Current.Dispatcher.Invoke(Sub()
                    _history.Clear()
                    For Each item In items
                        ' Force status update based on current file state
                        If item.Status = DownloadStatus.Completed AndAlso Not File.Exists(item.FilePath) Then
                            item.Status = DownloadStatus.Removed
                        End If
                        item.UpdateFileStatus()
                        _history.Add(item)
                    Next
                End Sub)
            Catch ex As Exception
                Debug.WriteLine($"[DEBUG] Error loading download history: {ex.Message}")
            End Try
        End If
    End Sub

    ' Add this method to refresh file statuses
    Public Sub RefreshFileStatuses() Implements IDownloadService.RefreshFileStatuses
        SyncLock _mutex
            Application.Current.Dispatcher.Invoke(Sub()
                For Each item In _history
                    If item.Status = DownloadStatus.Completed Then
                        If Not File.Exists(item.FilePath) Then
                            item.Status = DownloadStatus.Removed
                        End If
                    ElseIf item.Status = DownloadStatus.Removed Then
                        If File.Exists(item.FilePath) Then
                            item.Status = DownloadStatus.Completed
                        End If
                    End If
                    item.UpdateFileStatus()
                Next
            End Sub)
        End SyncLock
    End Sub

    Private Sub SaveHistory()
        SyncLock _mutex
            Dim directoryPath = Path.GetDirectoryName(HistoryFilePath)
            If Not Directory.Exists(directoryPath) Then
                Directory.CreateDirectory(directoryPath)
            End If

            Dim json = JsonSerializer.Serialize(_history.ToList())
            File.WriteAllText(HistoryFilePath, json)
        End SyncLock
    End Sub

    Public Sub Remove(item As DownloadHistoryItem) Implements IDownloadService.Remove
        SyncLock _mutex
            Application.Current.Dispatcher.Invoke(Sub()
                                                      If item.IsFileExists AndAlso File.Exists(item.FilePath) Then
                                                          File.Delete(item.FilePath)
                                                      End If

                                                      If _history.Contains(item) Then
                                                          _history.Remove(item)
                                                      End If
                                                  End Sub)
            SaveHistory()
        End SyncLock
    End Sub
End Class