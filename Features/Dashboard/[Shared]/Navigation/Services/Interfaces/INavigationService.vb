Imports Prism.Navigation

Public Interface INavigationService
    Sub Start(Optional region As String = "", Optional view As String = "", Optional item As String = "")
    Sub Go(Optional region As String = "", Optional view As String = "", 
             Optional item As String = "", Optional parameters As NavigationParameters = Nothing)
    Sub GoBack()
    Sub GoForward()
End Interface