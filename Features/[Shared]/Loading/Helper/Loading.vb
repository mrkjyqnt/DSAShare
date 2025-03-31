Imports Prism.Navigation.Regions
Imports Prism.Ioc

Public Class Loading
    Private Shared ReadOnly _loadingService As ILoadingService

' Shared constructor to initialize _loadingService
    Shared Sub New()
        Dim regionManager As IRegionManager = ContainerLocator.Container.Resolve(Of IRegionManager)()
        _loadingService = New LoadingService(regionManager) ' Initialize with your implementation
    End Sub

    Public Shared Sub Show()
        Application.Current.Dispatcher.InvokeAsync(Sub()
                                                _loadingService.Show(New LoadingView)
                                              End Sub)
    End Sub

    Public Shared Sub StartUp()
        Application.Current.Dispatcher.InvokeAsync(Sub()
                                                _loadingService.Show(New StartupLoadingView)
                                              End Sub)
    End Sub

    Public Shared Sub Hide()
        Application.Current.Dispatcher.InvokeAsync(Sub()
                                                  _loadingService.Hide()
                                              End Sub)
    End Sub
End Class