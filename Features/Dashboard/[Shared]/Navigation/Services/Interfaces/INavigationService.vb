Imports Prism.Navigation

Public Interface INavigationService
    Sub Go(Optional Region As String = "", Optional View As String = "", Optional NavigationItem As String = "", Optional parameters As NavigationParameters = Nothing)
    Sub GoBack()
    Sub Start(Optional Region As String = "", Optional View As String = "", Optional NavigationItem As String = "")
End Interface
