Public Interface INavigationService
    Function GetNavigationItems() As List(Of NavigationItemModel)
    Sub SetNavigation(Title As String)
    Function GetLastNavigationItem() As NavigationItemModel
End Interface
