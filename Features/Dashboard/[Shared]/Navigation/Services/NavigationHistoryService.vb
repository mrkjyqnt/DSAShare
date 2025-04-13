Imports Prism.Navigation

Public Class NavigationHistoryService
    Implements INavigationHistoryService
    
    Private ReadOnly _history As New List(Of NavigationRecord)()
    Private _currentIndex As Integer = -1
    
    Public Sub PushPage(region As String, view As String, item As String, parameters As NavigationParameters) _
        Implements INavigationHistoryService.PushPage
        
        ' Remove any forward history if we're not at the end
        If _currentIndex < _history.Count - 1 Then
            _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1)
        End If
        
        _history.Add(New NavigationRecord(region, view, item, parameters))
        _currentIndex = _history.Count - 1
    End Sub
    
    Public Function GoBack() As NavigationRecord _
        Implements INavigationHistoryService.GoBack
        
        If CanGoBack Then
            _currentIndex -= 1
            Return _history(_currentIndex)
        End If
        Return Nothing
    End Function
    
    Public Function GoForward() As NavigationRecord _
        Implements INavigationHistoryService.GoForward
        
        If CanGoForward Then
            _currentIndex += 1
            Return _history(_currentIndex)
        End If
        Return Nothing
    End Function
    
    Public ReadOnly Property CanGoBack As Boolean _
        Implements INavigationHistoryService.CanGoBack
        Get
            Return _currentIndex > 0
        End Get
    End Property
    
    Public ReadOnly Property CanGoForward As Boolean _
        Implements INavigationHistoryService.CanGoForward
        Get
            Return _currentIndex < _history.Count - 1
        End Get
    End Property
    
    Public Sub Reset() Implements INavigationHistoryService.Reset
        _history.Clear()
        _currentIndex = -1
    End Sub
End Class