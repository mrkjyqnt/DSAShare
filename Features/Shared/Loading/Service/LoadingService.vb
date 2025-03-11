Imports Prism.Navigation.Regions
Imports System.Windows.Threading

''' <summary>
''' Service to show and hide the loading view.
''' </summary>
Public Class LoadingService
    Implements ILoadingService

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _dispatcher As Dispatcher

    Public Sub New(regionManager As IRegionManager, dispatcher As Dispatcher)
        _regionManager = regionManager
        _dispatcher = dispatcher
    End Sub

    ''' <summary>
    ''' Show the loading view.
    ''' </summary>
    Public Sub ShowLoading() Implements ILoadingService.ShowLoading
        _dispatcher.Invoke(Sub()
                               If _regionManager.Regions.ContainsRegionWithName("LoadingRegion") Then
                                   Dim loadingRegion = _regionManager.Regions("LoadingRegion")
                                   Dim loadingView = loadingRegion.GetView("LoadingView")

                                   If loadingView Is Nothing Then
                                       loadingView = New LoadingView()
                                       loadingRegion.Add(loadingView, "LoadingView")
                                   End If

                                   loadingRegion.Activate(loadingView)
                               Else
                                   ' Handle the case where the region does not exist
                                   Throw New InvalidOperationException("LoadingRegion not found.")
                               End If
                           End Sub)
    End Sub

    ''' <summary>
    ''' Hide the loading view.
    ''' </summary>
    Public Sub HideLoading() Implements ILoadingService.HideLoading
        _dispatcher.Invoke(Sub()
                               If _regionManager.Regions.ContainsRegionWithName("LoadingRegion") Then
                                   Dim loadingRegion = _regionManager.Regions("LoadingRegion")
                                   Dim loadingView = loadingRegion.GetView("LoadingView")

                                   If loadingView IsNot Nothing Then
                                       loadingRegion.Deactivate(loadingView)
                                   End If
                               Else
                                   ' Handle the case where the region does not exist
                                   Throw New InvalidOperationException("LoadingRegion not found.")
                               End If
                           End Sub)
    End Sub
End Class