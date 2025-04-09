Imports Microsoft.VisualBasic.ApplicationServices
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

#Disable Warning CRR0029
Public Class UserDangerZoneViewModel
    Inherits BindableBase
    Implements INavigationAware
    Implements IRegionMemberLifetime

    Private ReadOnly _fileService As IFileService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _userService As IUserService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _regionManager As IRegionManager

    Private _user As Users

    Private _banningSectionVisibility As Visibility = Visibility.Collapsed
    Private _unBanAccountButtonVisibility As Visibility = Visibility.Collapsed
    Private _banAccountButtonVisibility As Visibility = Visibility.Collapsed

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Property BanningSectionVisibility As Visibility
        Get
            Return _banningSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_banningSectionVisibility, value)
            _banningSectionVisibility = value
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

    Public Sub New(fileService As IFileService,
                   navigationService As INavigationService,
                   activityService As IActivityService,
                   userService As IUserService,
                   sessionManager As ISessionManager,
                   regionManager As IRegionManager)
        _fileService = fileService
        _navigationService = navigationService
        _activityService = activityService
        _userService = userService
        _sessionManager = sessionManager
        _regionManager = regionManager

        BanAccountCommand = New AsyncDelegateCommand(AddressOf OnBanAccount)
        UnBanAccountCommand = New AsyncDelegateCommand(AddressOf OnUnBanAccount)
        DeleteAccountCommand = New AsyncDelegateCommand(AddressOf OnDeleteAccount)
    End Sub

    Private Async Function OnBanAccount() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the e updating of the user.").ConfigureAwait(True)

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation().ConfigureAwait(True)

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "Account banning was cancelled.").ConfigureAwait(True)
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim thisUser = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(thisUser)).ConfigureAwait(True)
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)").ConfigureAwait(True)
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts >= maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. Banning cancelled.").ConfigureAwait(True)
                Return
            End If

            If _user Is Nothing Then
                Await PopUp.Information("Failed", "Theres an error while updating the user, User reference is nothing")
                Return
            End If

            _user.Status = "Banned"

            Dim result = _userService.UpdateUser(_user)

            If result Then
                Await PopUp.Information("Success", "Succesfully banned the user").ConfigureAwait(True)
            Else
                Await PopUp.Information("Failed", "Unable to ban user").ConfigureAwait(True)
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

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

        Catch ex As Exception

        Finally
            UpdateVisibility()
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnUnBanAccount() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the updating of the user.").ConfigureAwait(True)

            While attempts < maxAttempts
                attempts += 1

                Dim popUpResult As PopupResult = Await PopUp.Confirmation().ConfigureAwait(True)

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "Account banning was cancelled.").ConfigureAwait(True)
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim thisUser = New Users With {
                        .PasswordHash = HashPassword(enteredPassword)
                    }

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(thisUser)).ConfigureAwait(True)
                    If Not hasPermission Then
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)").ConfigureAwait(True)
                    Else
                        Exit While
                    End If
                End If
            End While

            If attempts >= maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. Unbanning cancelled.").ConfigureAwait(True)
                Return
            End If

            If _user Is Nothing Then
                Await PopUp.Information("Failed", "Theres an error while updating a user, User reference is nothing")
                Return
            End If

            _user.Status = "Active"

            Dim result = _userService.UpdateUser(_user)

            If result Then
                Await PopUp.Information("Success", "Succesfully banned the user").ConfigureAwait(True)
            Else
                Await PopUp.Information("Failed", "Unable to ban user").ConfigureAwait(True)
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

            If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                _navigationService.GoBack()
            End If

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

            If _user Is Nothing Then
                Await PopUp.Information("Failed", "Theres an error while deleting the user, User reference is nothing")
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

            If _user Is Nothing Then
                Await PopUp.Information("Failed", "Theres an error while disabling the file, File reference is nothing")
                Return
            End If

            Dim filesResult = Await Task.Run(Function() _fileService.DeleteAllFileByUser(_user)).ConfigureAwait(True)
            Await Task.Run(Sub() _activityService.DeleteAllActivity(_user)).ConfigureAwait(True)
            Dim result = Await Task.Run(Function() _userService.DeleteUser(_user)).ConfigureAwait(True)

            If Not result Then
                Await PopUp.Information("Failed", "Unable to delete the user").ConfigureAwait(True)
                _navigationService.GoBack()
                Return
            End If

            Await PopUp.Information("Success", "Account has been permanently deleted").ConfigureAwait(True)
            Await PopUp.Information("Success", "Application will restart for data refresh").ConfigureAwait(True)
            _sessionManager.Logout
            RestartApplication()
        Catch ex As Exception
            Debug.WriteLine($"[Debug] Error deleting user: {ex.Message}")
            _sessionManager.Logout

            RestartApplication()
        Finally
            Loading.Hide()
        End Try
    End Function

    Public Sub UpdateVisibility()
        Try
            If _user.Status = "Banned" Then
                _unBanAccountButtonVisibility = Visibility.Visible
            End If

            If _user.Status = "Active" Then
                _banAccountButtonVisibility = Visibility.Visible
            End If

            RaisePropertyChanged(NameOf(_unBanAccountButtonVisibility))
            RaisePropertyChanged(NameOf(_banAccountButtonVisibility))
        Catch ex As Exception

        End Try
    End Sub

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedTo
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100).ConfigureAwait(True)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If navigationContext.Parameters.ContainsKey("userId") Then
                Dim user = New Users With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("userId")
                }

                _user = Await Task.Run(Function() _userService.GetUserById(user)).ConfigureAwait(True)

                If String.IsNullOrEmpty(_user?.Id) Then
                    Await PopUp.Information("Error", "User not found").ConfigureAwait(True)
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
        Try
            If navigationContext IsNot Nothing Then
                Dim region = _regionManager.Regions("UserPageRegion")
                Dim view = region.Views.FirstOrDefault(Function(v) v.GetType().Name = "UserInformationView")
                If view IsNot Nothing Then
                    region.Remove(view)
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationView] OnNavigatedFrom Error: {ex.Message}")
        End Try
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Return True
    End Function
End Class
