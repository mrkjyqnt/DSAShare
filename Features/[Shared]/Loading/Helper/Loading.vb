Imports Prism.Navigation.Regions
Imports Prism.Ioc

Public Class Loading
    Private Shared ReadOnly _loadingService As ILoadingService

    ' Shared constructor to initialize _loadingService
    Shared Sub New()
        Dim regionManager As IRegionManager = ContainerLocator.Container.Resolve(Of IRegionManager)()
        _loadingService = New LoadingService(regionManager)
    End Sub

    Public Shared Sub Show()
        If Application.Current?.Dispatcher?.CheckAccess() Then
            _loadingService.Show(New LoadingView)
        Else
            Application.Current?.Dispatcher?.Invoke(Sub() _loadingService.Show(New LoadingView))
        End If
    End Sub

    Public Shared Sub StartUp()
        If Application.Current?.Dispatcher?.CheckAccess() Then
            _loadingService.Show(New StartupLoadingView)
        Else
            Application.Current?.Dispatcher?.Invoke(Sub() _loadingService.Show(New StartupLoadingView))
        End If
    End Sub

    Public Shared Sub Hide()
        If Application.Current?.Dispatcher?.CheckAccess() Then
            _loadingService.Hide()
        Else
            Application.Current?.Dispatcher?.Invoke(Sub() _loadingService.Hide())
        End If
    End Sub
End Class