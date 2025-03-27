Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Public Class StackPanelHelper
    Public Shared ReadOnly SpacingProperty As DependencyProperty = 
        DependencyProperty.RegisterAttached("Spacing", 
            GetType(Double), 
            GetType(StackPanelHelper),
            New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, AddressOf OnSpacingChanged))

    Public Shared Function GetSpacing(obj As DependencyObject) As Double
        Return CDbl(obj.GetValue(SpacingProperty))
    End Function

    Public Shared Sub SetSpacing(obj As DependencyObject, value As Double)
        obj.SetValue(SpacingProperty, value)
    End Sub

    Private Shared Sub OnSpacingChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim panel = TryCast(d, StackPanel)
        If panel IsNot Nothing Then
            UpdateSpacing(panel)
            AddHandler panel.Loaded, AddressOf PanelLoaded
        End If
    End Sub

    Private Shared Sub PanelLoaded(sender As Object, e As RoutedEventArgs)
        Dim panel = TryCast(sender, StackPanel)
        If panel IsNot Nothing Then
            UpdateSpacing(panel)
        End If
    End Sub

    Private Shared Sub UpdateSpacing(panel As StackPanel)
        If panel Is Nothing Then Return
        
        Dim spacing = GetSpacing(panel)
        Dim visibleChildren = panel.Children.OfType(Of FrameworkElement)().Where(Function(c) c.Visibility = Visibility.Visible).ToList()
        
        For i = 0 To visibleChildren.Count - 1
            Dim child = visibleChildren(i)
            If panel.Orientation = Orientation.Vertical Then
                child.Margin = New Thickness(0, 0, 0, If(i < visibleChildren.Count - 1, spacing, 0))
            Else
                child.Margin = New Thickness(0, 0, If(i < visibleChildren.Count - 1, spacing, 0), 0)
            End If
        Next
    End Sub
End Class
