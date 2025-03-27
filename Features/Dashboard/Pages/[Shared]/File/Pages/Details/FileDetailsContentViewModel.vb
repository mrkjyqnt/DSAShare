Imports System.Collections.ObjectModel
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports System.IO
Imports System.Windows.Media.Imaging
Imports ICSharpCode.AvalonEdit.Highlighting
Imports Spire.Doc

Public Class FileDetailsContentViewModel
    Inherits BindableBase
    Implements INavigationAware
    Implements IRegionMemberLifetime

    Public Enum PreviewTypes
        None
        Image
        Document
        Text
        Unsupported
    End Enum

    Private _file As FilesShared
    Private _dataGridFileDetails As FileDetailsContentModel
    Private _descriptionText As String
    Private _currentPreviewType As PreviewTypes
    Private _previewContent As Object
    Private _syntaxHighlighting As IHighlightingDefinition
    Private _filePath As String

    Private ReadOnly _fileDataService As IFileDataService

    Public Property DescriptionText As String
        Get
            Return _descriptionText
        End Get
        Set(value As String)
            SetProperty(_descriptionText, value)
        End Set
    End Property

    Public Property CurrentPreviewType As PreviewTypes
        Get
            Return _currentPreviewType
        End Get
        Set(value As PreviewTypes)
            SetProperty(_currentPreviewType, value)
        End Set
    End Property

    Public Property PreviewContent As Object
        Get
            Return _previewContent
        End Get
        Set(value As Object)
            SetProperty(_previewContent, value)
        End Set
    End Property

    Public Property SyntaxHighlighting As IHighlightingDefinition
        Get
            Return _syntaxHighlighting
        End Get
        Set(value As IHighlightingDefinition)
            SetProperty(_syntaxHighlighting, value)
        End Set
    End Property

    Public ReadOnly Property DataGridFileDetails As ObservableCollection(Of KeyValuePair(Of String, String))
        Get
            If _dataGridFileDetails Is Nothing Then Return Nothing
            Return New ObservableCollection(Of KeyValuePair(Of String, String)) From {
                New KeyValuePair(Of String, String)("Author", _dataGridFileDetails.Author),
                New KeyValuePair(Of String, String)("File Name", _dataGridFileDetails.FileName),
                New KeyValuePair(Of String, String)("Access Level", _dataGridFileDetails.AccessLevel),
                New KeyValuePair(Of String, String)("Download Count", _dataGridFileDetails.DownloadCount.ToString()),
                New KeyValuePair(Of String, String)("Publish Date", _dataGridFileDetails.PublishDate.ToString()),
                New KeyValuePair(Of String, String)("Expiration Date", If(_dataGridFileDetails.ExpirationDate?.ToString(), "N/A")),
                New KeyValuePair(Of String, String)("File Type", _dataGridFileDetails.FileType)
            }
        End Get
    End Property

    Public Sub New(fileDataService As IFileDataService)
        _fileDataService = fileDataService
        _dataGridFileDetails = New FileDetailsContentModel()
    End Sub

    Private Sub Load()
        Try
            _dataGridFileDetails.Author = If(_file?.UploadedBy, "Nothing")
            _dataGridFileDetails.FileName = If(_file?.FileName, "Nothing")
            _dataGridFileDetails.AccessLevel = If(_file?.Privacy, "Nothing")
            _dataGridFileDetails.DownloadCount = If(_file?.DownloadCount, 0)
            _dataGridFileDetails.PublishDate = If(_file?.CreatedAt, DateTime.MinValue)
            _dataGridFileDetails.ExpirationDate = _file?.ExpiryDate
            _dataGridFileDetails.FileType = If(_file?.FileType, "Nothing")

            DescriptionText = If(_file?.FileDescription, "There no descriptions")

            RaisePropertyChanged(NameOf(DataGridFileDetails))
        Catch ex As Exception
            Debug.WriteLine($"Error loading file details: {ex.Message}")
        End Try
    End Sub

#Disable Warning
    Private Async Function LoadPreviewAsync() As Task
        Try
            Dim extension = Path.GetExtension(_filePath).ToLower()
            Dim previewType = GetPreviewTypeForExtension(extension)

            ' Update UI on UI thread
            Await Application.Current.Dispatcher.InvokeAsync(Sub()
                                                                 CurrentPreviewType = previewType
                                                                 RaisePropertyChanged(NameOf(CurrentPreviewType))
                                                             End Sub)

            ' Load content based on type
            Select Case previewType
                Case PreviewTypes.Image
                    Dim bitmap = Await Task.Run(Function()
                                                    Dim img = New BitmapImage()
                                                    img.BeginInit()
                                                    img.UriSource = New Uri(_filePath)
                                                    img.CacheOption = BitmapCacheOption.OnLoad
                                                    img.EndInit()
                                                    If img.CanFreeze Then img.Freeze()
                                                    Return img
                                                End Function)
                    PreviewContent = bitmap

                Case PreviewTypes.Document
                    Dim imageSource = Await DocumentToImageHelper.ConvertFirstPageToImageAsync(_filePath)
                    PreviewContent = imageSource

                Case PreviewTypes.Text
                    Dim text = Await Task.Run(Function() File.ReadAllText(_filePath))
                    PreviewContent = text
                Case PreviewTypes.Unsupported
                    Dim text = New Object()
                    PreviewContent = text
            End Select

            RaisePropertyChanged(NameOf(PreviewContent))

        Catch ex As Exception
            Application.Current.Dispatcher.BeginInvoke(Sub()
                                                           CurrentPreviewType = PreviewTypes.Unsupported
                                                           RaisePropertyChanged(NameOf(CurrentPreviewType))
                                                       End Sub)
            Debug.WriteLine($"Preview load error: {ex.Message}")
        End Try
    End Function

    Private Function GetPreviewTypeForExtension(extension As String) As PreviewTypes
        Select Case extension
            Case ".jpg", ".jpeg", ".png", ".bmp", ".gif"
                Return PreviewTypes.Image
            Case ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx"
                Return PreviewTypes.Document
            Case ".txt", ".xml", ".json", ".csv", ".log", ".vb", ".cs"
                Return PreviewTypes.Text
            Case Else
                Return PreviewTypes.Unsupported
        End Select
    End Function

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo
        Try
            Loading.Show()
            If navigationContext.Parameters.ContainsKey("fileId") Then
                Dim file = New FilesShared With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("fileId")
                }
                _file = Await Task.Run(Function() _fileDataService.GetFileById(file)).ConfigureAwait(True)
                If IsNullOrEmpty(_file?.FileName) Then
                    PopUp.Information("Error", "File not found")
                    Return
                End If

                _filePath = _file.FilePath
                Load()
                Await LoadPreviewAsync().ConfigureAwait(True)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error navigating to FileDetailsContentModel: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Return False
    End Function

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedFrom
        ' Cleanup if needed
    End Sub

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property
End Class
