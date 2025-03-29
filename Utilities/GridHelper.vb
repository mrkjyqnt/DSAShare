Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Public Class GridHelper
    ' DependencyProperty for Column Spacing
    Public Shared ReadOnly ColumnSpacingProperty As DependencyProperty =
        DependencyProperty.RegisterAttached("ColumnSpacing", GetType(Double), GetType(GridHelper), New PropertyMetadata(0.0, AddressOf OnSpacingChanged))

    ' DependencyProperty for Row Spacing
    Public Shared ReadOnly RowSpacingProperty As DependencyProperty =
        DependencyProperty.RegisterAttached("RowSpacing", GetType(Double), GetType(GridHelper), New PropertyMetadata(0.0, AddressOf OnSpacingChanged))

    ' Setters & Getters
    Public Shared Sub SetColumnSpacing(ByVal element As DependencyObject, ByVal value As Double)
        element.SetValue(ColumnSpacingProperty, value)
    End Sub

    Public Shared Function GetColumnSpacing(ByVal element As DependencyObject) As Double
        Return CType(element.GetValue(ColumnSpacingProperty), Double)
    End Function

    Public Shared Sub SetRowSpacing(ByVal element As DependencyObject, ByVal value As Double)
        element.SetValue(RowSpacingProperty, value)
    End Sub

    Public Shared Function GetRowSpacing(ByVal element As DependencyObject) As Double
        Return CType(element.GetValue(RowSpacingProperty), Double)
    End Function

    ' Apply spacing when properties change
    Private Shared Sub OnSpacingChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim grid As Grid = TryCast(d, Grid)
        If grid Is Nothing Then Return

        ' Apply spacing to all children
        AddHandler grid.Loaded, Sub(sender, args)
                                    ApplySpacing(grid)
                                End Sub
    End Sub

    ' Method to apply spacing
    Private Shared Sub ApplySpacing(grid As Grid)
        Dim columnSpacing As Double = GetColumnSpacing(grid)
        Dim rowSpacing As Double = GetRowSpacing(grid)

        For Each child As UIElement In grid.Children
            If TypeOf child Is FrameworkElement Then
                Dim fe As FrameworkElement = CType(child, FrameworkElement)
                Dim margin As Thickness = fe.Margin

                ' Get Grid row and column positions
                Dim col As Integer = Grid.GetColumn(fe)
                Dim row As Integer = Grid.GetRow(fe)

                ' Apply spacing (except on the first row/column)
                If col > 0 Then margin.Left = columnSpacing
                If row > 0 Then margin.Top = rowSpacing

                fe.Margin = margin
            End If
        Next
    End Sub
End Class
