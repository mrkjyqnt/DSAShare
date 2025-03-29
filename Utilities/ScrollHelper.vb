Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Controls
Imports System.Windows.Media

Public Class ScrollHelper
    Public Shared Function GetBubbleScroll(ByVal obj As DependencyObject) As Boolean
        Return CBool(obj.GetValue(BubbleScrollProperty))
    End Function

    Public Shared Sub SetBubbleScroll(ByVal obj As DependencyObject, ByVal value As Boolean)
        obj.SetValue(BubbleScrollProperty, value)
    End Sub

    Public Shared ReadOnly BubbleScrollProperty As DependencyProperty =
        DependencyProperty.RegisterAttached("BubbleScroll", GetType(Boolean), GetType(ScrollHelper), New PropertyMetadata(False, AddressOf OnBubbleScrollChanged))

    Private Shared Sub OnBubbleScrollChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim element As UIElement = TryCast(d, UIElement)
        If element IsNot Nothing Then
            If CBool(e.NewValue) Then
                AddHandler element.PreviewMouseWheel, AddressOf OnPreviewMouseWheel
            Else
                RemoveHandler element.PreviewMouseWheel, AddressOf OnPreviewMouseWheel
            End If
        End If
    End Sub

    Private Shared Sub OnPreviewMouseWheel(ByVal sender As Object, ByVal e As MouseWheelEventArgs)
        Dim element As UIElement = TryCast(sender, UIElement)
        If element Is Nothing Then Return

        ' Find the parent ScrollViewer
        Dim scrollViewer As ScrollViewer = FindParentScrollViewer(element)

        ' If found, bubble up the scroll event
        If scrollViewer IsNot Nothing Then
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3)
            e.Handled = True
        End If
    End Sub

    ' Recursively search for the parent ScrollViewer
    Private Shared Function FindParentScrollViewer(ByVal element As DependencyObject) As ScrollViewer
        Dim parent As DependencyObject = VisualTreeHelper.GetParent(element)
        While parent IsNot Nothing
            If TypeOf parent Is ScrollViewer Then
                Return DirectCast(parent, ScrollViewer)
            End If
            parent = VisualTreeHelper.GetParent(parent)
        End While
        Return Nothing
    End Function
End Class
