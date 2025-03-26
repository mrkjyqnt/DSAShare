Imports System.Collections.Specialized

Public Class StackPanelHelper
    Private Shared ReadOnly _panelHandlers As New Dictionary(Of StackPanel, NotifyCollectionChangedEventHandler)()
    
    Public Shared Function GetSpacing(obj As DependencyObject) As Double
        Return CDbl(obj.GetValue(SpacingProperty))
    End Function

    Public Shared Sub SetSpacing(obj As DependencyObject, value As Double)
        obj.SetValue(SpacingProperty, value)
    End Sub

    Public Shared ReadOnly SpacingProperty As DependencyProperty =
        DependencyProperty.RegisterAttached("Spacing",
                                        GetType(Double),
                                        GetType(StackPanelHelper),
                                        New FrameworkPropertyMetadata(0.0,
                                                                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                                                                    AddressOf SpacingChanged))

    Private Shared Sub SpacingChanged(sender As Object, e As DependencyPropertyChangedEventArgs)
        Dim panel = TryCast(sender, StackPanel)
        If panel Is Nothing Then Return

        ' Clean up any existing handlers
        RemoveHandlers(panel)

        ' Only setup handlers if we have spacing > 0
        If e.NewValue IsNot Nothing AndAlso CDbl(e.NewValue) > 0 Then
            AddHandler panel.Loaded, AddressOf Panel_Loaded
            AddHandler panel.Unloaded, AddressOf Panel_Unloaded
            
            ' If already loaded, set up immediately
            If panel.IsLoaded Then
                AttachCollectionChangedHandler(panel)
                UpdateChildSpacing(panel)
            End If
        Else
            ' Reset all margins if spacing is 0
            ResetChildMargins(panel)
        End If
    End Sub

    Private Shared Sub Panel_Loaded(sender As Object, e As RoutedEventArgs)
        Dim panel = TryCast(sender, StackPanel)
        If panel IsNot Nothing Then
            AttachCollectionChangedHandler(panel)
            UpdateChildSpacing(panel)
        End If
    End Sub

    Private Shared Sub Panel_Unloaded(sender As Object, e As RoutedEventArgs)
        Dim panel = TryCast(sender, StackPanel)
        If panel IsNot Nothing Then
            RemoveHandlers(panel)
        End If
    End Sub

    Private Shared Sub AttachCollectionChangedHandler(panel As StackPanel)
        Dim children = TryCast(panel.Children, INotifyCollectionChanged)
If children IsNot Nothing Then
            Dim handler As NotifyCollectionChangedEventHandler = 
                Sub(s, args) UpdateChildSpacing(panel)
            
            ' Store the handler for later removal
            _panelHandlers(panel) = handler
            AddHandler children.CollectionChanged, handler
        End If
    End Sub

    Private Shared Sub RemoveHandlers(panel As StackPanel)
        RemoveHandler panel.Loaded, AddressOf Panel_Loaded
        RemoveHandler panel.Unloaded, AddressOf Panel_Unloaded
        
        ' Remove collection changed handler if it exists
        If _panelHandlers.ContainsKey(panel) Then
            Dim children = TryCast(panel.Children, INotifyCollectionChanged)
            If children IsNot Nothing Then
                RemoveHandler children.CollectionChanged, _panelHandlers(panel)
            End If
            _panelHandlers.Remove(panel)
        End If
    End Sub

    Private Shared Sub UpdateChildSpacing(panel As StackPanel)
        If panel Is Nothing OrElse Not panel.IsLoaded Then Return

        Dim spacing = GetSpacing(panel)
        Dim childCount = panel.Children.Count

        For i As Integer = 0 To childCount - 1
            Dim child = TryCast(panel.Children(i), FrameworkElement)
            If child Is Nothing Then Continue For

            If panel.Orientation = Orientation.Vertical Then
                ' Vertical - only bottom margin (except last item)
                child.Margin = New Thickness(0, 0, 0, If(i < childCount - 1, spacing, 0))
            Else
                ' Horizontal - only right margin (except last item)
                child.Margin = New Thickness(0, 0, If(i < childCount - 1, spacing, 0), 0)
            End If
        Next
    End Sub

    Private Shared Sub ResetChildMargins(panel As StackPanel)
        If panel Is Nothing OrElse Not panel.IsLoaded Then Return

        For Each child In panel.Children
            Dim fe = TryCast(child, FrameworkElement)
            If fe IsNot Nothing Then
                fe.Margin = New Thickness(0)
            End If
        Next
    End Sub
End Class