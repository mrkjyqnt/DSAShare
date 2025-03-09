Public Class SessionManager
    Private _currentUser As Users

    ''' <summary>
    ''' Gets the current logged-in user.
    ''' </summary>
    Public ReadOnly Property CurrentUser As Users
        Get
            Return _currentUser
        End Get
    End Property

    ''' <summary>
    ''' Logs in a user.
    ''' </summary>
    ''' <param name="user">The user to log in.</param>
    Public Sub Login(user As Users)
        _currentUser = user
    End Sub

    ''' <summary>
    ''' Logs out the current user.
    ''' </summary>
    Public Sub Logout()
        _currentUser = Nothing
    End Sub

    ''' <summary>
    ''' Checks if a user is logged in.
    ''' </summary>
    ''' <returns>True if a user is logged in; otherwise, False.</returns>
    Public Function IsLoggedIn() As Boolean
        Return _currentUser IsNot Nothing
    End Function

    ''' <summary>
    ''' Checks if the current user has a specific role.
    ''' </summary>
    ''' <param name="role">The role to check (e.g., "Admin", "Member", "Guest").</param>
    ''' <returns>True if the user has the specified role; otherwise, False.</returns>
    Public Function HasRole(role As String) As Boolean
        Return IsLoggedIn() AndAlso _currentUser.Role.Equals(role, StringComparison.OrdinalIgnoreCase)
    End Function
End Class