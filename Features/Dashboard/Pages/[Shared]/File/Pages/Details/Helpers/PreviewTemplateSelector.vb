Public Class PreviewTemplateSelector
    Inherits DataTemplateSelector

    Public Property ImageTemplate As DataTemplate
    Public Property DocumentTemplate As DataTemplate
    Public Property TextTemplate As DataTemplate
    Public Property UnsupportedTemplate As DataTemplate

    Public Overrides Function SelectTemplate(item As Object, container As DependencyObject) As DataTemplate
        Try
            Dim viewModel = TryCast(item, FileDetailsContentViewModel)
            If viewModel Is Nothing Then Return UnsupportedTemplate
            
            Select Case viewModel.CurrentPreviewType
                Case FileDetailsContentViewModel.PreviewTypes.Image
                    Return ImageTemplate
                    
                Case FileDetailsContentViewModel.PreviewTypes.Document
                    ' Use the same template for documents and images since we're converting to images
                    Return ImageTemplate
                    
                Case FileDetailsContentViewModel.PreviewTypes.Text
                    Return TextTemplate
                    
                Case Else
                    Return UnsupportedTemplate
            End Select
            
        Catch ex As Exception
            Return UnsupportedTemplate
        End Try
    End Function
End Class