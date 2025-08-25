Imports System.Collections.ObjectModel
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports System.IO
Imports ICSharpCode.AvalonEdit.Highlighting
Imports Prism.Commands
Imports Prism.Navigation

#Disable Warning
Public Class ReportDetailsViewModel
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

    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _fileService As IFileService
    Private ReadOnly _reportsService As IReportsService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _userService As IUserService

    Private _fileShared As FilesShared
    Private _reports As Reports
    Private _dataGridFileDetails As FileDetailsContentModel
    Private _descriptionText As String
    Private _currentPreviewType As PreviewTypes
    Private _previewContent As Object
    Private _syntaxHighlighting As IHighlightingDefinition

    Private _filePath As String
    Private _fileNameText As String
    Private _reportDescriptionText As String
    Private _reportCountText As String
    Private _reportStatusText As String
    Private _adminNameText As String
    Private _adminDescriptionText As String

    Private ReadOnly _folderPath As String = ConfigurationModule.GetSettings().Network.FolderPath

    Private _resolvedButtonVisibility As Visibility = Visibility.Collapsed
    Private _unResolvedButtonVisibility As Visibility = Visibility.Collapsed
    Private _resolvedSectionVisibility As Visibility = Visibility.Collapsed

    Public Property DescriptionText As String
        Get
            Return _descriptionText
        End Get
        Set(value As String)
            SetProperty(_descriptionText, value)
        End Set
    End Property

    Public Property FileNameText As String
        Get
            Return _fileNameText
        End Get
        Set(value As String)
            SetProperty(_fileNameText, value)
        End Set
    End Property

    Public Property ReportDescriptionText As String
        Get
            Return _reportDescriptionText
        End Get
        Set(value As String)
            SetProperty(_reportDescriptionText, value)
        End Set
    End Property

    Public Property ReportCountText As String
        Get
            Return _reportCountText
        End Get
        Set(value As String)
            SetProperty(_reportCountText, value)
        End Set
    End Property

    Public Property ReportStatusText As String
        Get
            Return _reportStatusText
        End Get
        Set(value As String)
            SetProperty(_reportStatusText, value)
        End Set
    End Property
    Public Property AdminNameText As String
        Get
            Return _adminNameText
        End Get
        Set(value As String)
            SetProperty(_adminNameText, value)
        End Set
    End Property

    Public Property AdminDescriptionText As String
        Get
            Return _adminDescriptionText
        End Get
        Set(value As String)
            SetProperty(_adminDescriptionText, value)
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

    Public Property ResolvedButtonVisibility As Visibility
        Get
            Return _resolvedButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_resolvedButtonVisibility, value)
        End Set
    End Property
    Public Property UnResolvedButtonVisibility As Visibility
        Get
            Return _unResolvedButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_unResolvedButtonVisibility, value)
        End Set
    End Property

    Public Property ResolvedSectionVisibility As Visibility
        Get
            Return _resolvedSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_resolvedSectionVisibility, value)
        End Set
    End Property

    Public ReadOnly Property DataGridFileDetails As ObservableCollection(Of KeyValuePair(Of String, String))
        Get
            If _dataGridFileDetails Is Nothing Then Return Nothing
            Return New ObservableCollection(Of KeyValuePair(Of String, String)) From {
                New KeyValuePair(Of String, String)("Author", _dataGridFileDetails.Author),
                New KeyValuePair(Of String, String)("File", _dataGridFileDetails.FileName),
                New KeyValuePair(Of String, String)("Publish Date", _dataGridFileDetails.PublishDate.ToString()),
                New KeyValuePair(Of String, String)("Availability", _dataGridFileDetails.FileType)
            }
        End Get
    End Property

    Public ReadOnly Property BackCommand As DelegateCommand
    Public ReadOnly Property ViewCommand As AsyncDelegateCommand
    Public ReadOnly Property ResolvedCommand As AsyncDelegateCommand

    Public Sub New(fileDataService As IFileDataService,
                   fileService As IFileService,
                   reportsService As IReportsService,
                   sessionManager As ISessionManager,
                   activityService As IActivityService,
                   navigationService As INavigationService,
                   userService As IUserService)
        _fileDataService = fileDataService
        _fileService = fileService
        _reportsService = reportsService
        _sessionManager = sessionManager
        _dataGridFileDetails = New FileDetailsContentModel()
        _activityService = activityService
        _navigationService = navigationService
        _userService = userService

        BackCommand = New DelegateCommand(AddressOf OnBack)
        ViewCommand = New AsyncDelegateCommand(AddressOf OnView)
        ResolvedCommand = New AsyncDelegateCommand(AddressOf OnResolved)
    End Sub

    Private Sub OnBack()
        _navigationService.GoBack()
    End Sub

    Private Async Function OnView() As Task
        Try
            Dim parameters As New NavigationParameters()
            parameters.Add("fileId", _fileShared.Id)
            parameters.Add("openedFrom", "ManageFilesView")

            _navigationService.Go("PageRegion", "FileDetailsView", "Reports", parameters)
        Catch ex As Exception
            Debug.WriteLine("[ReportDetailsViewModel] OnView Error: " & ex.Message)
        End Try
    End Function

    Private Async Function OnResolved() As Task
        Try
            Loading.Show()
            Await Task.Delay(100)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            Dim popUpResult As PopupResult = Await PopUp.Description()


            If popUpResult Is Nothing Then
                Await PopUp.Information("Cancelled", "File resolving was cancelled.").ConfigureAwait(True)
                Exit Function
            Else
                _reports.AdminId = _sessionManager.CurrentUser.Id
                _reports.Status = "Resolved"
                _reports.AdminDescription = popUpResult.GetValue(Of String)("Input")

                Dim _reported = Await Task.Run(Function() _reportsService.UpdateReport(_reports)).ConfigureAwait(True)

                If _reported Then
                    Await PopUp.Information("Success", "Report has been resolved")
                    _navigationService.GoBack()
                Else
                    Await PopUp.Information("Failed", "Report resolving failed.")
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"[FileDetailsContentViewModel] OnResolved Error: {ex.Message}")
        Finally
            ChangeVisibility()
            Loading.Hide()
        End Try
    End Function

    Private Async Sub Load()
        Try
            Dim author = Await Task.Run(Function() _userService.GetUserById(New Users With {.Id = _fileShared.UploadedBy})).ConfigureAwait(False)
            Dim admin = Await Task.Run(Function() _userService.GetUserById(New Users With {.Id = _reports?.AdminId})).ConfigureAwait(False)

            _dataGridFileDetails.Author = If(author?.Name, "Unknown")
            _dataGridFileDetails.FileName = If(_fileShared?.FileName, "Unknown")
            _dataGridFileDetails.PublishDate = If(_fileShared?.CreatedAt, DateTime.Now)
            _dataGridFileDetails.FileType = If(_fileShared?.FileType, "Unknown")

            FileNameText = _fileShared.FileName
            ReportDescriptionText = _reports?.ReporterDescription
            ReportStatusText = _reports?.Status
            AdminNameText = admin?.Name
            AdminDescriptionText = _reports?.AdminDescription

            DescriptionText = If(_fileShared?.FileDescription, "No description available.")

            ChangeVisibility()
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

            Await Application.Current.Dispatcher.InvokeAsync(Sub()
                                                                 CurrentPreviewType = previewType
                                                                 RaisePropertyChanged(NameOf(CurrentPreviewType))
                                                             End Sub)

            Select Case previewType
                Case PreviewTypes.Image
                    Dim bitmap = Await Task.Run(Function()
                                                    Dim img = New BitmapImage()
                                                    img.BeginInit()
                                                    img.UriSource = New Uri(Path.Combine(_folderPath, _filePath))
                                                    img.CacheOption = BitmapCacheOption.OnLoad
                                                    img.EndInit()
                                                    If img.CanFreeze Then img.Freeze()
                                                    Return img
                                                End Function).ConfigureAwait(False)
                    PreviewContent = bitmap

                Case PreviewTypes.Document
                    Dim imageSource = Await DocumentToImageHelper.ConvertFirstPageToImageAsync(Path.Combine(_folderPath, _filePath)).ConfigureAwait(False)
                    PreviewContent = imageSource

                Case PreviewTypes.Text
                    Dim text = Await Task.Run(Function() File.ReadAllText(Path.Combine(_folderPath, _filePath))).ConfigureAwait(False)
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

    Private Sub ChangeVisibility()
        Try
            If _reports.Status = "Pending" Then
                UnResolvedButtonVisibility = Visibility.Visible
            End If

            If _reports.Status = "Resolved" Then
                ResolvedSectionVisibility = Visibility.Visible
                ResolvedButtonVisibility = Visibility.Visible
                UnResolvedButtonVisibility = Visibility.Collapsed
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error changing button visibility: {ex.Message}")
        End Try
    End Sub

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo
        Try
            Loading.Show()
            Await Task.Delay(100)

            If Not Await Fallback.CheckConnection() Then
                Exit Try
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Exit Try
            End If

            If Not navigationContext.Parameters.ContainsKey("fileId") Then
                Await PopUp.Information("Failed", "File was empty").ConfigureAwait(True)
                _navigationService.GoBack()
                Exit Try
            End If

            If Not navigationContext.Parameters.ContainsKey("reportId") Then
                Await PopUp.Information("Failed", "Report was empty").ConfigureAwait(True)
                _navigationService.GoBack()
                Exit Try
            End If

            If navigationContext.Parameters.ContainsKey("fileId") AndAlso
                navigationContext.Parameters.ContainsKey("reportId") Then

                Dim fileShared = New FilesShared With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("fileId")
                }

                _fileShared = Await Task.Run(Function() _fileDataService?.GetSharedFileById(fileShared)).ConfigureAwait(False)
                _reports = Await Task.Run(Function() _reportsService?.GetReportById(navigationContext.Parameters.GetValue(Of Integer)("reportId"))).ConfigureAwait(False)
                ReportCountText = Await Task.Run(Function() _reportsService?.GetReportsByFileId(_reports.FileId).Count).ConfigureAwait(False)

                If String.IsNullOrEmpty(_fileShared?.FileName) Then
                    Await PopUp.Information("Error", "Shared File not found").ConfigureAwait(True)
                    Exit Try
                End If

                _filePath = _fileShared.FilePath
                Await Application.Current.Dispatcher.InvokeAsync(Sub() Load()).Task.ConfigureAwait(False)
            Else
                Await PopUp.Information("Failed", "This report has been removed.").ConfigureAwait(True)
                _navigationService.GoBack()
            End If
        Catch ex As Exception
            Debug.WriteLine($"[Debug] Error navigating to FileDetailsContentModel: {ex.Message}")
            _navigationService.GoBack()
        Finally
            Loading.Hide()
        End Try
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Return False
    End Function

    Public Async Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedFrom
    End Sub

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property
End Class
