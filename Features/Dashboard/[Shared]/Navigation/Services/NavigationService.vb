Imports Prism.Events
Imports Prism.Navigation
Imports Prism.Navigation.Regions

Public Class NavigationService
    Implements INavigationService

    Private ReadOnly _eventAggregator As IEventAggregator
    Private ReadOnly _navigationHistoryService As INavigationHistoryService
    Private ReadOnly _regionManager As IRegionManager
    Public Sub New(eventAggregator As IEventAggregator,
                   navigationHistoryService As INavigationHistoryService,
                   regionManager As IRegionManager)
        _eventAggregator = eventAggregator
        _navigationHistoryService = navigationHistoryService
        _regionManager = regionManager
    End Sub

    Public Sub Start(Optional Region As String = "", Optional View As String = "", Optional NavigationItem As String = "") Implements INavigationService.Start
        _navigationHistoryService.Reset()
        _navigationHistoryService.PushPage(Region, View, NavigationItem)
    End Sub

    Public Sub Go(Optional Region As String = "", Optional View As String = "", Optional NavigationItem As String = "", Optional parameters As NavigationParameters = Nothing) Implements INavigationService.Go
        Try
            _navigationHistoryService.PushPage(Region, View, NavigationItem)

            Dim safeParams = If(parameters, New NavigationParameters())

            _regionManager.RequestNavigate(Region, View, safeParams)
            _eventAggregator.GetEvent(Of NavigationSelectionEvent)().Publish(NavigationItem)
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Theres an error getting the user activities")
            Debug.WriteLine(ex.Message)

            Return
        End Try
    End Sub

    Public Sub GoBack() Implements INavigationService.GoBack
        If _navigationHistoryService.CanGoBack Then
            Dim prev = _navigationHistoryService.PopPage()

            _regionManager.RequestNavigate(prev.Region, prev.View)
            _eventAggregator.GetEvent(Of NavigationSelectionEvent)().Publish(prev.Item)

            _navigationHistoryService.RemoveCurrentPage()
        End If
    End Sub
End Class