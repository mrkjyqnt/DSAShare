Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

#Disable Warning
Public Class FileDangerZoneViewModel
    Inherits BindableBase
    Implements INavigationAware
    Implements IRegionMemberLifetime

    Private ReadOnly _fileService As IFileService
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _fileBlockService As IFileBlockService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _userService As IUserService
    Private ReadOnly _sessionManager As ISessionManager

    Private _file As FilesShared
    Private _openedFrom As String

    Private _availabilityText As String
    Private _enableButtonVisibility As Visibility = Visibility.Collapsed
    Private _disableButtonVisibility As Visibility = Visibility.Collapsed
    Private _blockFileSectionVisibility As Visibility = Visibility.Collapsed

    Public Property AvailabilityText As String
        Get
            Return _availabilityText
        End Get
        Set(value As String)
            SetProperty(_availabilityText, value)
        End Set
    End Property

    Public Property EnableButtonVisibility As Visibility
        Get
            Return _enableButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_enableButtonVisibility, value)
        End Set
    End Property

    Public Property DisableButtonVisibility As Visibility
        Get
            Return _disableButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_disableButtonVisibility, value)
        End Set
    End Property
    Public Property BlockFileSectionVisibility As Visibility
        Get
            Return _blockFileSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_blockFileSectionVisibility, value)
        End Set
    End Property

    Public ReadOnly Property DisableFileCommand As AsyncDelegateCommand
    Public ReadOnly Property EnableFileCommand As AsyncDelegateCommand
    Public ReadOnly Property DeleteFileCommand As AsyncDelegateCommand
    Public ReadOnly Property BlockFileCommand As AsyncDelegateCommand

    Public Sub New(fileService As IFileService,
                   fileDataService As IFileDataService,
                   fileBlockService As IFileBlockService,
                   navigationService As INavigationService,
                   activityService As IActivityService,
                   userService As IUserService,
                   sessionManager As ISessionManager)
        _fileService = fileService
        _fileDataService = fileDataService
        _navigationService = navigationService
        _activityService = activityService
        _userService = userService
        _sessionManager = sessionManager
        _fileBlockService = fileBlockService

        DisableFileCommand = New AsyncDelegateCommand(AddressOf OnDisableFile)
        EnableFileCommand = New AsyncDelegateCommand(AddressOf OnEnableFile)
        DeleteFileCommand = New AsyncDelegateCommand(AddressOf OnDeleteFile)
        BlockFileCommand = New AsyncDelegateCommand(AddressOf OnBlockFile)
    End Sub

    Public Sub UpdateVisibility()
        If _file.Availability = "Disabled" Then
            EnableButtonVisibility = Visibility.Visible
            AvailabilityText = "Enable file"
        End If

        If _file.Availability = "Available" Then
            DisableButtonVisibility = Visibility.Visible
            AvailabilityText = "Disable file"
        End If

        If _sessionManager.CurrentUser.Role = "Admin" AndAlso
            _openedFrom = "ManageFilesView" Then
            BlockFileSectionVisibility = Visibility.Visible
        End If
    End Sub

    Private Async Function OnDisableFile() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            If _file.Privacy = "Blocked" OrElse _file.Privacy = "Deleted" Then
                Await PopUp.Information("Warning", "This has been blocked or deleted. Actions cannot be done.").ConfigureAwait(True)
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the disable of the file.").ConfigureAwait(True)

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation().ConfigureAwait(True)

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "File disable was cancelled.").ConfigureAwait(True)
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim user = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(user)).ConfigureAwait(True)
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)").ConfigureAwait(True)
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts = maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. disable cancelled.").ConfigureAwait(True)
                Return
            End If

            If _file Is Nothing Then
                PopUp.Information("Failed", "Theres an error while disabling the file, File reference is nothing")
                Return
            End If

            Dim file As FilesShared = _file
            file.Availability = "Disabled"

            Dim result = _fileService.UpdateFile(file)

            If result.Success Then
                Await PopUp.Information("Success", "Succesfully disabled the file").ConfigureAwait(True)
            Else
                Await PopUp.Information("Failed", result.Message).ConfigureAwait(True)
                Return
            End If

            Dim activity = New Activities With {
                .Action = "Disabled a file",
                .ActionIn = "Shared Files",
                .ActionAt = Date.Now,
                .FileId = _file.Id,
                .Name = $"{_file.FileName}{_file.FileType}",
                .UserId = _sessionManager.CurrentUser.Id
            }

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

        Catch ex As Exception
            Debug.WriteLine($"[FileDangerZoneViewModel] OnDisableFile Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnEnableFile() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            If _file Is Nothing Then
                PopUp.Information("Failed", "Theres an error while disabling the file, File reference is nothing")
                Return
            End If

            If _file.Privacy = "Blocked" OrElse _file.Privacy = "Deleted" Then
                Await PopUp.Information("Warning", "This has been blocked or deleted. Actions cannot be done.").ConfigureAwait(True)
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the enable of the file.").ConfigureAwait(True)

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation().ConfigureAwait(True)

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "File enable was cancelled.").ConfigureAwait(True)
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim user = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(user)).ConfigureAwait(True)
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)").ConfigureAwait(True)
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts = maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. enable cancelled.").ConfigureAwait(True)
                Return
            End If

            Dim file As FilesShared = _file
            file.Availability = "Available"

            Dim result = _fileService.UpdateFile(file)

            If result.Success Then
                Await PopUp.Information("Success", "Succesfully enabled the file").ConfigureAwait(True)
            Else
                Await PopUp.Information("Failed", result.Message).ConfigureAwait(True)
                Return
            End If

            Dim activity = New Activities With {
                .Action = "Enabled a file",
                .ActionIn = "Shared Files",
                .ActionAt = Date.Now,
                .FileId = _file.Id,
                .Name = $"{_file.FileName}{_file.FileType}",
                .UserId = _file.UploadedBy
            }

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

        Catch ex As Exception
            Debug.WriteLine($"[FileDangerZoneViewModel] OnEnableFile Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnDeleteFile() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            If _file Is Nothing Then
                PopUp.Information("Failed", "Theres an error while disabling the file, File reference is nothing")
                Return
            End If

            If _file.Privacy = "Blocked" OrElse _file.Privacy = "Deleted" Then
                Await PopUp.Information("Warning", "This has been blocked or deleted. Actions cannot be done.").ConfigureAwait(True)
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the deletion of the file.").ConfigureAwait(True)

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation().ConfigureAwait(True)

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "File deletion was cancelled.").ConfigureAwait(True)
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim user = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(user)).ConfigureAwait(True)
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)").ConfigureAwait(True)
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts = maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. Deletion cancelled.").ConfigureAwait(True)
                Return
            End If

            If _file Is Nothing Then
                PopUp.Information("Failed", "Theres an error while disabling the file, File reference is nothing")
                Return
            End If

            _file.Privacy = "Deleted"
            Dim result = Await Task.Run(Function() _fileService.DeleteFile(_file)).ConfigureAwait(True)

            If result.Success Then
                Await PopUp.Information("Success", result.Message).ConfigureAwait(True)
            Else
                Await PopUp.Information("Failed", result.Message).ConfigureAwait(True)
                Return
            End If

            Dim activity = New Activities With {
                .Action = "Deleted a file",
                .ActionIn = "Shared Files",
                .ActionAt = Date.Now,
                .FileId = _file.Id,
                .Name = $"{_file.FileName}{_file.FileType}",
                .UserId = _sessionManager.CurrentUser.Id
            }

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

        Catch ex As Exception
            Debug.WriteLine($"[FileDangerZoneViewModel] OnDeleteFile Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnBlockFile() As Task
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

            If _file Is Nothing Then
                PopUp.Information("Failed", "Theres an error while disabling the file, File reference is nothing")
                Return
            End If

            If _file.Privacy = "Blocked" OrElse _file.Privacy = "Deleted" Then
                Await PopUp.Information("Warning", "This has been blocked or deleted. Actions cannot be done.").ConfigureAwait(True)
                Return
            End If

                        Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the blocking of the file.").ConfigureAwait(True)

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation().ConfigureAwait(True)

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "File deletion was cancelled.").ConfigureAwait(True)
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim user = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(user)).ConfigureAwait(True)
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)").ConfigureAwait(True)
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts = maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. Blocking cancelled.").ConfigureAwait(True)
                Return
            End If

            Await PopUp.Information("Information", "Please enter the blocking description.").ConfigureAwait(True)
            Dim descriptionResult As PopupResult = Await PopUp.Description().ConfigureAwait(True)

            If descriptionResult Is Nothing Then
                Await PopUp.Information("Failed", "You did not add any description.").ConfigureAwait(True)
                Exit Try
            End If

            If _file Is Nothing Then
                PopUp.Information("Failed", "Theres an error while blocking the file, File reference is nothing")
                Return
            End If

            Dim fileBlock = New FilesBlocked With {
                .FileHash = ComputeSHA256($"{ConfigurationModule.GetSettings().Network.FolderPath}\{_file.FilePath}"),
                .Reason = descriptionResult.GetValue(Of String)("Input"),
                .BlockedBy = _sessionManager.CurrentUser.Id,
                .BlockedAt = Date.Now
            }

            Dim result = Await Task.Run(Function() _fileBlockService.BlockFile(fileBlock)).ConfigureAwait(True)

            If result Then
                _file.Privacy = "Blocked"
                Dim deleteResult = Await Task.Run(Function() _fileService.DeleteFile(_file)).ConfigureAwait(True)

                If deleteResult.Success Then
                    Await PopUp.Information("Success", "Succesfully blocked the file").ConfigureAwait(True)
                Else
                    Await PopUp.Information("Failed", deleteResult.Message).ConfigureAwait(True)
                    Return
                End If

                Dim activity = New Activities With {
                   .Action = "Blocked a file",
                   .ActionIn = "Shared Files",
                   .ActionAt = Date.Now,
                   .FileId = _file.Id,
                   .Name = $"{_file.FileName}{_file.FileType}",
                   .UserId = _sessionManager.CurrentUser.Id
               }

                 Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) 
                _navigationService.GoBack()
            Else
                Await PopUp.Information("Failed", "Theres something wrong while trying to block the file").ConfigureAwait(True)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[FileDangerZoneViewModel] OnBlockFile Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedTo
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
                Dim file = New FilesShared With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("fileId")
                }

                _openedFrom = navigationContext.Parameters.GetValue(Of String)("openedFrom")

                _file = Await Task.Run(Function() _fileDataService.GetSharedFileById(file)).ConfigureAwait(True)

                If String.IsNullOrEmpty(_file?.FileName) Then
                    Await PopUp.Information("Error", "File not found").ConfigureAwait(True)
                    Return
                End If

                UpdateVisibility()
            End If
        Catch ex As Exception
            Debug.WriteLine($"[Debug] Error navigating to FileDetailsContentModel: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedFrom
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Return True
    End Function
End Class
