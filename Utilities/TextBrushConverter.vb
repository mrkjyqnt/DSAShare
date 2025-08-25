Imports System.Globalization

Public Class TextBrushConverter
    Implements IValueConverter

    Public Function Convert(value As Object,
                            targetType As Type,
                            parameter As Object,
                            culture As CultureInfo) As Object _
                        Implements IValueConverter.Convert

        Dim s = TryCast(value, String)
        If String.IsNullOrEmpty(s) Then
            Return Brushes.Gray
        End If

        Select Case s.Trim().ToLowerInvariant()
            Case "pending"
                Return Brushes.Orange
            Case "resolved"
                Return Brushes.green
            Case Else
                Return Brushes.Gray
        End Select
    End Function

    Public Function ConvertBack(value As Object,
                                targetType As Type,
                                parameter As Object,
                                culture As CultureInfo) As Object _
                            Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

