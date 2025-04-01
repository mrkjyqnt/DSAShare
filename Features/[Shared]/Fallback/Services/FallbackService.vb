Imports Prism.Navigation.Regions

Public Class FallbackService
    Implements IFallbackService

    Private ReadOnly _regionManager As IRegionManager

    Public Sub New(regionManager As IRegionManager)
        _regionManager = regionManager
    End Sub

    Public Sub Show(View As Object) Implements IFallbackService.Show
        _regionManager.Regions("FallbackRegion").Add(View)

        Dim mainWindow = CType(Application.Current.MainWindow, MainWindow)
        mainWindow.Fallback.Visibility = Visibility.Visible
    End Sub

    Public Sub Hide() Implements IFallbackService.Hide
        _regionManager.Regions("FallbackRegion").RemoveAll()

        Dim mainWindow = CType(Application.Current.MainWindow, MainWindow)
        mainWindow.Fallback.Visibility = Visibility.Collapsed
    End Sub
End Class
