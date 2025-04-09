Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

#Disable Warning CRR0029
#Disable Warning BC42358
Public Class UserInformationViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime
    Implements INavigationAware

    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _userService As IUserService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _regionManager As IRegionManager

    Private _openedFrom As String
    Private _userDetails As Users

    Private _informationSectionVisibility As Visibility
    Private _passwordSectionVisibility As Visibility
    Private _roleSectionVisibility As Visibility

    Private _usernameText As String
    Private _nameText As String
    Private _currentPasswordText As String
    Private _newPasswordText As String
    Private _rePasswordText As String
    Private _isMemberSelected As Boolean
    Private _isAdminSelected As Boolean

    Public Property InformationSectionVisibility As Visibility
        Get
            Return _informationSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_informationSectionVisibility, value)
        End Set
    End Property

    Public Property PasswordSectionVisibility As Visibility
        Get
            Return _passwordSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_passwordSectionVisibility, value)
        End Set
    End Property

    Public Property RoleSectionVisibility As Visibility
        Get
            Return _roleSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_roleSectionVisibility, value)
        End Set
    End Property

    Public Property UsernameText As String
        Get
            Return _usernameText
        End Get
        Set(value As String)
            SetProperty(_usernameText, value)
        End Set
    End Property

    Public Property NameText As String
        Get
            Return _nameText
        End Get
        Set(value As String)
            SetProperty(_nameText, value)
        End Set
    End Property

    Public Property CurrentPasswordText As String
        Get
            Return _currentPasswordText
        End Get
        Set(value As String)
            SetProperty(_currentPasswordText, value)
        End Set
    End Property

    Public Property NewPasswordText As String
        Get
            Return _newPasswordText
        End Get
        Set(value As String)
            SetProperty(_newPasswordText, value)
        End Set
    End Property

    Public Property RePasswordText As String
        Get
            Return _rePasswordText
        End Get
        Set(value As String)
            SetProperty(_rePasswordText, value)
        End Set
    End Property

    Public Property IsMemberSelected As Boolean
        Get
            Return _isMemberSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isMemberSelected, value)
        End Set
    End Property

    Public Property IsAdminSelected As Boolean
        Get
            Return _isAdminSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isAdminSelected, value)
        End Set
    End Property

    Public ReadOnly Property InformationSaveButtonCommand As AsyncDelegateCommand
    Public ReadOnly Property PasswordSaveButtonCommand As AsyncDelegateCommand
    Public ReadOnly Property RoleSaveButtonCommand As AsyncDelegateCommand
    Public ReadOnly Property SignOutButtonCommand As AsyncDelegateCommand

    Public Sub New(sessionManager As ISessionManager,
                   userService As IUserService,
                   navigationService As INavigationService,
                   regionManager As IRegionManager)
        _sessionManager = sessionManager
        _userService = userService
        _navigationService = navigationService
        _regionManager = regionManager

        ' Initialize commands
        InformationSaveButtonCommand = New AsyncDelegateCommand(AddressOf OnSaveInformation)
        PasswordSaveButtonCommand = New AsyncDelegateCommand(AddressOf OnSavePassword)
        RoleSaveButtonCommand = New AsyncDelegateCommand(AddressOf OnSaveRole)
        SignOutButtonCommand = New AsyncDelegateCommand(AddressOf OnSignOut)
    End Sub

    Private Async Function OnSaveInformation() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100).ConfigureAwait(True)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If String.IsNullOrEmpty(UsernameText) Or String.IsNullOrEmpty(NameText) Then
                Await PopUp.Information("Failed", "Please fill in all fields")
                Return
            End If

            Dim user = Await Task.Run(Function() _userService.GetUserById(_userDetails)).ConfigureAwait(True)

            If user Is Nothing Then
                Await PopUp.Information("Failed", "User not found")
                Await Application.Current.Dispatcher.InvokeAsync(Sub() _navigationService.Go("MainRegion", "AuthenticationView"))
                Return
            End If

            If user.Name = NameText Then
                Await PopUp.Information("Failed", "No changes has been made")
                Return
            End If

            If Not Await Task.Run(Function() _userService.CheckPermission(user)).ConfigureAwait(True) Then
                Await PopUp.Information("Failed", "You do not have permission to update information")
                Return
            End If

            user.Name = NameText

            If Await Task.Run(Function() _userService.UpdateUser(user)).ConfigureAwait(True) Then
                Await PopUp.Information("Success", "Information updated successfully")
            Else
                Await PopUp.Information("Failed", "Failed to update user information")
            End If
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationView] OnSaveInformation Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnSavePassword() As Task
        Try
            If CurrentPasswordText Is Nothing Or NewPasswordText Is Nothing Or RePasswordText Is Nothing Then
                Await PopUp.Information("Failed", "Please fill in all fields")
                Return
            End If

            Dim user = Await Task.Run(Function() _userService.GetUserById(_userDetails)).ConfigureAwait(True)

            If user Is Nothing Then
                Await PopUp.Information("Failed", "User not found")
                Await Application.Current.Dispatcher.InvokeAsync(Sub() _navigationService.Go("MainRegion", "AuthenticationView"))
                Return
            End If

            If String.IsNullOrEmpty(CurrentPasswordText) Or String.IsNullOrEmpty(NewPasswordText) Or String.IsNullOrEmpty(RePasswordText) Then
                Await PopUp.Information("Failed", "Please fill in all fields")
                Return
            End If

            If Not user.PasswordHash = HashPassword(CurrentPasswordText) Then
                Await PopUp.Information("Failed", "Current Password is Incorrect")
                CurrentPasswordText = ""
                Return
            End If

            If user.PasswordHash = HashPassword(NewPasswordText) Then
                Await PopUp.Information("Failed", "New Password is the same with the Current Password")
                NewPasswordText = ""
                RePasswordText = ""
                Return
            End If

            If Not NewPasswordText = RePasswordText Then
                Await PopUp.Information("Failed", "New Password and Re-Password do not match")
                NewPasswordText = ""
                RePasswordText = ""
                Return
            End If

            If Not Await Task.Run(Function() _userService.CheckPermission(user)).ConfigureAwait(True) Then
                Await PopUp.Information("Failed", "You do not have permission to change the password")
                Return
            End If

            user.PasswordHash = HashPassword(NewPasswordText)

            If Await Task.Run(Function() _userService.UpdateUser(user)).ConfigureAwait(True) Then
                Await PopUp.Information("Success", "Password updated successfully")
            Else
                Await PopUp.Information("Failed", "Failed to update password")
            End If
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationView] OnSavePassword Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnSaveRole() As Task
        Try
            If False = IsMemberSelected And False = IsAdminSelected Then
                Await PopUp.Information("Failed", "Please select a role")
                Return
            End If

            Dim user = Await Task.Run(Function() _userService.GetUserById(_userDetails)).ConfigureAwait(True)

            If user Is Nothing Then
                Await PopUp.Information("Failed", "User not found")
                Await Application.Current.Dispatcher.InvokeAsync(Sub() _navigationService.Go("MainRegion", "AuthenticationView"))
                Return
            End If

            If Not Await Task.Run(Function() _userService.CheckPermission(user)).ConfigureAwait(True) Then
                Await PopUp.Information("Failed", "You do not have permission to change the role")
                Return
            End If

            If IsMemberSelected Then
                user.Role = "Member"
            End If

            If IsAdminSelected Then
                user.Role = "Admin"
            End If

            If Await Task.Run(Function() _userService.UpdateUser(user)).ConfigureAwait(True) Then
                Await PopUp.Information("Success", "Role updated successfully")
            Else
                Await PopUp.Information("Failed", "Failed to update role")
            End If
        Catch ex As Exception

        End Try
    End Function

    Private Async Function OnSignOut() As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100).ConfigureAwait(True)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            _sessionManager.Logout()
            For Each region In _regionManager.Regions
                region.RemoveAll()
            Next

            Await PopUp.Information("Success", "Application will restart for data refresh").ConfigureAwait(True)
            RestartApplication()
        Catch ex As Exception
            Debug.WriteLine($"[LogoutReset] Error: {ex.Message}")
            RestartApplication()
        End Try
    End Function

    Private Sub Load()
        Try
            If Not _openedFrom = "ManageUsersView" Then

                If _sessionManager.CurrentUser.Id = _userDetails.Id Then
                    RoleSectionVisibility = Visibility.Collapsed
                End If

                If _sessionManager.CurrentUser.Role = "Guest" Then
                    InformationSectionVisibility = Visibility.Collapsed
                    RoleSectionVisibility = Visibility.Collapsed
                    PasswordSectionVisibility = Visibility.Collapsed
                End If

                UpdateValues()
            End If

        Catch ex As Exception
            Debug.WriteLine($"[UserInformationView] Load Error: {ex.Message}")
        Finally
            RaisePropertyChanged(NameOf(InformationSectionVisibility))
            RaisePropertyChanged(NameOf(RoleSectionVisibility))
            RaisePropertyChanged(NameOf(PasswordSectionVisibility))
        End Try
    End Sub

    Private Sub UpdateValues()
        Try
            UsernameText = _userDetails.Username
            NameText = _userDetails.Name

            If _sessionManager.CurrentUser.Role = "Member" Then
                IsMemberSelected = True
                IsAdminSelected = False
            End If

            If _sessionManager.CurrentUser.Role = "Admin" Then
                IsMemberSelected = False
                IsAdminSelected = True
            End If
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationView] UpdateValues Error: {ex.Message}")
        Finally
            RaisePropertyChanged(NameOf(UsernameText))
            RaisePropertyChanged(NameOf(NameText))
            RaisePropertyChanged(NameOf(IsMemberSelected))
            RaisePropertyChanged(NameOf(IsAdminSelected))
        End Try
    End Sub

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

                Await Task.Delay(1000).ContinueWith(Sub() Application.Current.Dispatcher.InvokeAsync(Sub() Load()))
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