Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports Prism.Events

Public Class NavigationViewModel
    Inherits BindableBase
    Implements INavigationAware

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationListService As INavigationListService
    Private ReadOnly _eventAggregator As IEventAggregator
    Private _menuItems As List(Of NavigationItemModel)
    Private _lastMenuItem As NavigationItemModel
    Private _navSelectionCommand As DelegateCommand(Of NavigationItemModel)

    Public Property NavSelectionCommand As DelegateCommand(Of NavigationItemModel)
        Get
            Return _navSelectionCommand
        End Get
        Private Set(value As DelegateCommand(Of NavigationItemModel))
            SetProperty(_navSelectionCommand, value)
        End Set
    End Property

    Public Property MenuItems As List(Of NavigationItemModel)
        Get
            Return _menuItems
        End Get
        Set(value As List(Of NavigationItemModel))
            SetProperty(_menuItems, value)
        End Set
    End Property

    Public Property LastMenuItem As NavigationItemModel
        Get
            Return _lastMenuItem
        End Get
        Set(value As NavigationItemModel)
            SetProperty(_lastMenuItem, value)
        End Set
    End Property

    Public Sub New(regionManager As IRegionManager, navigationService As INavigationListService, eventAggregator As IEventAggregator)
        _regionManager = regionManager
        _navigationListService = navigationService
        _eventAggregator = eventAggregator

        MenuItems = New List(Of NavigationItemModel)()

        NavSelectionCommand = New DelegateCommand(Of NavigationItemModel)(AddressOf OnNavigationSelection)

        _eventAggregator.GetEvent(Of NavigationSelectionEvent)().Subscribe(AddressOf SetSelection)
        _eventAggregator.GetEvent(Of ThemeChangedEvent)().Subscribe(AddressOf OnThemeChanged)
    End Sub

    Private Sub OnNavigationSelection(selectedItem As NavigationItemModel)
        If selectedItem Is Nothing Then Exit Sub

        If selectedItem.IsSelected Then
            Debug.WriteLine("[NavigationViewModel] Item already selected, skipping...")
            Return
        End If

        For Each item In MenuItems
            item.IsSelected = False
        Next
        LastMenuItem.IsSelected = False

        selectedItem.IsSelected = True

        _regionManager.RequestNavigate("PageRegion", selectedItem.NavigationPath)

        RaisePropertyChanged(NameOf(MenuItems))
        RaisePropertyChanged(NameOf(LastMenuItem))
    End Sub


    Public Sub SetSelection(itemTitle As String)
        ' Guard against null references
        If MenuItems Is Nothing OrElse LastMenuItem Is Nothing Then
            Debug.WriteLine("[NavigationViewModel] MenuItems or LastMenuItem not loaded yet")
            Return
        End If

        ' Deselect all items first
        For Each item In MenuItems
            item.IsSelected = False
        Next

        LastMenuItem.IsSelected = False

        ' Find the item to select by Title
        If itemTitle = LastMenuItem.Title Then
            LastMenuItem.IsSelected = True
        Else
            Dim selectedItem = MenuItems.FirstOrDefault(Function(x) x.Title = itemTitle)
            If selectedItem IsNot Nothing Then
                selectedItem.IsSelected = True
            End If
        End If
    End Sub

    Private Sub OnThemeChanged(theme As AppTheme)
        If MenuItems Is Nothing Then Exit Sub

        For Each item In MenuItems
            item.RefreshIcons()
        Next

        RaisePropertyChanged(NameOf(MenuItems))
    End Sub

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedTo
        Try
            Await Task.Run(Sub() MenuItems = _navigationListService.GetNavigationItems()).ConfigureAwait(True)
            Await Task.Run(Sub() LastMenuItem = _navigationListService.GetLastNavigationItem()).ConfigureAwait(True)

            If MenuItems IsNot Nothing AndAlso MenuItems.Count > 0 Then
                MenuItems(0).IsSelected = True
            End If
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Return True
    End Function

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedFrom
    End Sub
End Class