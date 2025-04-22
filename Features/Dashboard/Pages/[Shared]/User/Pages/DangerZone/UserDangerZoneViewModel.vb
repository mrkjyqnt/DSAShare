Imports Microsoft.VisualBasic.ApplicationServices
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

#Disable Warning CRR0029
#Disable Warning
Public Class UserDangerZoneViewModel
    Inherits BindableBase
    Implements INavigationAware
    Implements IRegionMemberLifetime

    Private ReadOnly _fileService As IFileService
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _userService As IUserService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _regionManager As IRegionManager

    Private _user As Users
    Private _openedFrom As String

    Private _secretSectionVisibility As Visibility = Visibility.Collapsed
    Private _deactivateSectionVisibility As Visibility = Visibility.Collapsed
    Private _unBanAccountButtonVisibility As Visibility = Visibility.Collapsed
    Private _banAccountButtonVisibility As Visibility = Visibility.Collapsed

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Property SecretSectionVisibility As Visibility
        Get
            Return _secretSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_secretSectionVisibility, value)
            _secretSectionVisibility = value
        End Set
    End Property
    Public Property DeactivateSectionVisibility As Visibility
        Get
            Return _deactivateSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_deactivateSectionVisibility, value)
            _deactivateSectionVisibility = value
        End Set
    End Property

    Public Property UnBanAccountButtonVisibility As Visibility
        Get
            Return _unBanAccountButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_unBanAccountButtonVisibility, value)
            _unBanAccountButtonVisibility = value
        End Set
    End Property

    Public Property BanAccountButtonVisibility As Visibility
        Get
            Return _banAccountButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_banAccountButtonVisibility, value)
            _banAccountButtonVisibility = value
        End Set
    End Property

    Public ReadOnly Property BanAccountCommand As AsyncDelegateCommand
    Public ReadOnly Property UnBanAccountCommand As AsyncDelegateCommand
    Public ReadOnly Property DeleteAccountCommand As AsyncDelegateCommand
    Public ReadOnly Property DeactivateAccountCommand As AsyncDelegateCommand

    Public Sub New(fileService As IFileService,
                   fileDataService As IFileDataService,
                   navigationService As INavigationService,
                   activityService As IActivityService,
                   userService As IUserService,
                   sessionManager As ISessionManager,
                   regionManager As IRegionManager)
        _fileService = fileService
        _fileDataService = fileDataService
        _navigationService = navigationService
        _activityService = activityService
        _userService = userService
        _sessionManager = sessionManager
        _regionManager = regionManager

        BanAccountCommand = New AsyncDelegateCommand(AddressOf OnBanAccount)
        UnBanAccountCommand = New AsyncDelegateCommand(AddressOf OnUnBanAccount)
        DeleteAccountCommand = New AsyncDelegateCommand(AddressOf OnDeleteAccount)
        DeactivateAccountCommand = New AsyncDelegateCommand(AddressOf OnDeactivateAccount)
    End Sub

    Private Async Function OnBanAccount() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.")
                RestartApplication()
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the e updating of the user.")

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation()

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "Account banning was cancelled.")
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim thisUser = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(thisUser))
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)")
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts >= maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. Banning cancelled.")
                Return
            End If

            If _user Is Nothing Then
                Await PopUp.Information("Failed", "Theres an error while updating the user, User reference is nothing")
                Return
            End If

            _user.Status = "Banned"

            Dim result = Await Task.Run(Function() _userService.UpdateUser(_user)).ConfigureAwait(True)

            If result Then
                Await Task.Run(Function() _fileDataService.DisableAllSharedFileByUploader(New FilesShared With {.UploadedBy = _user.Id})).ConfigureAwait(True)
                Await PopUp.Information("Success", "Succesfully banned the user")
            Else
                Await PopUp.Information("Failed", "Unable to ban user")
                _navigationService.GoBack()
                Return
            End If

            Dim activity = New Activities With {
                .Action = "Banned a user",
                .ActionIn = "Manage Users",
                .ActionAt = Date.Now,
                .AccountId = _user.Id,
                .Name = _user.Name,
                .UserId = _sessionManager.CurrentUser.Id
            }

            Await Task.Run(Function() _activityService.AddActivity(activity))

            UpdateVisibility()
            Loading.Hide()
        Catch ex As Exception

        End Try
    End Function

    Private Async Function OnUnBanAccount() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.")
                RestartApplication()
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the unban of the user.")

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation()

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "Account unbanning was cancelled.")
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim thisUser = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(thisUser))
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)")
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts >= maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. Unbanning cancelled.")
                Return
            End If

            If _user Is Nothing Then
                Await PopUp.Information("Failed", "Theres an error while updating a user, User reference is nothing")
                Return
            End If

            _user.Status = "Active"

            Dim result = _userService.UpdateUser(_user)

            If result Then
                Await PopUp.Information("Success", "Succesfully unbanned the user")
            Else
                Await PopUp.Information("Failed", "Unable to unban user")
                _navigationService.GoBack()
                Return
            End If

            Dim activity = New Activities With {
                .Action = "Unbanned a user",
                .ActionIn = "Manage Users",
                .ActionAt = Date.Now,
                .AccountId = _user.Id,
                .Name = _user.Name,
                .UserId = _sessionManager.CurrentUser.Id
            }

            Await Task.Run(Function() _activityService.AddActivity(activity))
        Catch ex As Exception

        Finally
            UpdateVisibility()
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnDeleteAccount() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.")
                RestartApplication()
                Return
            End If

            If _user Is Nothing Then
                Await PopUp.Information("Failed", "Theres an error while deleting the user, User reference is nothing")
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the deletion of the account.")

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation()

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "File deletion was cancelled.")
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim user = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(user))
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)")
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts = maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. Deletion cancelled.")
                Return
            End If

            If _user Is Nothing Then
                Await PopUp.Information("Failed", "Theres an error while disabling the file, File reference is nothing")
                Return
            End If

            Dim filesResult = Await Task.Run(Function() _fileService.DeleteAllFileByUser(_user))
            Await Task.Run(Sub() _activityService.DeleteAllActivity(_user))
            Dim result = Await Task.Run(Function() _userService.DeleteUser(_user))

            If Not result Then
                Await PopUp.Information("Failed", "Unable to delete the user")
                _navigationService.GoBack()
                Return
            End If

            If _openedFrom = "ManageUsersView" Then
                Dim activity = New Activities With {
                    .Action = "Deleted a user",
                    .ActionIn = "Manage Users",
                    .ActionAt = Date.Now,
                    .AccountId = _user.Id,
                    .Name = _user.Name,
                    .UserId = _sessionManager.CurrentUser.Id
                }

                Await Task.Run(Function() _activityService.AddActivity(activity))
                _navigationService.GoBack()
            Else
                Await PopUp.Information("Success", "Account has been permanently deleted")
                Await PopUp.Information("Success", "Application will restart for data refresh")
                _sessionManager.Logout()
                RestartApplication()
            End If

        Catch ex As Exception
            Debug.WriteLine($"[Debug] Error deleting user: {ex.Message}")
            PopUp.Information("Error", "Unable to delete the user")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnDeactivateAccount() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.")
                RestartApplication()
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "By entering password, you agree that all your Shared Files will be disabled by default")

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation()

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "Account deactivation was cancelled.")
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim thisUser = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(thisUser))
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)")
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts >= maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. Deactivation cancelled.")
                Return
            End If

            If _user Is Nothing Then
                Await PopUp.Information("Failed", "Theres an error while deactivating account, User reference is nothing")
                Return
            End If

            _user.Status = "Deactivated"

            Dim result = Await Task.Run(Function() _userService.UpdateUser(_user)).ConfigureAwait(True)

            If result Then
                _sessionManager.Logout()
                Await Task.Run(Function() _fileDataService.DisableAllSharedFileByUploader(New FilesShared With {.UploadedBy = _user.Id})).ConfigureAwait(True)
                Await PopUp.Information("Success", "Succesfully Deactivated")
            Else
                Await PopUp.Information("Failed", "Unable to Deactivate")
                Return
            End If


            Dim activity = New Activities With {
                .Action = "Deactivated",
                .ActionIn = "Account",
                .ActionAt = Date.Now,
                .UserId = _sessionManager.CurrentUser.Id
            }

            Await Task.Run(Function() _activityService.AddActivity(activity))
            _sessionManager.Logout()
            RestartApplication()
            Return
        Catch ex As Exception
            Debug.WriteLine($"[UserDangerZoneViewModel] OnDeactivateAccount Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Public Sub UpdateVisibility()
        Try
            If _openedFrom = "ManageUsersView" Then
                If _sessionManager.CurrentUser.Role = "Admin" Then
                    SecretSectionVisibility = Visibility.Visible
                End If

                If _user.Status = "Banned" Then
                    UnBanAccountButtonVisibility = Visibility.Visible
                    BanAccountButtonVisibility = Visibility.Collapsed
                End If

                If _user.Status = "Active" Then
                    BanAccountButtonVisibility = Visibility.Visible
                    UnBanAccountButtonVisibility = Visibility.Collapsed
                End If

                Return
            End If

            DeactivateSectionVisibility = Visibility.Visible
        Catch ex As Exception

        End Try
    End Sub

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedTo
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If navigationContext.Parameters.ContainsKey("userId") And navigationContext.Parameters.ContainsKey("openedFrom") Then
                Dim user = New Users With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("userId")
                }

                _openedFrom = navigationContext.Parameters.GetValue(Of String)("openedFrom")
                _user = Await Task.Run(Function() _userService.GetUserById(user))

                If String.IsNullOrEmpty(_user?.Id) Then
                    Await PopUp.Information("Error", "User not found")
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
