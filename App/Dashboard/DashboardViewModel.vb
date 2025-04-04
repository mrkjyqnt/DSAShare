Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Class DashboardViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _sessionManager As ISessionManager

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Sub New(regionManager As IRegionManager, navigationService As INavigationService, sessionManager As ISessionManager)
        _regionManager = regionManager
        _navigationService = navigationService
        _sessionManager = sessionManager
        Try
            If _sessionManager.CurrentUser.Role = "Guest" Then
                _regionManager.RegisterViewWithRegion("NavigationRegion", "NavigationView")
                _regionManager.RegisterViewWithRegion("PageRegion", "PublicFilesView")
                _navigationService.Start("PageRegion", "PublicFilesView", "Public Files")
                Return
            End If

            _regionManager.RegisterViewWithRegion("NavigationRegion", "NavigationView")
            _regionManager.RegisterViewWithRegion("PageRegion", "HomeView")
            _navigationService.Start("PageRegion", "HomeView", "Home")
        Catch ex As Exception
            Debug.WriteLine($"[ERROR] Theres an error occured: {ex.Message}")
        End Try
    End Sub
End Class