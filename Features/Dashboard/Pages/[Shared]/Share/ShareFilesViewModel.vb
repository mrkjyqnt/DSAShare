Imports System.IO
Imports CommunityToolkit.Mvvm.Input
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

#Disable Warning
Public Class ShareFilesViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _fileInfoService As IFileInfoService
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _fileUploadService As IFileService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _activityService As IActivityService

    Private _encryptionInput As String
    Private _nameInput As String
    Private _descriptionInput As String
    Private _isPrivateSelected As Boolean
    Private _isPublicSelected As Boolean
    Private _privacy As String
    Private _isCodeSelected As Boolean
    Private _isWordSelected As Boolean
    Private _selectedOption As String
    Private _selectedDate As DateTime?
    Private _encryptionSection As Boolean
    Private _isExpirationEnabled As Boolean
    Private _expirationPicker As Boolean
    Private _filePath As String
    Private _fileName As String
    Private _fileSize As String
    Private _addFileButtonVisibility As Visibility
    Private _addedFileVisibility As Visibility

    Public ReadOnly Property BackCommand As DelegateCommand
    Public ReadOnly Property PublishFileCommand As AsyncDelegateCommand
    Public ReadOnly Property AddFileCommand As DelegateCommand
    Public ReadOnly Property RemoveFileCommand As DelegateCommand

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

    Public Property AddFileButtonVisibility As Visibility
        Get
            Return _addFileButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_addFileButtonVisibility, value)
        End Set
    End Property

    Public Property AddedFileVisibility As Visibility
        Get
            Return _addedFileVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_addedFileVisibility, value)
        End Set
    End Property

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Sub New(navigationService As INavigationService,
                   fileInfoService As IFileInfoService,
                   fileDataService As IFileDataService,
                   fileUploadService As IFileService,
                   ActivityService As IActivityService,
                   SessionManager As ISessionManager)
        _navigationService = navigationService
        _fileInfoService = fileInfoService
        _fileUploadService = fileUploadService
        _activityService = ActivityService
        _sessionManager = SessionManager
        _fileDataService = fileDataService

        SelectedOption = "Code"
        EncryptionInput = ""
        IsPublicSelected = True
        IsCodeSelected = True
        EncryptionSection = False
        IsExpirationEnabled = False
        ExpirationPicker = False
        AddFileButtonVisibility = Visibility.Visible
        AddedFileVisibility = Visibility.Collapsed

        PublishFileCommand = New AsyncDelegateCommand(AddressOf OnPublish)
        AddFileCommand = New DelegateCommand(AddressOf OnAddFile)
        RemoveFileCommand = New DelegateCommand(AddressOf OnRemoveFile)
        BackCommand = New DelegateCommand(AddressOf OnBack)
    End Sub

    Private Async Function OnPublish() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If NameInput = "" OrElse NameInput Is Nothing Then
                Await PopUp.Information("Failed", "Please add a name").ConfigureAwait(True)
                Return
            End If

            If DescriptionInput = "" OrElse DescriptionInput Is Nothing Then
                Await PopUp.Information("Failed", "Please add a description").ConfigureAwait(True)
                Return
            End If

            If FilePath = "" OrElse FilePath Is Nothing Then
                Await PopUp.Information("Failed", "Please add a file").ConfigureAwait(True)
                Return
            End If

            If Privacy = "Private" Then
                If EncryptionInput = "" OrElse EncryptionInput Is Nothing Then
                    Await PopUp.Information("Failed", "Please add an encryption").ConfigureAwait(True)
                    Return
                End If
            End If

            If IsExpirationEnabled Then
                If SelectedDate Is Nothing Then
                    Await PopUp.Information("Failed", "Please add an encryption").ConfigureAwait(True)
                    Return
                End If
            End If

            If IsExpirationEnabled AndAlso SelectedDate <= Date.Now Then
                Await PopUp.Information("Failed", "Expiry date must be in the future").ConfigureAwait(True)
                Return
            End If

            Dim file = New FilesShared With {
                .Name = If(NameInput, ""),
                .FileName = If(FileName, ""),
                .FileDescription = DescriptionInput,
                .FilePath = If(FilePath, ""),
                .FileSize = If(FileSize, ""),
                .FileType = If(_fileInfoService.Type, ""),
                .UploadedBy = _sessionManager.CurrentUser.Id,
                .ShareType = If(SelectedOption, ""),
                .ShareValue = If(EncryptionInput, ""),
                .ExpiryDate = SelectedDate,
                .Privacy = If(Privacy, ""),
                .DownloadCount = 0,
                .Availability = "Available",
                .CreatedAt = Date.Now,
                .UpdatedAt = Date.Now
            }

            If Not IsExpirationEnabled Then file.ExpiryDate = Nothing
            If Not IsPrivateSelected Then
                file.ShareValue = Nothing
                file.ShareType = Nothing
            End If

            Dim shareTypeExist = Await Task.Run(Function() _fileDataService.GetSharedFileByPrivate(file)).ConfigureAwait(True)

            If shareTypeExist IsNot Nothing Then
                Await PopUp.Information("Failed", "Code is already taken, please create another one or change selection").ConfigureAwait(True)
                Return
            End If

            Dim result = Await Task.Run(Function() _fileUploadService.UploadFile(file)).ConfigureAwait(True)

            If result.Success Then
                Await PopUp.Information("Success", result.Message).ConfigureAwait(True)
            Else
                Await PopUp.Information("Failed", result.Message).ConfigureAwait(True)
                Return
            End If

            file.Id = Await Task.Run(Function() _fileDataService.GetSharedFileInfo(file)?.Id).ConfigureAwait(True)

            Dim activity = New Activities With {
                .Action = "Shared a file",
                .ActionIn = "Shared Files",
                .ActionAt = Date.Now,
                .FileId = file.Id,
                .Name = $"{file.FileName}{file.FileType}",
                .UserId = _sessionManager.CurrentUser.Id
            }

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error uploading file: {ex.Message}")
            Debug.WriteLine($"[DEBUG] Stack Trace: {ex.StackTrace}")
        Finally
            Loading.Hide()
        End Try
    End Function
    Private Sub OnBack()
        _navigationService.GoBack()
    End Sub

    Private Sub OnAddFile()
        Try
            Loading.Show()

            Dim openFileDialog As New Microsoft.Win32.OpenFileDialog()

            openFileDialog.Filter = "All Files (*.*)|*.*"
            openFileDialog.DefaultExt = ".*"

            Dim result As Boolean = openFileDialog.ShowDialog()

            If result Then
                _filePath = openFileDialog.FileName

                FilePath = _filePath
                _fileInfoService.Extract(FilePath)

                FileName = _fileInfoService.Name
                FileSize = _fileInfoService.Size
            Else
                PopUp.Information("Failed", "No file was selected")
                Return
            End If

            AddFileButtonVisibility = Visibility.Collapsed
            AddedFileVisibility = Visibility.Visible
        Catch ex As Exception
        Finally
            Loading.Hide()
        End Try
    End Sub

    Private Sub OnRemoveFile()
        FilePath = String.Empty
        AddedFileVisibility = Visibility.Collapsed
        AddFileButtonVisibility = Visibility.Visible
    End Sub

End Class
