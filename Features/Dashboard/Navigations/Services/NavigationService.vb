Imports Prism.Events
Imports Prism.Navigation.Regions

Public Class NavigationService
    Implements INavigationService

    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _eventAggregator As IEventAggregator
    Private ReadOnly _navigationHistoryService As INavigationHistoryService
    Private ReadOnly _regionManager As IRegionManager

    Public Sub New(sessionManager As ISessionManager, 
                   eventAggregator As IEventAggregator, 
                   navigationHistoryService As INavigationHistoryService,   
                   regionManager As IRegionManager)
        _sessionManager = sessionManager
        _eventAggregator = eventAggregator
        _navigationHistoryService = navigationHistoryService
        _regionManager = regionManager
    End Sub

    Public Function GetNavigationItems() As List(Of NavigationItemModel) Implements INavigationService.GetNavigationItems
        Dim menuItems As New List(Of NavigationItemModel)

        If _sessionManager.HasRole("Admin") Then
            menuItems.AddRange(GetAdminNavItems())
        ElseIf _sessionManager.HasRole("Member") Then
            menuItems.AddRange(GetMemberNavItems())
        ElseIf _sessionManager.HasRole("Guest") Then
            menuItems.AddRange(GetGuestNavItems())
        End If

        Return menuItems
    End Function

    Public Sub Start(Optional Region As String = "",Optional View As String = "", Optional NavigationItem As String = "") Implements INavigationService.Start
        _navigationHistoryService.Reset()
        _navigationHistoryService.PushPage(Region, View, NavigationItem)
    End Sub

    Public Sub Go(Optional Region As String = "",Optional View As String = "", Optional NavigationItem As String = "") Implements INavigationService.Go
        _navigationHistoryService.PushPage(Region, View, NavigationItem)
        _regionManager.RequestNavigate(Region, View)
        _eventAggregator.GetEvent(Of NavigationSelectionEvent)().Publish(NavigationItem)
    End Sub

    Public Sub GoBack() Implements INavigationService.GoBack
        Debug.WriteLine("GoBack: Checking CanGoBack...")
        If _navigationHistoryService.CanGoBack Then
            Debug.WriteLine("GoBack: CanGoBack is True. Navigating to previous view...")

            Dim previousNavigation = _navigationHistoryService.PopPage()

            If previousNavigation.Item IsNot Nothing Then
                Debug.WriteLine($"GoBack: Navigating back to: {previousNavigation.View} in region: {previousNavigation.Region}")
                _regionManager.RequestNavigate(previousNavigation.Region, previousNavigation.View)
                _eventAggregator.GetEvent(Of NavigationSelectionEvent)().Publish(previousNavigation.Item)

                _navigationHistoryService.RemoveCurrentPage()
            End If
        Else
            Debug.WriteLine("GoBack: CanGoBack is False. No more pages to go back to.")
            PopUp.Information("Info", "No more pages to go back to.")
        End If
    End Sub

    Public Function GetLastNavigationItem() As NavigationItemModel Implements INavigationService.GetLastNavigationItem
        Return New NavigationItemModel("Account", "AccountView", IconSource("account.png"), IconSource("account-filled.png"))
    End Function

    Private Function GetAdminNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Home", "HomeView", IconSource("home.png"), IconSource("home-filled.png")),
            New NavigationItemModel("Public Files", "PublicFilesView", IconSource("public.png"), IconSource("public-filled.png")),
            New NavigationItemModel("Shared Files", "SharedFilesView", IconSource("shared.png"), IconSource("shared-filled.png")),
            New NavigationItemModel("Accessed Files", "AccessedFilesView", IconSource("accessed.png"), IconSource("accessed-filled.png")),
            New NavigationItemModel("Manage Files", "ManageFilesView", IconSource("files.png"), IconSource("files-filled.png")),
            New NavigationItemModel("Manage Users", "ManageUsersView", IconSource("users.png"), IconSource("users-filled.png"))
        }
    End Function

    Private Function GetMemberNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Home", "HomeView", IconSource("home.png"), IconSource("home-filled.png")),
            New NavigationItemModel("Public Files", "PublicFilesView", IconSource("public.png"), IconSource("public-filled.png")),
            New NavigationItemModel("Shared Files", "SharedFilesView", IconSource("shared.png"), IconSource("shared-filled.png")),
            New NavigationItemModel("Accessed Files", "AccessedFilesView", IconSource("accessed.png"), IconSource("accessed-filled.png"))
        }
    End Function

    Private Function GetGuestNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Public Files", "PublicFilesView", IconSource("public.png"), IconSource("public-filled.png"))
        }
    End Function
End Class