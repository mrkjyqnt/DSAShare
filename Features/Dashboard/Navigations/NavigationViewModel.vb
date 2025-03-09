Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Class MenuNavigationViewModel
    Inherits BindableBase

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationService As NavigationService

    Private _menuItems As List(Of NavigationItemModel)

    Public ReadOnly Property NavSelectionCommand As DelegateCommand(Of String)

    Public Property MenuItems As List(Of NavigationItemModel)
        Get
            Return _menuItems
        End Get
        Set(value As List(Of NavigationItemModel))
            SetProperty(_menuItems, value)
        End Set
    End Property

    Public Sub New(regionManager As IRegionManager, menuNavigationService As NavigationService)
        _regionManager = regionManager
        _navigationService = menuNavigationService

        ' Initialize the navigation command
        NavSelectionCommand = New DelegateCommand(Of String)(AddressOf OnNavigationSelection)

        ' Load the menu items based on the user's role
        MenuItems = _navigationService.GetNavigationItems()
    End Sub

    Private Sub OnNavigationSelection(navigationPath As String)
        ' Navigate to the selected view in the PageRegion
        _regionManager.RequestNavigate("PageRegion", navigationPath)
    End Sub
End Class