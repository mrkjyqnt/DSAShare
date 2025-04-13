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
        Return New NavigationItemModel("Account", "AccountView", IconSource("account.png"), IconSource("account-filled.png"))
    End Function

    Private Function GetAdminNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Home", "HomeView", IconSource("home.png"), IconSource("home-filled.png")),
            New NavigationItemModel("Public Files", "PublicFilesView", IconSource("public.png"), IconSource("public-filled.png")),
            New NavigationItemModel("Shared Files", "SharedFilesView", IconSource("shared.png"), IconSource("shared-filled.png")),
            New NavigationItemModel("Accessed Files", "AccessedFilesView", IconSource("accessed.png"), IconSource("accessed-filled.png")),
            New NavigationItemModel("Manage Files", "ManageFilesView", IconSource("files.png"), IconSource("files-filled.png")),
            New NavigationItemModel("Manage Users", "ManageUsersView", IconSource("users.png"), IconSource("users-filled.png")),
            New NavigationItemModel("Activities", "ActivitiesView", IconSource("activities.png"), IconSource("activities-filled.png")),
            New NavigationItemModel("Downloads", "DownloadsView", IconSource("folder-download.png"), IconSource("folder-download-filled.png"))
        }
    End Function

    Private Function GetMemberNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Home", "HomeView", IconSource("home.png"), IconSource("home-filled.png")),
            New NavigationItemModel("Public Files", "PublicFilesView", IconSource("public.png"), IconSource("public-filled.png")),
            New NavigationItemModel("Shared Files", "SharedFilesView", IconSource("shared.png"), IconSource("shared-filled.png")),
            New NavigationItemModel("Accessed Files", "AccessedFilesView", IconSource("accessed.png"), IconSource("accessed-filled.png")),
            New NavigationItemModel("Activities", "ActivitiesView", IconSource("activities.png"), IconSource("activities-filled.png")),
            New NavigationItemModel("Downloads", "DownloadsView", IconSource("folder-download.png"), IconSource("folder-download-filled.png"))
        }
    End Function

    Private Function GetGuestNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Public Files", "PublicFilesView", IconSource("public.png"), IconSource("public-filled.png")),
            New NavigationItemModel("Downloads", "DownloadsView", IconSource("folder-download.png"), IconSource("folder-download-filled.png"))
        }
    End Function
End Class
