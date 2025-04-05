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
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _userService As IUserService

    Private _file As FilesShared

    Private _availabilityText As String
    Private _enableButtonVisibility As Visibility = Visibility.Collapsed
    Private _disableButtonVisibility As Visibility = Visibility.Collapsed

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

    Public ReadOnly Property DisableFileCommand As AsyncDelegateCommand
    Public ReadOnly Property EnableFileCommand As AsyncDelegateCommand
    Public ReadOnly Property DeleteFileCommand As AsyncDelegateCommand

    Public Sub New(fileService As IFileService,
                   fileDataService As IFileDataService,
                   navigationService As INavigationService,
                   activityService As IActivityService,
                   userService As IUserService)
        _fileService = fileService
        _fileDataService = fileDataService
        _navigationService = navigationService
        _activityService = activityService
        _userService = userService

        DisableFileCommand = New AsyncDelegateCommand(AddressOf OnDisableFile)
        EnableFileCommand = New AsyncDelegateCommand(AddressOf OnEnableFile)
        DeleteFileCommand = New AsyncDelegateCommand(AddressOf OnDeleteFile)
    End Sub

    Public Async Sub UpdateVisibility()
        Try
            If _file.Availability = "Disabled" Then
                EnableButtonVisibility = Visibility.Visible
                AvailabilityText = "Enable file"
            End If

            If _file.Availability = "Available" Then
                DisableButtonVisibility = Visibility.Visible
                AvailabilityText = "Disable file"
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Async Function OnDisableFile() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
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
                .FileName = $"{_file.FileName}{_file.FileType}",
                .UserId = _file.UploadedBy
            }

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

        Catch ex As Exception

        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnEnableFile() As Task
        Try
            Loading.Show()

            If _file Is Nothing Then
                PopUp.Information("Failed", "Theres an error while disabling the file, File reference is nothing")
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

            Dim file As FilesShared = _file
            file.Availability = "Available"

            Dim result = _fileService.UpdateFile(file)

            If result.Success Then
                Await PopUp.Information("Success", "Succesfully enabling the file").ConfigureAwait(True)
            Else
                Await PopUp.Information("Failed", result.Message).ConfigureAwait(True)
                Return
            End If

            Dim activity = New Activities With {
                .Action = "Enabled a file",
                .ActionIn = "Shared Files",
                .ActionAt = Date.Now,
                .FileId = _file.Id,
                .FileName = $"{_file.FileName}{_file.FileType}",
                .UserId = _file.UploadedBy
            }

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

        Catch ex As Exception

        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnDeleteFile() As Task
        Try
            Loading.Show()

            If _file Is Nothing Then
                PopUp.Information("Failed", "Theres an error while disabling the file, File reference is nothing")
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

            Dim result = _fileService.DeleteFile(_file)

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
                .FileName = $"{_file.FileName}{_file.FileType}",
                .UserId = _file.UploadedBy
            }

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

        Catch ex As Exception

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
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100).ConfigureAwait(True)

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
