Imports Prism.Events
Imports Prism.Navigation
Imports Prism.Navigation.Regions

Public Class NavigationService
    Implements INavigationService

    Private ReadOnly _eventAggregator As IEventAggregator
    Private ReadOnly _navigationHistory As INavigationHistoryService
    Private ReadOnly _regionManager As IRegionManager
    Private _isNavigating As Boolean = False

    Public Sub New(eventAggregator As IEventAggregator,
                 navigationHistory As INavigationHistoryService,
                 regionManager As IRegionManager)
        _eventAggregator = eventAggregator
        _navigationHistory = navigationHistory
        _regionManager = regionManager
    End Sub

    Public Sub Start(Optional region As String = "", Optional view As String = "", Optional item As String = "") _
        Implements INavigationService.Start

        ' Simply reset and record initial state without navigating
        _navigationHistory.Reset()
        If Not String.IsNullOrEmpty(region) AndAlso Not String.IsNullOrEmpty(view) Then
            _navigationHistory.PushPage(region, view, item, New NavigationParameters())
        End If
    End Sub

    Public Sub Go(Optional region As String = "", Optional view As String = "",
                   Optional item As String = "", Optional parameters As NavigationParameters = Nothing) _
        Implements INavigationService.Go

        If _isNavigating Then Return

        Try
            _isNavigating = True
            Dim safeParams = If(parameters, New NavigationParameters())

            ' First record the navigation intent
            _navigationHistory.PushPage(region, view, item, safeParams)
            _eventAggregator.GetEvent(Of NavigationSelectionEvent)().Publish(item)

            ' Then perform actual navigation
            _regionManager.RequestNavigate(region, view, safeParams)

        Catch ex As Exception
            Debug.WriteLine($"[NavigationService] Go Error: {ex.Message}")
        Finally
            _isNavigating = False
        End Try
    End Sub

    Public Sub GoBack() Implements INavigationService.GoBack
        If _isNavigating OrElse Not _navigationHistory.CanGoBack Then Return

        Try
            _isNavigating = True
            Dim previous = _navigationHistory.GoBack()

            _eventAggregator.GetEvent(Of NavigationSelectionEvent)().Publish(previous.Item)
            _regionManager.RequestNavigate(previous.Region, previous.View, previous.Parameters)

        Finally
            _isNavigating = False
        End Try
    End Sub

    Public Sub GoForward() Implements INavigationService.GoForward
        If _isNavigating OrElse Not _navigationHistory.CanGoForward Then Return

        Try
            _isNavigating = True
            Dim [next] = _navigationHistory.GoForward()

            _eventAggregator.GetEvent(Of NavigationSelectionEvent)().Publish([next].Item)
            _regionManager.RequestNavigate([next].Region, [next].View, [next].Parameters)

        Finally
            _isNavigating = False
        End Try
    End Sub
End Class