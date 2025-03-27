Imports System.IO
Imports System.Windows.Media.Imaging
Imports ICSharpCode.AvalonEdit
Imports ICSharpCode.AvalonEdit.Highlighting

Public Class FilePreviewService
    Implements IFilePreviewService

    Private _imagePreview As Image
    Private _documentPreview As WebBrowser
    Private _textPreview As TextEditor
    Private _unsupportedPreview As TextBlock

    Public Sub New(imagePreview As Image, 
                  documentPreview As WebBrowser, 
                  textPreview As TextEditor,
                  unsupportedPreview As TextBlock)
        _imagePreview = imagePreview
        _documentPreview = documentPreview
        _textPreview = textPreview
        _unsupportedPreview = unsupportedPreview
    End Sub

    Public Sub LoadPreview(filePath As String, fileType As String) Implements IFilePreviewService.LoadPreview
        If String.IsNullOrEmpty(filePath) OrElse Not File.Exists(filePath) Then
            ShowUnsupportedPreview()
            Return
        End If

        Try
            Select Case fileType.ToLower()
                Case "jpg", "jpeg", "png", "bmp", "gif"
                    LoadImagePreview(filePath)
                Case "pdf"
                    LoadPdfPreview(filePath)
                Case "doc", "docx", "xls", "xlsx", "ppt", "pptx"
                    LoadDocumentPreview(filePath)
                Case "txt", "xml", "json", "csv", "log", "vb", "cs"
                    LoadTextPreview(filePath)
                Case Else
                    ShowUnsupportedPreview()
            End Select
        Catch ex As Exception
            Debug.WriteLine($"Error loading preview: {ex.Message}")
            ShowUnsupportedPreview()
        End Try
    End Sub

    Public Sub ClearPreview() Implements IFilePreviewService.ClearPreview
        HideAllPreviews()
    End Sub

    Private Sub LoadImagePreview(filePath As String)
        HideAllPreviews()
        Try
            Dim bitmap = New BitmapImage()
            bitmap.BeginInit()
            bitmap.UriSource = New Uri(filePath)
            bitmap.CacheOption = BitmapCacheOption.OnLoad
            bitmap.EndInit()
            _imagePreview.Source = bitmap
            _imagePreview.Visibility = Visibility.Visible
        Catch ex As Exception
            ShowUnsupportedPreview()
        End Try
    End Sub

    Private Sub LoadPdfPreview(filePath As String)
        HideAllPreviews()
        Try
            ' Using WebBrowser control for PDF preview
            _documentPreview.Navigate(filePath)
            _documentPreview.Visibility = Visibility.Visible
        Catch ex As Exception
            ShowUnsupportedPreview()
        End Try
    End Sub

    Private Sub LoadDocumentPreview(filePath As String)
        HideAllPreviews()
        Try
            ' Using WebBrowser control for Office docs
            _documentPreview.Navigate(filePath)
            _documentPreview.Visibility = Visibility.Visible
        Catch ex As Exception
            ShowUnsupportedPreview()
        End Try
    End Sub

    Private Sub LoadTextPreview(filePath As String)
        HideAllPreviews()
        Try
            _textPreview.Load(filePath)
            
            ' Set syntax highlighting based on file extension
            Dim ext = Path.GetExtension(filePath).ToLower()
            _textPreview.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(ext)
            
            _textPreview.Visibility = Visibility.Visible
        Catch ex As Exception
            ShowUnsupportedPreview()
        End Try
    End Sub

    Private Sub HideAllPreviews()
        _imagePreview.Visibility = Visibility.Collapsed
        _documentPreview.Visibility = Visibility.Collapsed
        _textPreview.Visibility = Visibility.Collapsed
        _unsupportedPreview.Visibility = Visibility.Collapsed
    End Sub

    Private Sub ShowUnsupportedPreview()
        HideAllPreviews()
        _unsupportedPreview.Visibility = Visibility.Visible
    End Sub
End Class