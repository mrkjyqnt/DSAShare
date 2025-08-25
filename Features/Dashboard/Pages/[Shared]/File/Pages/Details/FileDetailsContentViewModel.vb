Imports System.Collections.ObjectModel
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports System.IO
Imports System.Windows.Media.Imaging
Imports ICSharpCode.AvalonEdit.Highlighting
Imports Spire.Doc
Imports Prism.Commands

#Disable Warning
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

    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _fileService As IFileService
    Private ReadOnly _fileBlockService As IFileBlockService
    Private ReadOnly _reportsService As IReportsService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _userService As IUserService

    Private _openedFrom As String
    Private _fileShared As FilesShared
    Private _reports As Reports
    Private _fileAccessed As FilesAccessed
    Private _dataGridFileDetails As FileDetailsContentModel
    Private _descriptionText As String
    Private _currentPreviewType As PreviewTypes
    Private _previewContent As Object
    Private _syntaxHighlighting As IHighlightingDefinition
    Private _filePath As String
    Private _shareTypeText As String
    Private _shareValueText As String
    Private ReadOnly _folderPath As String = ConfigurationModule.GetSettings().Network.FolderPath

    Private _accessButtonVisibility As Visibility = Visibility.Collapsed
    Private _downloadButtonVisibility As Visibility = Visibility.Collapsed
    Private _removeAccessButtonVisibility As Visibility = Visibility.Collapsed
    Private _saveAccessButtonVisibility As Visibility = Visibility.Collapsed
    Private _encryptionSectionVisibility As Visibility = Visibility.Collapsed
    Private _reportButtonVisibility As Visibility = Visibility.Collapsed
    Private _reportedButtonVisibility As Visibility = Visibility.Collapsed

    Public Property DescriptionText As String
        Get
            Return _descriptionText
        End Get
        Set(value As String)
            SetProperty(_descriptionText, value)
        End Set
    End Property

    Public Property ShareTypeText As String
        Get
            Return _shareTypeText
        End Get
        Set(value As String)
            SetProperty(_shareTypeText, value)
        End Set
    End Property

    Public Property ShareValueText As String
        Get
            Return _shareValueText
        End Get
        Set(value As String)
            SetProperty(_shareValueText, value)
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

    Public Property AccessButtonVisibility As Visibility
        Get
            Return _accessButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_accessButtonVisibility, value)
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

    Public Property SaveAccessButtonVisibility As Visibility
        Get
            Return _saveAccessButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_saveAccessButtonVisibility, value)
        End Set
    End Property

    Public Property RemoveAccessButtonVisibility As Visibility
        Get
            Return _removeAccessButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_removeAccessButtonVisibility, value)
        End Set
    End Property

    Public Property EncryptionSectionVisibility As Visibility
        Get
            Return _encryptionSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_encryptionSectionVisibility, value)
        End Set
    End Property
    Public Property ReportButtonVisibility As Visibility
        Get
            Return _reportButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_reportButtonVisibility, value)
        End Set
    End Property
    Public Property ReportedButtonVisibility As Visibility
        Get
            Return _reportedButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_reportedButtonVisibility, value)
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
                New KeyValuePair(Of String, String)("Availability", _dataGridFileDetails.Availability),
                New KeyValuePair(Of String, String)("File Type", _dataGridFileDetails.FileType)
            }
        End Get
    End Property

    Public ReadOnly Property DownloadCommand As AsyncDelegateCommand
    Public ReadOnly Property SaveAccessCommand As AsyncDelegateCommand
    Public ReadOnly Property RemoveAccessCommand As AsyncDelegateCommand
    Public ReadOnly Property ReportCommand As AsyncDelegateCommand

    Public Sub New(fileDataService As IFileDataService,
                   fileService As IFileService,
                   fileBlockService As IFileBlockService,
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
        _fileBlockService = fileBlockService

        DownloadCommand = New AsyncDelegateCommand(AddressOf OnDownload)
        SaveAccessCommand = New AsyncDelegateCommand(AddressOf OnSaveAccess)
        RemoveAccessCommand = New AsyncDelegateCommand(AddressOf OnRemoveAccess)
        ReportCommand = New AsyncDelegateCommand(AddressOf OnReport)
    End Sub

    Private Async Sub Load()
        Try
            Dim author = Await Task.Run(Function() _userService.GetUserById(New Users With {.Id = _fileShared.UploadedBy})).ConfigureAwait(False)

            _dataGridFileDetails.Author = If(author?.Name, "Unknown")
            _dataGridFileDetails.FileName = If(_fileShared?.FileName, "Unknown")
            _dataGridFileDetails.AccessLevel = If(_fileShared?.Privacy, "Unknown")
            _dataGridFileDetails.DownloadCount = If(_fileShared?.DownloadCount, 0)
            _dataGridFileDetails.PublishDate = If(_fileShared?.CreatedAt, DateTime.Now)
            _dataGridFileDetails.ExpirationDate = If(_fileShared?.ExpiryDate, Nothing)
            _dataGridFileDetails.Availability = If(_fileShared?.Availability, "Unknown")
            _dataGridFileDetails.FileType = If(_fileShared?.FileType, "Unknown")


            DescriptionText = If(_fileShared?.FileDescription, "No description available.")
            ShareValueText = If(_fileShared?.ShareValue, "N/A")
            ShareTypeText = If(_fileShared?.ShareType, "N/A")

            ChangeVisibility()
            RaisePropertyChanged(NameOf(DataGridFileDetails))
            Await LoadPreviewAsync().ConfigureAwait(True)
        Catch ex As Exception
            Debug.WriteLine($"Error loading file details: {ex.Message}")
        End Try
    End Sub

    Private Async Function LoadPreviewAsync() As Task
        Try
            If String.IsNullOrEmpty(_filePath) Then
                Debug.WriteLine("Error: _filePath is empty")
                CurrentPreviewType = PreviewTypes.Unsupported
                Return
            End If

            If String.IsNullOrEmpty(_folderPath) Then
                Debug.WriteLine("Error: _folderPath is empty")
                CurrentPreviewType = PreviewTypes.Unsupported
                Return
            End If

            Dim fullPath = Path.Combine(_folderPath, _filePath)
            If Not File.Exists(fullPath) Then
                Debug.WriteLine($"Error: File not found at {fullPath}")
                CurrentPreviewType = PreviewTypes.Unsupported
                Return
            End If

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

    Public Async Function OnDownload() As Task
        Try
            Loading.Show()

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            If Not Await CheckFileAvailability() Then
                _navigationService.GoBack()
                Return
            End If

            Dim isBlockResult = Await Task.Run(Function() _fileBlockService.IsBlocked($"{ConfigurationModule.GetSettings().Network.FolderPath}\{_fileShared.FilePath}")).ConfigureAwait(True)

            If isBlockResult Then
                Await PopUp.Information("Warning", "This file is miscellanous, download failed.").ConfigureAwait(True)

                Await Task.Run(Function() _fileService.DeleteFile(_fileShared)).ConfigureAwait(True)

                _navigationService.GoBack()
                Exit Try
            End If

            Dim result = Await Task.Run(Function() _fileService.DownloadFile(_fileShared)).ConfigureAwait(True)

            If result.Success Then
                PopUp.Information("Success", result.Message)
                Load()
            Else
                PopUp.Information("Failed", result.Message)
                Return
            End If
        Catch ex As Exception
            Debug.WriteLine($"[FileDetailsContentViewModel] OnDownload Error: {ex.ToString}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Public Async Function OnSaveAccess() As Task
        Try
            Await RunOnUiAsync(Sub() Loading.Show())
            Await Task.Delay(100)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await RunOnUiAsync(Async Function()
                                       Await PopUp.Information("Warning", "Your account has been banned.")
                                       RestartApplication()
                                   End Function)
                Return
            End If

            If Not Await CheckFileAvailability() Then
                Await RunOnUiAsync(Sub() _navigationService.GoBack())
                Return
            End If

            Dim accessedFile = New FilesAccessed With {
                .UserId = _sessionManager.CurrentUser.Id,
                .FileId = _fileShared.Id,
                .AccessedAt = Date.Now
            }

            Dim _accessedFiles = Await Task.Run(Function() _fileDataService.GetAccessedFileByUserFile(accessedFile))

            If _accessedFiles Is Nothing Then
                If Not Await Task.Run(Function() _fileDataService.AddAccessFile(accessedFile)) Then
                    Await RunOnUiAsync(Sub() PopUp.Information("Failed", "Theres a problem while accessing the file"))
                    Return
                End If

                Dim activity = New Activities With {
                    .Action = "Save Access a file",
                    .ActionIn = "Public Files",
                    .ActionAt = Date.Now,
                    .FileId = _fileShared.Id,
                    .Name = $"{_fileShared.FileName}{_fileShared.FileType}",
                    .UserId = _sessionManager.CurrentUser.Id
                }

                Await Task.Run(Function() _activityService.AddActivity(activity))

                Await RunOnUiAsync(Async Function()
                                       Await PopUp.Information("Success", "File has been added to your Accessed Files")
                                       _fileAccessed = New FilesAccessed With {.FileId = _fileShared.Id}
                                       ChangeVisibility()
                                   End Function)
            End If

        Catch ex As Exception
            Debug.WriteLine($"[FileDetailsContentViewModel] OnSaveAccess Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Public Async Function OnRemoveAccess() As Task
        Try
            Await RunOnUiAsync(Sub() Loading.Show())
            Await Task.Delay(100)

            If Not Await Fallback.CheckConnection() Then Return

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await RunOnUiAsync(Async Function()
                                       Await PopUp.Information("Warning", "Your account has been banned.")
                                       RestartApplication()
                                   End Function)
                Return
            End If

            If Not Await CheckFileAvailability() Then
                Await RunOnUiAsync(Sub() _navigationService.GoBack())
                Return
            End If

            Dim accessedFile = New FilesAccessed With {
                .UserId = _sessionManager.CurrentUser.Id,
                .FileId = _fileShared.Id,
                .AccessedAt = Date.Now
            }

            Dim _accessedFiles = Await Task.Run(Function() _fileDataService.GetAccessedFileByUserFile(accessedFile))

            If _accessedFiles IsNot Nothing Then
                Dim result = Await Task.Run(Function() _fileDataService.RemoveAccessedFile(_accessedFiles))

                If result Then
                    ' Clear the file accessed reference
                    _fileAccessed = Nothing

                    Dim activity = New Activities With {
                        .Action = "Removed Access a file",
                        .ActionIn = "Public Files",
                        .ActionAt = Date.Now,
                        .FileId = _fileShared.Id,
                        .Name = $"{_fileShared.FileName}{_fileShared.FileType}",
                        .UserId = _sessionManager.CurrentUser.Id
                    }

                    Await Task.Run(Function() _activityService.AddActivity(activity))

                    Await RunOnUiAsync(Async Function()
                                           Await PopUp.Information("Success", "Access successfully removed")
                                           ChangeVisibility()
                                       End Function)
                    Return
                End If
            End If

            Await RunOnUiAsync(Async Function()
                                   Await PopUp.Information("Failed", "Access was already removed")
                               End Function)
        Catch ex As Exception
            Debug.WriteLine($"[FileDetailsContentViewModel] OnRemoveAccess Error: {ex.Message}")
        Finally
            ChangeVisibility()
            Loading.Hide()
        End Try
    End Function

    Public Async Function OnReport() As Task
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

            If Not Await CheckFileAvailability() Then
                _navigationService.GoBack()
                Return
            End If

            Dim popUpResult As PopupResult = Await PopUp.Description()

            If popUpResult Is Nothing Then
                Await PopUp.Information("Cancelled", "Report was cancelled.").ConfigureAwait(True)
                Exit Function
            Else
                Dim description = popUpResult.GetValue(Of String)("Input")

                _reports = New Reports With {
                    .FileId = _fileShared.Id,
                    .ReporterId = _sessionManager.CurrentUser.Id,
                    .ReporterDescription = description,
                    .Status = "Pending",
                    .ReportedAt = Date.Now
                }

                Dim reportResult = Await Task.Run(Function() _reportsService.CreateReport(_reports)).ConfigureAwait(True)

                If reportResult Then
                    Await PopUp.Information("Success", "File successfully reported")
                Else
                    Await PopUp.Information("Failed", "File is already reported")
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"[FileDetailsContentViewModel] OnRemoveAccess Error: {ex.Message}")
        Finally
            ChangeVisibility()
            Loading.Hide()
        End Try
    End Function


    Private Sub ChangeVisibility()
        Try
            Application.Current.Dispatcher.Invoke(Sub()
                                                      If _sessionManager.CurrentUser.Id = _fileShared.UploadedBy Then
                                                          DownloadButtonVisibility = Visibility.Visible
                                                          AccessButtonVisibility = Visibility.Collapsed
                                                          SaveAccessButtonVisibility = Visibility.Collapsed
                                                          RemoveAccessButtonVisibility = Visibility.Collapsed

                                                          If _fileShared?.Privacy = "Private" Then
                                                              EncryptionSectionVisibility = Visibility.Visible
                                                          Else
                                                              EncryptionSectionVisibility = Visibility.Collapsed
                                                          End If

                                                          Return
                                                      End If

                                                      If _sessionManager.CurrentUser.Role = "Guest" Then
                                                          DownloadButtonVisibility = Visibility.Visible
                                                          AccessButtonVisibility = Visibility.Collapsed
                                                          SaveAccessButtonVisibility = Visibility.Collapsed
                                                          RemoveAccessButtonVisibility = Visibility.Collapsed
                                                          Return
                                                      End If

                                                      If Not _sessionManager.CurrentUser.Id = _fileShared.UploadedBy Then
                                                          AccessButtonVisibility = Visibility.Visible

                                                          If _fileAccessed Is Nothing Then
                                                              SaveAccessButtonVisibility = Visibility.Visible
                                                              RemoveAccessButtonVisibility = Visibility.Collapsed
                                                          Else
                                                              RemoveAccessButtonVisibility = Visibility.Visible
                                                              SaveAccessButtonVisibility = Visibility.Collapsed
                                                          End If

                                                          If Not _sessionManager.CurrentUser.Role = "Admin" Then
                                                              If _reports?.ReporterId = _sessionManager.CurrentUser.Id Then
                                                                  ReportButtonVisibility = Visibility.Collapsed
                                                                  ReportedButtonVisibility = Visibility.Visible
                                                              Else
                                                                  ReportButtonVisibility = Visibility.Visible
                                                                  ReportedButtonVisibility = Visibility.Collapsed
                                                              End If
                                                          End If
                                                      End If
                                                  End Sub)
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error changing button visibility: {ex.Message}")
        Finally
            ' Explicitly raise property changed notifications
            RaisePropertyChanged(NameOf(DownloadButtonVisibility))
            RaisePropertyChanged(NameOf(AccessButtonVisibility))
            RaisePropertyChanged(NameOf(SaveAccessButtonVisibility))
            RaisePropertyChanged(NameOf(RemoveAccessButtonVisibility))
            RaisePropertyChanged(NameOf(ReportButtonVisibility))
            RaisePropertyChanged(NameOf(ReportedButtonVisibility))
            RaisePropertyChanged(NameOf(EncryptionSectionVisibility))
        End Try
    End Sub

    Private Async Function CheckFileAvailability() As Task(Of Boolean)
        Dim fileShared = Await Task.Run(Function() _fileDataService.GetSharedFileById(_fileShared)).ConfigureAwait(False)

        If fileShared.Availability = "Available" Then
            Return True
        End If

        Await PopUp.Information("Failed", "File was already expired or removed")
        Return False
    End Function

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

            If Not navigationContext.Parameters.ContainsKey("openedFrom") Then
                Await PopUp.Information("Failed", $"Navigation history is empty {navigationContext.Parameters.GetValue(Of String)("openedFrom")}").ConfigureAwait(True)
                _navigationService.GoBack()
                Exit Try
            End If

            If navigationContext.Parameters.ContainsKey("fileId") AndAlso navigationContext.Parameters.ContainsKey("openedFrom") Then
                Dim fileShared = New FilesShared With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("fileId")
                }

                Dim fileAccessed = New FilesAccessed With {
                    .UserId = _sessionManager.CurrentUser.Id,
                    .FileId = fileShared.Id
                }

                Dim report = New Reports With {
                    .ReporterId = _sessionManager.CurrentUser.Id,
                    .FileId = fileShared.Id
                }

                _openedFrom = navigationContext.Parameters.GetValue(Of String)("openedFrom")

                _fileShared = Await Task.Run(Function() _fileDataService?.GetSharedFileById(fileShared)).ConfigureAwait(False)
                _reports = Await Task.Run(Function() _reportsService?.GetReportByIdByUser(report)).ConfigureAwait(False)

                If String.IsNullOrEmpty(_fileShared?.FileName) Then
                    Await PopUp.Information("Error", "Shared File not found").ConfigureAwait(True)
                    Exit Try
                End If

                _fileAccessed = Await Task.Run(Function() _fileDataService.GetAccessedFileByUserFile(fileAccessed)).ConfigureAwait(False)

                _filePath = _fileShared.FilePath
                Await Application.Current.Dispatcher.InvokeAsync(Sub() Load()).Task.ConfigureAwait(False)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[Debug] Error navigating to FileDetailsContentModel: {ex.Message}")
            PopUp.Information("Failed", "Theres an error while loading the file")
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
