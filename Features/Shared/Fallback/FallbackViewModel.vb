Imports CommunityToolkit.Mvvm.Input
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Class FallbackViewModel
    Inherits BindableBase

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _fallBackService As IFallbackService

    Public ReadOnly Property RetryCommand As IAsyncRelayCommand

    Sub New(regionManager As IRegionManager, fallbackService As IFallbackService)
        _regionManager = regionManager
        _fallBackService = fallbackService
        RetryCommand = New AsyncRelayCommand(AddressOf OnRetryCommand)
    End Sub

    Private Async Function OnRetryCommand() As Task
        Try
            If Await Task.Run(Function() _fallBackService.Retry()).ConfigureAwait(False) Then
                Application.Current.Dispatcher.Invoke(Sub()
                    _regionManager.RequestNavigate("MainRegion", "DashboardView")
                End Sub)
            End If

            Return
        Catch ex As Exception

        End Try
    End Function

End Class
