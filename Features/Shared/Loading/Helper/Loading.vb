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
        _loadingService.Show(New LoadingView)
    End Sub

    Public Shared Sub StartUp()
        _loadingService.Show(New StartupLoadingView)
    End Sub

    Public Shared Sub Hide()
        _loadingService.Hide()
    End Sub
End Class