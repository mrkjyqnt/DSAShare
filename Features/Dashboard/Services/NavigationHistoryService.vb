Public Class NavigationHistoryService
    Implements INavigationHistoryService

    Private ReadOnly NavigationStack As New Stack(Of String)

    ' Push the current page URI onto the stack
    Public Sub PushPage(uri As String) Implements INavigationHistoryService.PushPage
        NavigationStack.Push(uri)
    End Sub

    ' Pop the last page URI from the stack
    Public Function PopPage() As String Implements INavigationHistoryService.PopPage
        If NavigationStack.Count > 0 Then
            Return NavigationStack.Pop()
        End If
        Return Nothing
    End Function

    ' Peek at the last page URI without removing it
    Public Function PeekPage() As String Implements INavigationHistoryService.PeekPage
        If NavigationStack.Count > 0 Then
            Return NavigationStack.Peek()
        End If
        Return Nothing
    End Function

    ' Check if there is a page to go back to
    Public ReadOnly Property CanGoBack As Boolean Implements INavigationHistoryService.CanGoBack
        Get
            Return NavigationStack.Count > 0
        End Get
    End Property
End Class