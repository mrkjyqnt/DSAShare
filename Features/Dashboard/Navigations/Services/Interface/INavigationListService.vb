Public Interface INavigationListService
    Function GetNavigationItems() As List(Of NavigationItemModel)
    Function GetLastNavigationItem() As NavigationItemModel
End Interface
