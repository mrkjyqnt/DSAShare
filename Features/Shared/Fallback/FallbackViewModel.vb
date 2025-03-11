Imports System.Windows.Threading
Imports CommunityToolkit.Mvvm.Input
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Class FallbackViewModel
    Inherits BindableBase

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _fallBackService As IFallbackService
    Private ReadOnly _loadingService As ILoadingService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _dispatcher As Dispatcher

    Public ReadOnly Property RetryCommand As IAsyncRelayCommand

    Sub New(regionManager As IRegionManager, fallbackService As IFallbackService, sessionManager As ISessionManager)
        _regionManager = regionManager
        _fallBackService = fallbackService
        _sessionManager = sessionManager

        _dispatcher = Application.Current.Dispatcher
        _loadingService = New LoadingService(_regionManager, _dispatcher)

        RetryCommand = New AsyncRelayCommand(AddressOf OnRetryCommand)
    End Sub

    Private Async Function OnRetryCommand() As Task
        Debug.WriteLine("Retry Command Clicked")
        Try
            _loadingService.ShowLoading

            If Await Task.Run(Function() _fallBackService.Retry()).ConfigureAwait(False) Then

                If _sessionManager.IsLoggedIn() Then
                    _dispatcher.Invoke(Sub()
                                           _regionManager.RequestNavigate("MainRegion", "DashboardView")
                                       End Sub)
                Else
                    _dispatcher.Invoke(Sub()
                                           _regionManager.RequestNavigate("MainRegion", "AuthenticationView")
                                       End Sub)
                End If
            End If

            Return
        Catch ex As Exception
            ErrorHandler.SetError(ex.Message)
        Finally
            _dispatcher.Invoke(Sub()
                                   _loadingService.HideLoading()
                               End Sub)
        End Try
    End Function

End Class
