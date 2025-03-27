Imports System
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Data

''' <summary>
''' Converts a null value to a <see cref="Visibility"/> value.
''' </summary>
Public Class NullToVisibilityConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        ' If the bound value is Nothing, return Visible (i.e. still loading)
        Return If(value Is Nothing, Visibility.Visible, Visibility.Collapsed)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
