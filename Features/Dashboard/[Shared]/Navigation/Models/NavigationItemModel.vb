Imports Prism.Mvvm

Public Class NavigationItemModel
    Inherits BindableBase

    Private _isSelected As Boolean

    Public Property Title As String
    Public Property NavigationPath As String
    Public Property Icon As String
    Public Property IconFilled As String

    Public Property IsSelected As Boolean
        Get
            Return _isSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isSelected, value)
        End Set
    End Property

    Public Sub New(title As String, navigationPath As String, icon As String, iconFilled As String)
        Me.Title = title
        Me.NavigationPath = navigationPath
        Me.Icon = icon
        Me.IconFilled = iconFilled
        Me.IsSelected = False
    End Sub
End Class
