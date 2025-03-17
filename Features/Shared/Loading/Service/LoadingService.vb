Imports Prism.Navigation.Regions
Imports System.Windows.Threading

''' <summary>
''' Service to show and hide the loading view.
''' </summary>
Public Class LoadingService
    Implements ILoadingService

    Private ReadOnly _regionManager As IRegionManager

    Public Sub New(regionManager As IRegionManager)
        _regionManager = regionManager
    End Sub

    Public Sub Show(View As Object) Implements ILoadingService.Show
        _regionManager.Regions("LoadingRegion").Add(View)

        Dim mainWindow = CType(Application.Current.MainWindow, MainWindow)
        mainWindow.Loading.Visibility = Visibility.Visible
    End Sub

    Public Sub Hide() Implements ILoadingService.Hide
        _regionManager.Regions("LoadingRegion").RemoveAll()

        Dim mainWindow = CType(Application.Current.MainWindow, MainWindow)
        mainWindow.Loading.Visibility = Visibility.Collapsed
    End Sub

End Class