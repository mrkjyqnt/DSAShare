Imports Prism.Navigation

Public Class NavigationHistoryService
    Implements INavigationHistoryService

    Private ReadOnly ItemStack As New Stack(Of String)
    Private ReadOnly ViewStack As New Stack(Of String)
    Private ReadOnly RegionStack As New Stack(Of String)

    Public Sub PushPage(Region As String, View As String, Item As String) Implements INavigationHistoryService.PushPage
        ItemStack.Push(Item)
        RegionStack.Push(Region)
        ViewStack.Push(View)
    End Sub

    Public Function PopPage() As (Region As String, View As String, Item As String) Implements INavigationHistoryService.PopPage
        If ItemStack.Count > 1 Then
            Return (RegionStack.ElementAt(1), ViewStack.ElementAt(1), ItemStack.ElementAt(1))
        End If
        Return (Nothing, Nothing, Nothing)
    End Function

    Public Function PeekPage() As (Region As String, View As String, Item As String) Implements INavigationHistoryService.PeekPage
        If ItemStack.Count > 0 Then
            Return (RegionStack.Peek(), ViewStack.Peek(), ItemStack.Peek())
        End If
        Return (Nothing, Nothing, Nothing)
    End Function

    Public Sub RemoveCurrentPage() Implements INavigationHistoryService.RemoveCurrentPage
        If ItemStack.Count > 1 Then
            RegionStack.Pop()
            ViewStack.Pop()
            ItemStack.Pop()
        End If
    End Sub

    Public Sub Reset() Implements INavigationHistoryService.Reset
        RegionStack.Clear
        ViewStack.Clear
        ItemStack.Clear
    End Sub

    Public ReadOnly Property CanGoBack As Boolean Implements INavigationHistoryService.CanGoBack
        Get
            Return ItemStack.Count > 1
        End Get
    End Property
End Class