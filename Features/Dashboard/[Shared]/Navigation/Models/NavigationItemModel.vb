Imports Prism.Mvvm
Imports System.Windows.Media

Public Class NavigationItemModel
    Inherits BindableBase

    Private _isSelected As Boolean
    Private _iconKey As String
    Private _iconFilledKey As String

    Public Property Title As String
    Public Property NavigationPath As String

    Public Property IconKey As String
        Get
            Return _iconKey
        End Get
        Set(value As String)
            SetProperty(_iconKey, value)
            RaisePropertyChanged(NameOf(Icon)) ' Refresh UI
        End Set
    End Property

    Public Property IconFilledKey As String
        Get
            Return _iconFilledKey
        End Get
        Set(value As String)
            SetProperty(_iconFilledKey, value)
            RaisePropertyChanged(NameOf(IconFilled)) ' Refresh UI
        End Set
    End Property

    Public ReadOnly Property Icon As DrawingBrush
        Get
            Return TryFindResource(Of DrawingBrush)(_iconKey)
        End Get
    End Property

    Public ReadOnly Property IconFilled As DrawingBrush
        Get
            Return TryFindResource(Of DrawingBrush)(_iconFilledKey)
        End Get
    End Property

    Public Property IsSelected As Boolean
        Get
            Return _isSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isSelected, value)
        End Set
    End Property

    Public Sub New(title As String, navigationPath As String, iconKey As String, iconFilledKey As String)
        Me.Title = title
        Me.NavigationPath = navigationPath
        Me.IconKey = iconKey
        Me.IconFilledKey = iconFilledKey
        Me.IsSelected = False
    End Sub

    Public Sub RefreshIcons()
        RaisePropertyChanged(NameOf(Icon))
        RaisePropertyChanged(NameOf(IconFilled))
    End Sub

    Private Function TryFindResource(Of T)(key As String) As T
        If Application.Current.Resources.Contains(key) Then
            Return CType(Application.Current.Resources(key), T)
        End If
        Return Nothing
    End Function
End Class
