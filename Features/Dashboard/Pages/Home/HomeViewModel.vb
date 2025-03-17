Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Class HomeViewModel
    Inherits BindableBase

    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _loadingService As ILoadingService
    Private ReadOnly _popUpService As IPopupService

    Public Sub New()

    End Sub
End Class
