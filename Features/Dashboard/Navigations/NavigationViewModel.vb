Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Class NavigationViewModel
    Inherits BindableBase

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationService As NavigationService
    Private _menuItems As List(Of NavigationItemModel)
    Private _lastMenuItem As NavigationItemModel
    Private _navSelectionCommand As DelegateCommand(Of NavigationItemModel)

    ' Navigation command
    Public Property NavSelectionCommand As DelegateCommand(Of NavigationItemModel)
        Get
            Return _navSelectionCommand
        End Get
        Private Set(value As DelegateCommand(Of NavigationItemModel))
            SetProperty(_navSelectionCommand, value)
        End Set
    End Property

    ' Regular menu items
    Public Property MenuItems As List(Of NavigationItemModel)
        Get
            Return _menuItems
        End Get
        Set(value As List(Of NavigationItemModel))
            SetProperty(_menuItems, value)
        End Set
    End Property

    ' Separate property for the Account button
    Public Property LastMenuItem As NavigationItemModel
        Get
            Return _lastMenuItem
        End Get
        Set(value As NavigationItemModel)
            SetProperty(_lastMenuItem, value)
        End Set
    End Property

    Public Sub New(regionManager As IRegionManager, menuNavigationService As NavigationService)
        _regionManager = regionManager
        _navigationService = menuNavigationService

        ' Initialize the navigation command
        NavSelectionCommand = New DelegateCommand(Of NavigationItemModel)(AddressOf OnNavigationSelection)

        ' Load the menu items based on the user's role
        MenuItems = _navigationService.GetNavigationItems()
        LastMenuItem = _navigationService.GetLastNavigationItem()

        ' Debugging: Check LastMenuItem properties
        Debug.WriteLine($"LastMenuItem Title: {LastMenuItem?.Title}")
        Debug.WriteLine($"LastMenuItem Icon: {LastMenuItem?.Icon}")
        Debug.WriteLine($"LastMenuItem IsSelected: {LastMenuItem?.IsSelected}")
    End Sub


    Private Sub OnNavigationSelection(selectedItem As NavigationItemModel)
        If selectedItem Is Nothing Then Exit Sub

        ' Deselect all buttons first
        For Each item In MenuItems
            item.IsSelected = False
        Next
        LastMenuItem.IsSelected = False ' Deselect the Account button too

        ' Mark the selected item
        selectedItem.IsSelected = True

        ' Navigate to the selected view
        _regionManager.RequestNavigate("PageRegion", selectedItem.NavigationPath)

        ' Notify UI of property changes
        RaisePropertyChanged(NameOf(MenuItems))
        RaisePropertyChanged(NameOf(LastMenuItem))
    End Sub
End Class
