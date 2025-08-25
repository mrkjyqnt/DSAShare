Imports System.Globalization

Public Class IntToVisibilityConverter
    Implements IValueConverter
    
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim count = System.Convert.ToInt32(value)
        Dim isInverse = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase)
        
        If isInverse Then
            Return If(count > 0, Visibility.Collapsed, Visibility.Visible)
        Else
            Return If(count > 0, Visibility.Visible, Visibility.Collapsed)
        End If
    End Function
    
    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class