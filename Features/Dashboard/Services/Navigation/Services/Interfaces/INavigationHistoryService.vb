Imports Prism.Navigation

Public Interface INavigationHistoryService
    Sub PushPage(region As String, view As String, item As String, parameters As NavigationParameters)
    Function GoBack() As NavigationRecord
    Function GoForward() As NavigationRecord
    ReadOnly Property CanGoBack As Boolean
    ReadOnly Property CanGoForward As Boolean
    Sub Reset()
End Interface