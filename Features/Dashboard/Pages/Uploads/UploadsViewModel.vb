Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports System.Collections.ObjectModel
Imports System.Windows.Threading

Public Class UploadsViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _queueService As IUploadQueueService
    Private ReadOnly _dispatcher As Dispatcher = Dispatcher.CurrentDispatcher

    Private _uploadQueue As ObservableCollection(Of FilesShared)
    Public Property UploadQueue As ObservableCollection(Of FilesShared)
        Get
            Return _uploadQueue
        End Get
        Set(value As ObservableCollection(Of FilesShared))
            SetProperty(_uploadQueue, value)
            RaisePropertyChanged(NameOf(UploadQueue))
        End Set
    End Property

    Public ReadOnly Property RemoveCompletedCommand As DelegateCommand
    Public ReadOnly Property RemoveAllCommand As DelegateCommand
    Public ReadOnly Property RetryFailedCommand As DelegateCommand
    Public ReadOnly Property CancelCommand As DelegateCommand

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Sub New(queueService As IUploadQueueService)
        _queueService = queueService
        _uploadQueue = queueService.Queue

        RemoveCompletedCommand = New DelegateCommand(AddressOf RemoveCompleted)
        RemoveAllCommand = New DelegateCommand(AddressOf RemoveAll)
        RetryFailedCommand = New DelegateCommand(AddressOf RetryFailed)
        CancelCommand = New DelegateCommand(AddressOf CancelUpload)
    End Sub

    Private Sub CancelUpload()
        _dispatcher.Invoke(Sub()
                               ' Remove the items with status "Queued" or "InProgress"
                               For i = UploadQueue.Count - 1 To 0 Step -1
                                   Dim item = UploadQueue(i)
                                   If item.Status <> "Completed" AndAlso item.Status <> "Cancelled" Then
                                       ' Set status to "Cancelled" and remove from queue
                                       item.Status = "Cancelled"
                                       UploadQueue.RemoveAt(i)
                                   End If
                               Next
                           End Sub)

        RaisePropertyChanged(NameOf(UploadQueue))
    End Sub

    Private Sub RemoveCompleted()
        _dispatcher.Invoke(Sub()
                               For i = UploadQueue.Count - 1 To 0 Step -1
                                   Dim item = UploadQueue(i)
                                   If item.Status = "Completed" OrElse
                                   item.Status.StartsWith("Failed") OrElse
                                   item.Status = "Cancelled" Then
                                       UploadQueue.RemoveAt(i)
                                   End If
                               Next
                           End Sub)

        RaisePropertyChanged(NameOf(UploadQueue))
    End Sub
    Private Sub RemoveAll()
        _dispatcher.Invoke(Sub()
                               UploadQueue.Clear()
                           End Sub)

        RaisePropertyChanged(NameOf(UploadQueue))
    End Sub
    Private Sub RetryFailed()
        _dispatcher.Invoke(Sub()
                               For Each item In UploadQueue.Where(Function(f) f.Status.StartsWith("Failed") OrElse
                   f.Status.StartsWith("Error") OrElse
                   f.Status = "Cancelled").ToList()
                                   item.Status = "Queued"
                                   item.Progress = 0
                               Next
                           End Sub)

        RaisePropertyChanged(NameOf(UploadQueue))
    End Sub
End Class