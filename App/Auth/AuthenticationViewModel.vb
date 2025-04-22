Imports Prism.Mvvm
Imports Prism.Navigation.Regions

''' <summary>
''' ViewModel for the Authentication.
''' </summary>
#Disable Warning
Public Class AuthenticationViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _fallBackService As IFallbackService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _sessionManager As ISessionManager

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Private ReadOnly _regionManager As IRegionManager

    Public Sub New(regionManager As IRegionManager,
                   fallBackService As IFallbackService,
                   navigationService As INavigationService,
                   sessionManager As ISessionManager)

        _regionManager = regionManager
        _fallBackService = fallBackService
        _navigationService = navigationService
        _sessionManager = sessionManager

        Load()
    End Sub

    Private Async Sub Load()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            _navigationService.Start("MainRegion", "AuthenticationView", "")
            _navigationService.Go("AuthenticationRegion", "SignInView")

            If Await Task.Run(Function() _sessionManager.IsLoggedIn) Then
                _navigationService.Go("MainRegion", "DashboardView")
            End If

        Catch ex As Exception
            ' Handle errors
        Finally
            Loading.Hide()
        End Try
    End Sub


End Class