Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions

#Disable Warning CRR0029
Public Class AccountViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime
    Implements INavigationAware

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _userService As IUserService
    Private _parameters As NavigationParameters

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Sub New(regionManager As IRegionManager, 
                   navigationService As INavigationService,
                   sessionManager As ISessionManager, 
                   userService As IUserService)
        _regionManager = regionManager
        _navigationService = navigationService
        _sessionManager = sessionManager
        _userService = userService

        Load()
    End Sub

    Private Async Sub Load()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100).ConfigureAwait(True)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            _navigationService.Start("PageRegion","AccountView", "Account")

            Await Application.Current.Dispatcher.InvokeAsync(Sub() _regionManager.RequestNavigate("AccountPageRegion", "UserInformationsView", _parameters))
            Loading.Hide()
        Catch ex As Exception
            Debug.WriteLine($"[AccountViewModel] Load Error: {ex.Message}")
        End Try
    End Sub

    Public Sub OnNavigatedTo(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedTo
        _parameters = New NavigationParameters() From {
            {"userId", _sessionManager.CurrentUser.Id},
            {"openedFrom", "AccountView"}
        }
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Return True
    End Function

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedFrom
        Try
            If navigationContext IsNot Nothing Then
                Dim region = _regionManager.Regions("AccountPageRegion")
                Dim view = region.Views.FirstOrDefault(Function(v) v.GetType().Name = "UserInformationsView")
                If view IsNot Nothing Then
                    region.Remove(view)
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error navigating from AccountViewModel: {ex.Message}")
        End Try
    End Sub
End Class