Public Class DocumentPreviewHelper
    Inherits DependencyObject

    Public Shared ReadOnly DocumentSourceProperty As DependencyProperty =
        DependencyProperty.RegisterAttached(
            "DocumentSource",
            GetType(Uri),
            GetType(DocumentPreviewHelper),
            New PropertyMetadata(Nothing, AddressOf OnDocumentSourceChanged))

    Public Shared Sub SetDocumentSource(obj As WebBrowser, value As Uri)
        obj.SetValue(DocumentSourceProperty, value)
    End Sub

    Public Shared Function GetDocumentSource(obj As WebBrowser) As Uri
        Return DirectCast(obj.GetValue(DocumentSourceProperty), Uri)
    End Function

    Private Shared Sub OnDocumentSourceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim browser = TryCast(d, WebBrowser)
        If browser IsNot Nothing AndAlso e.NewValue IsNot Nothing Then
            ' Ensure navigation happens on the UI thread
            browser.Dispatcher.BeginInvoke(Sub()
                                               Try
                                                   browser.Source = DirectCast(e.NewValue, Uri)
                                               Catch ex As Exception
                                                   Debug.WriteLine($"Document navigation error: {ex.Message}")
                                               End Try
                                           End Sub)
        End If
    End Sub
End Class
