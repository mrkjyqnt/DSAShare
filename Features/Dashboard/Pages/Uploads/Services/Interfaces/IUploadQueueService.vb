Imports System.Collections.ObjectModel

Public Interface IUploadQueueService
    ReadOnly Property Queue As ObservableCollection(Of FilesShared)
    Sub Enqueue(item As FilesShared)
End Interface
