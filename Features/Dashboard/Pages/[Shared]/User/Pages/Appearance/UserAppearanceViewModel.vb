Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions

#Disable Warning
Public Class UserAppearanceViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime
    Implements INavigationAware

    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _userService As IUserService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _activityService As IActivityService

    Private _openedFrom As String
    Private _userDetails As Users
    Private _activity As Activities

    Private _isLightModeSelected As Boolean
    Private _isDarkModeSelected As Boolean
    Private _isSystemModeSelected As Boolean

    Public Property IsLightModeSelected As Boolean
        Get
            Return _isLightModeSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isLightModeSelected, value)
        End Set
    End Property

    Public Property IsDarkModeSelected As Boolean
        Get
            Return _isDarkModeSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isDarkModeSelected, value)
        End Set
    End Property

    Public Property IsSystemModeSelected As Boolean
        Get
            Return _isSystemModeSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isSystemModeSelected, value)
        End Set
    End Property

    Public ReadOnly Property SaveButtonCommand As AsyncDelegateCommand

    Public Sub New(sessionManager As ISessionManager,
                   userService As IUserService,
                   navigationService As INavigationService,
                   regionManager As IRegionManager,
                   activityService As ActivityService)
        _sessionManager = sessionManager
        _userService = userService
        _navigationService = navigationService
        _regionManager = regionManager
        _activityService = activityService

        ' Initialize commands
        SaveButtonCommand = New AsyncDelegateCommand(AddressOf OnSave)
    End Sub

    Private Async Sub Load()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100).ConfigureAwait(True)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            If _userDetails.AppAppearance = "Light" Then
                IsLightModeSelected = True
            ElseIf _userDetails.AppAppearance = "Dark" Then
                IsDarkModeSelected = True
            ElseIf _userDetails.AppAppearance = "System" Then
                IsSystemModeSelected = True
            End If

            Loading.Hide()
        Catch ex As Exception
        End Try
    End Sub

    Private Async Function OnSave() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100).ConfigureAwait(True)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            Dim user = Await Task.Run(Function() _userService.GetUserById(_userDetails)).ConfigureAwait(True)

            If user Is Nothing Then
                Await PopUp.Information("Failed", "User not found")
                Await Application.Current.Dispatcher.InvokeAsync(Sub() _navigationService.Go("MainRegion", "AuthenticationView"))
                Return
            End If

            If Not Await Task.Run(Function() _userService.CheckPermission(user)).ConfigureAwait(True) Then
                Await PopUp.Information("Failed", "You do not have permission to update appearance")
                Return
            End If

            If IsLightModeSelected Then
                user.AppAppearance = "Light"
            ElseIf IsDarkModeSelected Then
                user.AppAppearance = "Dark"
            ElseIf IsSystemModeSelected Then
                user.AppAppearance = "System"
            End If

            If Await Task.Run(Function() _userService.UpdateUser(user)).ConfigureAwait(True) Then
                Await PopUp.Information("Success", "Appearance updated successfully")
            Else
                Await PopUp.Information("Failed", "Failed to update user information")
                Return
            End If

            _activity = New Activities With {
                .Action = "Change appearance",
                .ActionIn = "Account",
                .ActionAt = Date.Now,
                .AccountId = user.Id,
                .Name = user.Name,
                .UserId = _sessionManager.CurrentUser.Id
            }

            Await Task.Run(Function() _activityService.AddActivity(_activity)).ConfigureAwait(True)
            
            Await Application.Current.Dispatcher.InvokeAsync(Sub()
                                                                 ThemeHelper.ApplyTheme(ThemeHelper.GetThemeFromString(user.AppAppearance))
                                                             End Sub)

            Loading.Hide()
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationView] OnSaveInformation Error: {ex.Message}")
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

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            If navigationContext Is Nothing Then
                Await PopUp.Information("Failed", "NavigationContext is null")
                Return
            End If

            If navigationContext.Parameters.ContainsKey("userId") And navigationContext.Parameters.ContainsKey("openedFrom") Then
                Dim user = New Users With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("userId")
                }

                _openedFrom = navigationContext.Parameters.GetValue(Of String)("openedFrom")

                If Not _sessionManager.CurrentUser.Role = "Guest" Then

                    _userDetails = Await Task.Run(Function() _userService.GetUserById(user)).ConfigureAwait(True)

                    If IsNullOrEmpty(_userDetails?.Id) Then
                        Await PopUp.Information("Failed", "User not found")
                        _navigationService.GoBack()
                        Return
                    End If

                End If

                Await Task.Run(Sub() Application.Current.Dispatcher.InvokeAsync(Sub() Load()))
                Loading.Hide()
            End If
        Catch ex As Exception
            Debug.WriteLine($"[Debug] Error navigating to FileDetailsContentModel: {ex.Message}")
            PopUp.Information("Failed", "An error occurred while navigating to the user information view.")
            _navigationService.GoBack()
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