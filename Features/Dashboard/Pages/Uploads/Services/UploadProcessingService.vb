Imports System.Collections.Specialized
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Threading

#Disable Warning
Public Class UploadProcessingService
    Implements IUploadProcessingService

    Private ReadOnly _queueService As IUploadQueueService
    Private ReadOnly _fileService As IFileService
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _dispatcher As Dispatcher = Dispatcher.CurrentDispatcher

    Private _cts As CancellationTokenSource
    Private _isProcessing As Boolean = False

    Public Sub New(queueService As IUploadQueueService,
                   fileService As IFileService,
                   fileDataService As IFileDataService,
                   activityService As IActivityService)
        _queueService = queueService
        _fileService = fileService
        _fileDataService = fileDataService
        _activityService = activityService

        ' Watch for changes in the queue to start/stop processing
        AddHandler _queueService.Queue.CollectionChanged, AddressOf OnQueueChanged
    End Sub

    ''' <summary>
    ''' Interface method—no-op. Processing is triggered automatically
    ''' by OnQueueChanged when items are queued/removed.
    ''' </summary>
    Public Sub Start(cancellationToken As CancellationToken) _
        Implements IUploadProcessingService.Start
        ' No-op
    End Sub

    ''' <summary>
    ''' Fired whenever an item is added to or removed from the queue.
    ''' Starts the loop on first enqueue; cancels when the queue empties.
    ''' </summary>
    Private Sub OnQueueChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
        If _queueService.Queue.Count > 0 AndAlso Not _isProcessing Then
            ' Start processing
            _cts = New CancellationTokenSource()
            StartProcessingLoop(_cts.Token)
            _isProcessing = True
        ElseIf _queueService.Queue.Count = 0 AndAlso _isProcessing Then
            ' Stop processing
            _cts.Cancel()
            _isProcessing = False
        End If
    End Sub

    ''' <summary>
    ''' The core upload loop. Picks one "Queued" item at a time,
    ''' calls UploadFile, updates Status/Progress, logs activity,
    ''' and shows a popup on success.
    ''' </summary>
    Private Sub StartProcessingLoop(token As CancellationToken)
        Task.Run(Async Function()
                     Try
                         While Not token.IsCancellationRequested
                             ' Find the next queued file
                             Dim item = Await _dispatcher.InvokeAsync(Function()
                                                                          Return _queueService.Queue.
                                                                              FirstOrDefault(Function(f) f.Status = "Queued")
                                                                      End Function)

                             If item Is Nothing Then
                                 ' No work—wait a bit
                                 Try
                                     Await Task.Delay(500, token)
                                 Catch ex As TaskCanceledException
                                     ' Expected when processing is stopped
                                     Exit While
                                 End Try
                                 Continue While
                             End If

                             ' Update status to Uploading before starting
                             Await _dispatcher.InvokeAsync(Sub()
                                                               item.Status = "Uploading"
                                                               item.Progress = 0
                                                           End Sub)

                             Dim lastUpdateTime As DateTime = DateTime.MinValue
                             Dim progressReporter = New Progress(Of Integer)(Sub(p)
                                                                                 ' Throttle updates to max 30fps (about every 33ms)
                                                                                 If (DateTime.Now - lastUpdateTime).TotalMilliseconds > 33 Then
                                                                                     _dispatcher.Invoke(Sub()
                                                                                                            item.Progress = p
                                                                                                            If p < 100 Then
                                                                                                                item.Status = $"Uploading: {p}%"
                                                                                                            End If
                                                                                                        End Sub)
                                                                                     lastUpdateTime = DateTime.Now
                                                                                 End If
                                                                             End Sub)

                             Try
                                 ' Perform the upload
                                 Dim result = Await Task.Run(Function()
                                                                 Return _fileService.UploadFile(item, progressReporter)
                                                             End Function, token)

                                 Await Task.Delay(1000)

                                 ' Process the result
                                 Await _dispatcher.InvokeAsync(Sub()
                                                                   If result.Success Then
                                                                       item.Status = "Completed"
                                                                       item.Progress = 100

                                                                       ' Save to DB & log activity
                                                                       Dim sharedInfo = _fileDataService.GetSharedFileInfo(item)
                                                                       If sharedInfo IsNot Nothing Then
                                                                           item.Id = sharedInfo.Id
                                                                       End If

                                                                       Dim activity = New Activities With {
                                                                           .Action = "Shared a file",
                                                                           .ActionIn = "Shared Files",
                                                                           .ActionAt = Date.Now,
                                                                           .FileId = item.Id.GetValueOrDefault(),
                                                                           .Name = $"{item.FileName}{item.FileType}",
                                                                           .UserId = item.UploadedBy.GetValueOrDefault()
                                                                       }

                                                                       _activityService.AddActivity(activity)

                                                                       PopUp.Information("Upload Complete", $"{item.FileName} has been uploaded.").ConfigureAwait(True)

                                                                       ' Remove completed item from queue
                                                                       _queueService.Queue.Remove(item)
                                                                   Else
                                                                       item.Status = "Failed: " & result.Message
                                                                   End If
                                                               End Sub)

                             Catch ex As OperationCanceledException
                                 ' Expected during cancellation
                                 _dispatcher.InvokeAsync(Sub()
                                                             item.Status = "Cancelled"
                                                         End Sub)
                                 Exit While
                             Catch ex As Exception
                                 _dispatcher.InvokeAsync(Sub()
                                                             item.Status = "Error: " & ex.Message
                                                         End Sub)
                             End Try

                             ' Brief pause before checking the queue again
                             Try
                                 Await Task.Delay(200, token)
                             Catch ex As TaskCanceledException
                                 Exit While
                             End Try
                         End While
                     Finally
                         ' Ensure processing flag is reset
                         _dispatcher.Invoke(Sub() _isProcessing = False)
                     End Try
                 End Function, token)
    End Sub
End Class
