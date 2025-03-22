Public Interface INavigationService
    Function GetNavigationItems() As List(Of NavigationItemModel)
    Function GetLastNavigationItem() As NavigationItemModel
    Sub Go(Optional Region As String = "", Optional View As String = "", Optional NavigationItem As String = "")
    Sub GoBack()
    Sub Start(Optional Region As String = "", Optional View As String = "", Optional NavigationItem As String = "")
End Interface
