Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

#Disable Warning
Public Class FileSettingsViewModel
    Inherits BindableBase
    Implements INavigationAware

    Implements IRegionMemberLifetime

    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _fileInfoService As IFileInfoService
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _fileUploadService As IFileService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _activityService As IActivityService

    Private _file As FilesShared

    Private _encryptionInput As String
    Private _nameInput As String
    Private _descriptionInput As String
    Private _isPrivateSelected As Boolean = False
    Private _isPublicSelected As Boolean = False
    Private _privacy As String
    Private _isCodeSelected As Boolean = False
    Private _isWordSelected As Boolean = False
    Private _selectedOption As String
    Private _selectedDate As DateTime? = Nothing
    Private _encryptionSection As Boolean = False
    Private _isExpirationEnabled As Boolean = False
    Private _expirationPicker As Boolean = False
    Private _filePath As String
    Private _fileName As String
    Private _fileSize As String

    Public ReadOnly Property SaveChangesCommand As AsyncDelegateCommand

    Public Property NameInput As String
        Get
            Return _nameInput
        End Get
        Set(value As String)
            SetProperty(_nameInput, value)
        End Set
    End Property
    Public Property DescriptionInput As String
        Get
            Return _descriptionInput
        End Get
        Set(value As String)
            SetProperty(_descriptionInput, value)
        End Set
    End Property
    Public Property IsPublicSelected As Boolean
        Get
            Return _isPublicSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isPublicSelected, value)
            If value Then
                Privacy = "Public"
                EncryptionSection = False
            End If
        End Set
    End Property
    Public Property IsPrivateSelected As Boolean
        Get
            Return _isPrivateSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isPrivateSelected, value)
            If value Then
                Privacy = "Private"
                EncryptionSection = True
            End If
        End Set
    End Property
    Public Property Privacy As String
        Get
            Return _privacy
        End Get
        Set(value As String)
            SetProperty(_privacy, value)
        End Set
    End Property

    Public Property EncryptionSection As Boolean
        Get
            Return _encryptionSection
        End Get
        Set(value As Boolean)
            SetProperty(_encryptionSection, value)
        End Set
    End Property

    Public Property EncryptionInput As String
        Get
            Return _encryptionInput
        End Get
        Set(value As String)
            If ValidateInput(value, SelectedOption) Then
                SetProperty(_encryptionInput, value)
            End If
        End Set
    End Property

    Public Property IsCodeSelected As Boolean
        Get
            Return _isCodeSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isCodeSelected, value)
            If value Then SelectedOption = "Code"
        End Set
    End Property

    Public Property IsWordSelected As Boolean
        Get
            Return _isWordSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isWordSelected, value)
            If value Then SelectedOption = "Word"
        End Set
    End Property

    Public Property SelectedOption As String
        Get
            Return _selectedOption
        End Get
        Set(value As String)
            If value IsNot _selectedOption Then
                SetProperty(_selectedOption, value)
                EncryptionInput = ""
            End If
        End Set
    End Property

    Public Property SelectedDate As DateTime?
        Get
            Return _selectedDate
        End Get
        Set(value As DateTime?)
            SetProperty(_selectedDate, value)
        End Set
    End Property

    Public Property IsExpirationEnabled As Boolean
        Get
            Return _isExpirationEnabled
        End Get
        Set(value As Boolean)
            If value Then
                ExpirationPicker = True
                SelectedDate = Nothing
            Else
                ExpirationPicker = False
            End If
            SetProperty(_isExpirationEnabled, value)
        End Set
    End Property

    Public Property ExpirationPicker As Boolean
        Get
            Return _expirationPicker
        End Get
        Set(value As Boolean)
            SetProperty(_expirationPicker, value)
        End Set
    End Property

    Public Property FilePath As String
        Get
            Return _filePath
        End Get
        Private Set(value As String)
            SetProperty(_filePath, value)
        End Set
    End Property

    Public Property FileName As String
        Get
            Return _fileName
        End Get
        Private Set(value As String)
            SetProperty(_fileName, value)
        End Set
    End Property

    Public Property FileSize As String
        Get
            Return _fileSize
        End Get
        Private Set(value As String)
            SetProperty(_fileSize, value)
        End Set
    End Property

    Public Sub New(navigationService As INavigationService,
                   fileInfoService As IFileInfoService,
                   fileDataService As IFileDataService,
                   fileUploadService As IFileService,
                   sessionManager As ISessionManager,
                   activityService As IActivityService)
        _navigationService = navigationService
        _fileInfoService = fileInfoService
        _fileDataService = fileDataService
        _fileUploadService = fileUploadService
        _sessionManager = sessionManager
        _activityService = activityService

        SaveChangesCommand = New AsyncDelegateCommand(AddressOf OnSaveChanges)
    End Sub

    Private Sub Load()
        Try
            If _file.Privacy = "Public" Then
                IsPublicSelected = True
            End If

            If _file.Privacy = "Private" Then
                IsPrivateSelected = True
                EncryptionSection = True

                If _file.ShareType = "Code" Then
                    IsCodeSelected = True
                End If

                If _file.ShareType = "Word" Then
                    IsWordSelected = True
                End If

                EncryptionInput = _file.ShareValue
            End If

            If _file.ExpiryDate IsNot Nothing Then
                IsExpirationEnabled = True
                ExpirationPicker = True
                SelectedDate = _file.ExpiryDate
            End If

            NameInput = _file.Name
            DescriptionInput = _file.FileDescription
            FileName = _file.FileName
            FileSize = _file.FileSize
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error loading file settings: {ex.Message}")
        End Try
    End Sub

    Private Async Function OnSaveChanges() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If String.IsNullOrEmpty(NameInput) Then
                Await PopUp.Information("Failed", "Please add a name").ConfigureAwait(True)
                Return
            End If

            If String.IsNullOrEmpty(DescriptionInput) Then
                Await PopUp.Information("Failed", "Please add a description").ConfigureAwait(True)
                Return
            End If

            If Privacy = "Private" AndAlso String.IsNullOrEmpty(EncryptionInput) Then
                Await PopUp.Information("Failed", "Please add an encryption").ConfigureAwait(True)
                Return
            End If

            If IsExpirationEnabled Then
                If Not SelectedDate.HasValue OrElse SelectedDate <= Date.Now Then
                    Await PopUp.Information("Failed", "Please select a valid future expiry date").ConfigureAwait(True)
                    Return
                End If
            End If

            ' Create updated file object
            Dim file = New FilesShared With {
                .Id = _file.Id,
                .Name = NameInput,
                .FileName = _file.FileName,
                .FileDescription = DescriptionInput,
                .FilePath = _file.FilePath,
                .FileSize = _file.FileSize,
                .FileType = _file.FileType,
                .UploadedBy = _file.UploadedBy,
                .ShareType = If(IsPrivateSelected, SelectedOption, Nothing),
                .ShareValue = If(IsPrivateSelected, EncryptionInput, Nothing),
                .ExpiryDate = If(IsExpirationEnabled, SelectedDate, Nothing),
                .Privacy = Privacy,
                .DownloadCount = _file.DownloadCount,
                .Availability = _file.Availability,
                .CreatedAt = _file.CreatedAt,
                .UpdatedAt = Date.Now
            }

            If Not HasChanges(file, _file) Then
                Await PopUp.Information("Info", "No changes detected").ConfigureAwait(True)
                Return
            End If

            Dim result = Await Task.Run(Function() _fileUploadService.UpdateFile(file)).ConfigureAwait(True)

            If result.Success Then
                Await PopUp.Information("Success", result.Message).ConfigureAwait(True)
            Else
                Await PopUp.Information("Failed", result.Message).ConfigureAwait(True)
                Return
            End If

            Dim activity = New Activities With {
                .Action = "Updated a file",
                .ActionIn = "Shared Files",
                .ActionAt = Date.Now,
                .FileId = file.Id,
                .FileName = $"{file.FileName}{file.FileType}",
                .UserId = _sessionManager.CurrentUser.Id
            }

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error in OnSaveChanges: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Function HasChanges(newFile As FilesShared, oldFile As FilesShared) As Boolean
        ' Compare all relevant properties
        Return newFile.Name <> oldFile.Name OrElse
               newFile.FileDescription <> oldFile.FileDescription OrElse
               newFile.Privacy <> oldFile.Privacy OrElse
               newFile.ShareType <> oldFile.ShareType OrElse
               newFile.ShareValue <> oldFile.ShareValue OrElse
               Not NullableDatesEqual(newFile.ExpiryDate, oldFile.ExpiryDate)
    End Function

    Private Function NullableDatesEqual(date1 As DateTime?, date2 As DateTime?) As Boolean
        If date1.HasValue AndAlso date2.HasValue Then
            Return date1.Value = date2.Value
        End If
        Return Not date1.HasValue AndAlso Not date2.HasValue
    End Function

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedTo
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If navigationContext.Parameters.ContainsKey("fileId") Then
                Dim file = New FilesShared With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("fileId")
                }
                _file = Await Task.Run(Function() _fileDataService.GetSharedFileById(file)).ConfigureAwait(True)

                If String.IsNullOrEmpty(_file?.FileName) Then
                    Await PopUp.Information("Error", "File not found").ConfigureAwait(True)
                    Return
                End If

                _filePath = _file.FilePath
                Application.Current.Dispatcher.Invoke(Sub()
                                                          Load()
                                                      End Sub)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error navigating to FileDetailsContentModel: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedFrom
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Return False
    End Function

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property
End Class
