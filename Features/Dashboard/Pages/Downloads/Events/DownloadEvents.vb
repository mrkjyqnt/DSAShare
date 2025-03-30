Imports Prism.Events

Public Class DownloadProgressEvent
    Inherits PubSubEvent(Of DownloadProgress)
End Class

Public Class DownloadCompletedEvent
    Inherits PubSubEvent(Of DownloadHistoryItem)
End Class

Public Class DownloadStatusUpdatedEvent
    Inherits PubSubEvent(Of Boolean)
End Class