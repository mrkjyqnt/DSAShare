Imports System.Collections.ObjectModel
Imports System.Windows.Threading

Public Class UploadQueueService
    Implements IUploadQueueService

    Private ReadOnly _dispatcher As Dispatcher = Dispatcher.CurrentDispatcher
    Private ReadOnly _queue As New ObservableCollection(Of FilesShared)()

    Public ReadOnly Property Queue As ObservableCollection(Of FilesShared) _
        Implements IUploadQueueService.Queue
        Get
            Return _queue
        End Get
    End Property

    Public Sub Enqueue(item As FilesShared) Implements IUploadQueueService.Enqueue
        item.Status = "Queued"
        item.Progress = 0
        _dispatcher.Invoke(Sub() _queue.Add(item))
    End Sub
End Class
