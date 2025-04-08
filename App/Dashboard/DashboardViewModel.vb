Imports Prism.Mvvm
Imports Prism.Navigation.Regions

#Disable Warning
Public Class DashboardViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime
    Implements INavigationAware

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

        Load()
    End Sub

    Private Async Sub Load()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())


            If _sessionManager.CurrentUser.Role = "Guest" Then

                _regionManager.RegisterViewWithRegion("NavigationRegion", GetType(NavigationView))
                _regionManager.RegisterViewWithRegion("PageRegion", GetType(PublicFilesView))
                _navigationService.Start("PageRegion", "PublicFilesView", "Public Files")
                Return
            End If

            _regionManager.RegisterViewWithRegion("NavigationRegion", GetType(NavigationView))
            _regionManager.RegisterViewWithRegion("PageRegion", GetType(HomeView))
            _navigationService.Start("PageRegion", "HomeView", "Home")
        Catch ex As Exception
            Debug.WriteLine($"[ERROR] Theres an error occured: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Public Sub OnNavigatedTo(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedTo
        Throw New NotImplementedException()
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Return True
    End Function

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedFrom
        Try
            Dim region = _regionManager.Regions("MainRegion")
                Dim view = region.Views.FirstOrDefault(Function(v) v.GetType().Name = "DashboardRegion")
                If view IsNot Nothing Then
                    region.Remove(view)
                End If
        Catch ex As Exception

        End Try
    End Sub
End Class