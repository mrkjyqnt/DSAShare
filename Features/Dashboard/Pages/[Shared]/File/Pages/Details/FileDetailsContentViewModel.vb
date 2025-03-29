Imports System.Collections.ObjectModel
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports System.IO
Imports System.Windows.Media.Imaging
Imports ICSharpCode.AvalonEdit.Highlighting
Imports Spire.Doc
Imports Prism.Commands

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
    Private ReadOnly _fileService As IFileService
    Private ReadOnly _sessionManager As ISessionManager

    Public ReadOnly Property DownloadCommand As AsyncDelegateCommand

    Private _removeAccessButtonEnabled As Visibility = Visibility.Collapsed
    Private _downloadButtonVisibility As Visibility = Visibility.Collapsed

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

    Public Property RemoveAccessButtonVisibility As Visibility
        Get
            Return _removeAccessButtonEnabled
        End Get
        Set(value As Visibility)
            SetProperty(_removeAccessButtonEnabled, value)
        End Set
    End Property

    Public Property DownloadButtonVisibility As Visibility
        Get
            Return _downloadButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_downloadButtonVisibility, value)
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

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Sub New(fileDataService As IFileDataService, fileService As IFileService, sessionManager As ISessionManager)
        _fileDataService = fileDataService
        _fileService = fileService
        _sessionManager = sessionManager
        _dataGridFileDetails = New FileDetailsContentModel()

        DownloadCommand = New AsyncDelegateCommand(AddressOf OnDownload)
    End Sub

    Private Async Sub Load()
        Try
            _dataGridFileDetails.Author = If(_file?.UploadedBy, "Unknown")
            _dataGridFileDetails.FileName = If(_file?.FileName, "Unknown")
            _dataGridFileDetails.AccessLevel = If(_file?.Privacy, "Unknown")
            _dataGridFileDetails.DownloadCount = If(_file?.DownloadCount, 0)
            _dataGridFileDetails.PublishDate = If(_file?.CreatedAt, DateTime.MinValue)
            _dataGridFileDetails.ExpirationDate = _file?.ExpiryDate
            _dataGridFileDetails.FileType = If(_file?.FileType, "Unknown")

            DescriptionText = If(_file?.FileDescription, "No description available.")

            ChangeButtonVisibility()
            RaisePropertyChanged(NameOf(DataGridFileDetails))
            Await LoadPreviewAsync().ConfigureAwait(True)
        Catch ex As Exception
            Debug.WriteLine($"Error loading file details: {ex.Message}")
        End Try
    End Sub

    Private Async Function LoadPreviewAsync() As Task
        Try
            Dim extension = Path.GetExtension(_filePath).ToLower()
            Dim previewType = GetPreviewTypeForExtension(extension)

#Disable Warning
            Await Application.Current.Dispatcher.InvokeAsync(Sub()
                                                                 CurrentPreviewType = previewType
                                                                 RaisePropertyChanged(NameOf(CurrentPreviewType))
                                                             End Sub)
#Enable Warning

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
                                                End Function).ConfigureAwait(True)
                    PreviewContent = bitmap

                Case PreviewTypes.Document
                    Dim imageSource = Await DocumentToImageHelper.ConvertFirstPageToImageAsync(_filePath).ConfigureAwait(True)
                    PreviewContent = imageSource

                Case PreviewTypes.Text
                    Dim text = Await Task.Run(Function() File.ReadAllText(_filePath)).ConfigureAwait(True)
                    PreviewContent = text
                Case PreviewTypes.Unsupported
                    PreviewContent = "Unsupported file format."
            End Select

            RaisePropertyChanged(NameOf(PreviewContent))

        Catch ex As Exception
            CurrentPreviewType = PreviewTypes.Unsupported
            RaisePropertyChanged(NameOf(CurrentPreviewType))
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

    Public Async Function OnDownload() As Task
        Try
            Loading.Show()
            Dim result = Await Task.Run(Function() _fileService.DownloadFile(_file)).ConfigureAwait(True)

            If result.Success Then
                Await PopUp.Information("Success", result.Message).ConfigureAwait(True)
            Else
                Await PopUp.Information("Failed", result.Message).ConfigureAwait(True)
                Return
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error downloading file: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Sub ChangeButtonVisibility()
        Try
            If _sessionManager.CurrentUser.Id = _file.UploadedBy Then
                DownloadButtonVisibility = Visibility.Visible
                Return
            End If

            If Not _sessionManager.CurrentUser.Id = _file.UploadedBy Then
                DownloadButtonVisibility = Visibility.Visible
                RemoveAccessButtonVisibility = Visibility.Visible
                Return
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error changing button visibility: {ex.Message}")
        End Try
    End Sub

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo
        Try
            Loading.Show()
            If navigationContext.Parameters.ContainsKey("fileId") Then
                Dim file = New FilesShared With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("fileId")
                }
                _file = Await Task.Run(Function() _fileDataService.GetFileById(file)).ConfigureAwait(True)

                If String.IsNullOrEmpty(_file?.FileName) Then
                    Await PopUp.Information("Error", "File not found").ConfigureAwait(True)
                    Return
                End If

                _filePath = _file.FilePath
                Application.Current.Dispatcher.Invoke(Sub()
                                                          Load()
                                                      End Sub)
            End If
            Loading.Hide()
        Catch ex As Exception
            Debug.WriteLine($"[Debug] Error navigating to FileDetailsContentModel: {ex.Message}")
        End Try
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Throw New NotImplementedException()
    End Function

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedFrom
        Throw New NotImplementedException()
    End Sub
End Class
