Imports System.Globalization

Public Class NullToVisibilityConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        ' Handle null case first
        If value Is Nothing Then Return Visibility.Visible

        ' Handle empty collections
        If TypeOf value Is IEnumerable Then
            ' Optimized check for ICollection first (faster for most cases)
            If TypeOf value Is ICollection Then
                Return If(DirectCast(value, ICollection).Count = 0, Visibility.Visible, Visibility.Collapsed)
            End If

            ' Fallback for other IEnumerable types
            Dim enumerator = DirectCast(value, IEnumerable).GetEnumerator()
            Try
                Return If(enumerator.MoveNext(), Visibility.Collapsed, Visibility.Visible)
            Finally
                If TypeOf enumerator Is IDisposable Then
                    DirectCast(enumerator, IDisposable).Dispose()
                End If
            End Try
        End If

        ' Default case for non-null, non-enumerable objects
        Return Visibility.Collapsed
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class