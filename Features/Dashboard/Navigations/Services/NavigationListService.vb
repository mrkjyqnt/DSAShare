Public Class NavigationListService
    Implements INavigationListService

    Private ReadOnly _sessionManager As ISessionManager

    Public Sub New(sessionManager As ISessionManager)
        _sessionManager = sessionManager
    End Sub

    Public Function GetNavigationItems() As List(Of NavigationItemModel) Implements INavigationListService.GetNavigationItems
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

    Public Function GetLastNavigationItem() As NavigationItemModel Implements INavigationListService.GetLastNavigationItem
        Return New NavigationItemModel("Account", "AccountView",
                                     "settings",
                                     "settings-filled")
    End Function

    Private Function GetAdminNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Home", "HomeView",
                                   "home",
                                   "home-filled"),
            New NavigationItemModel("Public Files", "PublicFilesView",
                                   "public",
                                   "public-filled"),
            New NavigationItemModel("Shared Files", "SharedFilesView",
                                   "shared",
                                   "shared-filled"),
            New NavigationItemModel("Accessed Files", "AccessedFilesView",
                                   "accessed",
                                   "accessed-filled"),
            New NavigationItemModel("Manage Files", "ManageFilesView",
                                   "manage-files",
                                   "manage-files-filled"),
            New NavigationItemModel("Manage Users", "ManageUsersView",
                                   "manage-users",
                                   "manage-users-filled"),
            New NavigationItemModel("Reports", "ReportsView",
                                   "reports",
                                   "reports-filled"),
            New NavigationItemModel("Activities", "ActivitiesView",
                                   "activities",
                                   "activities-filled"),
            New NavigationItemModel("Downloads", "DownloadsView",
                                   "downloads",
                                   "downloads-filled"),
            New NavigationItemModel("Uploads", "UploadsView",
                                   "downloads",
                                   "downloads-filled")
        }
    End Function

    Private Function GetMemberNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Home", "HomeView",
                                 "home",
                                 "home-filled"),
            New NavigationItemModel("Public Files", "PublicFilesView",
                                 "public",
                                 "public-filled"),
            New NavigationItemModel("Shared Files", "SharedFilesView",
                                 "shared",
                                 "shared-filled"),
            New NavigationItemModel("Accessed Files", "AccessedFilesView",
                                 "accessed",
                                 "accessed-filled"),
            New NavigationItemModel("Activities", "ActivitiesView",
                                 "activities",
                                 "activities-filled"),
            New NavigationItemModel("Downloads", "DownloadsView",
                                 "downloads",
                                 "downloads-filled"),
            New NavigationItemModel("Uploads", "UploadsView",
                                   "downloads", 
                                   "downloads-filled")
        }
    End Function

    Private Function GetGuestNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Public Files", "PublicFilesView",
                                 "public",
                                 "public-filled"),
            New NavigationItemModel("Downloads", "DownloadsView",
                                 "downloads",
                                 "downloads-filled")
        }
    End Function
End Class