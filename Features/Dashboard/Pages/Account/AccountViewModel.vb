Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions

#Disable Warning CRR0029
Public Class AccountViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _sessionManager As ISessionManager
    Private _parameters As NavigationParameters

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
            _parameters = New NavigationParameters() From {
                {"userId", _sessionManager.CurrentUser.Id},
                {"openedFrom", "AccountView"}
            }

            Load()
        Catch ex As Exception
            Debug.WriteLine($"[AccountViewModel] Initialize Error: {ex.Message}")
        End Try
    End Sub

    Private Async Sub Load()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100).ConfigureAwait(True)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            Await Task.Delay(100).ContinueWith(Sub()
                                                   Application.Current.Dispatcher.InvokeAsync(Sub() _regionManager.RequestNavigate("AccountPageRegion", "UserInformationsView", _parameters))
                                               End Sub)
        Catch ex As Exception
            Debug.WriteLine($"[AccountViewModel] Load Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub
End Class