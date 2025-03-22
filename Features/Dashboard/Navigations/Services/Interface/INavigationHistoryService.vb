Public Interface INavigationHistoryService
    Function PopPage() As (Region As String, View As String, Item As String)
    Function PeekPage() As (Region As String, View As String, Item As String)
    Sub PushPage(Region As String, View As String, Item As String)
    Sub RemoveCurrentPage()
    Sub Reset()
    ReadOnly Property CanGoBack As Boolean
End Interface