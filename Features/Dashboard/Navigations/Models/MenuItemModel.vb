Public Class NavigationItemModel
    Public Property Title As String
    Public Property NavigationPath As String
    Public Property Icon As String

    Public Sub New(title As String, navigationPath As String, Optional icon As String = "")
        Me.Title = title
        Me.NavigationPath = navigationPath
        Me.Icon = icon
    End Sub
End Class