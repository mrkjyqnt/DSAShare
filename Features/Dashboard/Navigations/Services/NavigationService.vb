
Public Class NavigationService
    Private ReadOnly _sessionManager As SessionManager

    Public Sub New(sessionManager As SessionManager)
        _sessionManager = sessionManager
    End Sub

    ''' <summary>
    ''' Gets the menu items based on the user's role.
    ''' </summary>
    ''' <returns>A list of menu items.</returns>
    Public Function GetNavigationItems() As List(Of NavigationItemModel)
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

    Private Function GetAdminNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Home", "HomeView", "HomeIcon"),
            New NavigationItemModel("Public Files", "PublicFilesView", "PublicFilesIcon"),
            New NavigationItemModel("Shared Files", "SharedFilesView", "SharedFilesIcon"),
            New NavigationItemModel("Accessed Files", "AccessedFilesView", "AccessedFilesIcon"),
            New NavigationItemModel("Manage Users", "ManageUsersView", "ManageUsersIcon"),
            New NavigationItemModel("Manage Files", "ManageFilesView", "ManageFilesIcon"),
            New NavigationItemModel("Account", "AccountView", "AccountIcon")
        }
    End Function

    Private Function GetMemberNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Home", "HomeView", "HomeIcon"),
            New NavigationItemModel("Public Files", "PublicFilesView", "PublicFilesIcon"),
            New NavigationItemModel("Shared Files", "SharedFilesView", "SharedFilesIcon"),
            New NavigationItemModel("Accessed Files", "AccessedFilesView", "AccessedFilesIcon"),
            New NavigationItemModel("Account", "AccountView", "AccountIcon")
        }
    End Function

    Private Function GetGuestNavItems() As List(Of NavigationItemModel)
        Return New List(Of NavigationItemModel) From {
            New NavigationItemModel("Home", "HomeView", "HomeIcon"),
            New NavigationItemModel("Public Files", "PublicFilesView", "PublicFilesIcon"),
            New NavigationItemModel("Account", "AccountView", "AccountIcon")
        }
    End Function
End Class