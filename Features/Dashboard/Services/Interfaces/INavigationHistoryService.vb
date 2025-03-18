Public Interface INavigationHistoryService
    Sub PushPage(uri As String)
    Function PopPage() As String
    Function PeekPage() As String
    ReadOnly Property CanGoBack As Boolean
End Interface