Imports System.Windows.Threading
Imports CommunityToolkit.Mvvm.Input
Imports Microsoft.Office.Interop.Excel
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

#Disable Warning
Partial Public Class SignInViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _authenticationService As IAuthenticationService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _securityService As ISecurityService
    Private ReadOnly _userService As IUserService

    Private _username As String
    Private _password As String

    Private _securityQuestion1 As String
    Private _securityQuestion2 As String
    Private _securityQuestion3 As String
    Private _answerOneText As String
    Private _answerTwoText As String
    Private _answerThreeText As String

    Private _newPassword As String
    Private _reNewPassword As String

    Private _signInSectionVisibility As Visibility = Visibility.Visible
    Private _forgotPasswordSectionVisibility As Visibility = Visibility.Collapsed
    Private _changePasswordSectionVisibility As Visibility = Visibility.Collapsed
    Private _forgotPasswordVisibility As Visibility = Visibility.Collapsed

    Public Property Username As String
        Get
            Return _username
        End Get
        Set(value As String)
            If ValidateInput(value, "Username") Then
                SetProperty(_username, value)
            End If
        End Set
    End Property

    Public Property Password As String
        Get
            Return _password
        End Get
        Set(value As String)
            SetProperty(_password, value)
        End Set
    End Property

    Public Property SecurityQuestion1 As String
        Get
            Return _securityQuestion1
        End Get
        Set(value As String)
            SetProperty(_securityQuestion1, value)
        End Set
    End Property

    Public Property SecurityQuestion2 As String
        Get
            Return _securityQuestion2
        End Get
        Set(value As String)
            SetProperty(_securityQuestion2, value)
        End Set
    End Property

    Public Property SecurityQuestion3 As String
        Get
            Return _securityQuestion3
        End Get
        Set(value As String)
            SetProperty(_securityQuestion3, value)
        End Set
    End Property

    Public Property AnswerOneText As String
        Get
            Return _answerOneText
        End Get
        Set(value As String)
            SetProperty(_answerOneText, value)
        End Set
    End Property

    Public Property AnswerTwoText As String
        Get
            Return _answerTwoText
        End Get
        Set(value As String)
            SetProperty(_answerTwoText, value)
        End Set
    End Property

    Public Property AnswerThreeText As String
        Get
            Return _answerThreeText
        End Get
        Set(value As String)
            SetProperty(_answerThreeText, value)
        End Set
    End Property

    Public Property NewPassword As String
        Get
            Return _newPassword
        End Get
        Set(value As String)
            SetProperty(_newPassword, value)
        End Set
    End Property

    Public Property ReNewPassword As String
        Get
            Return _reNewPassword
        End Get
        Set(value As String)
            SetProperty(_reNewPassword, value)
        End Set
    End Property

    Public Property SignInSectionVisibility As Visibility
        Get
            Return _signInSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_signInSectionVisibility, value)
        End Set
    End Property

    Public Property ForgotPasswordSectionVisibility As Visibility
        Get
            Return _forgotPasswordSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_forgotPasswordSectionVisibility, value)
        End Set
    End Property

    Public Property ChangePasswordSectionVisibility As Visibility
        Get
            Return _changePasswordSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_changePasswordSectionVisibility, value)
        End Set
    End Property

    Public Property ForgotPasswordVisibility As Visibility
        Get
            Return _forgotPasswordVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_forgotPasswordVisibility, value)
        End Set
    End Property

    Public ReadOnly Property SignInCommand As AsyncRelayCommand
    Public ReadOnly Property SignUpCommand As DelegateCommand
    Public ReadOnly Property GuestLoginCommand As DelegateCommand
    Public ReadOnly Property BackCommand As DelegateCommand
    Public ReadOnly Property ForgotPasswordCommand As AsyncRelayCommand
    Public ReadOnly Property ContinueCommand As AsyncRelayCommand
    Public ReadOnly Property EnterCommand As AsyncRelayCommand

    Public Sub New(authenticationService As IAuthenticationService,
                   sessionManager As ISessionManager,
                   regionManager As IRegionManager,
                   securityServuce As ISecurityService,
                   userService As IUserService)

        _authenticationService = authenticationService
        _sessionManager = sessionManager
        _regionManager = regionManager
        _securityService = securityServuce
        _userService = userService

        SignInCommand = New AsyncRelayCommand(AddressOf OnSignInAsync)
        SignUpCommand = New DelegateCommand(AddressOf OnSignUp)
        GuestLoginCommand = New DelegateCommand(AddressOf OnGuestLogin)
        BackCommand = New DelegateCommand(AddressOf OnBack)
        ForgotPasswordCommand = New AsyncRelayCommand(AddressOf OnForgotPassword)
        ContinueCommand = New AsyncRelayCommand(AddressOf OnContinue)
        EnterCommand = New AsyncRelayCommand(AddressOf OnEnter)
    End Sub

    Private Async Function OnSignInAsync() As Task
        If String.IsNullOrEmpty(Username) OrElse String.IsNullOrEmpty(Password) Then
            Await PopUp.Information("Failed", "Please fill the Username and Password").ConfigureAwait(True)
            Return
        End If

        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(1000).ConfigureAwait(True)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not Await _securityService.SecurityCheck() Then
                Dim unlockTime = _securityService.GetLockUntil()
                Dim formattedTime = unlockTime?.ToString("hh:mm tt")
                ForgotPasswordVisibility = Visibility.Collapsed
                Await PopUp.Information("Failed", $"Logging in is temporarily locked until {formattedTime}. Please try again later.").ConfigureAwait(True)
                Return
            End If

            If Not Await Task.Run(Function() _authenticationService.IsUsernameExist(Username)).ConfigureAwait(True) Then
                Await PopUp.Information("Failed", "Username does not exist.").ConfigureAwait(True)
                Return
            End If

            Dim isAuthenticated = Await Task.Run(Function() _authenticationService.Authenticate(Username, Password)).ConfigureAwait(True)

            If isAuthenticated Then
                Dim user = Await Task.Run(Function() _userService.GetUserByUsername(New Users With {.Username = Username})).ConfigureAwait(True)

                If user.Status = "Deactivated" Then
                    Await PopUp.Information("Information", "This account has been deactivated, re-enter your password to activate again")
                    Dim maxAttempts As Integer = 3
                    Dim attempts As Integer = 0

                    While attempts < maxAttempts
                        attempts += 1

                        Dim popUpResult As PopupResult = Await PopUp.Confirmation()

                        If popUpResult Is Nothing Then
                            Await PopUp.Information("Cancelled", "Account activation was cancelled.")
                            Return
                            Exit Function
                        Else
                            If user.PasswordHash = HashPassword(popUpResult.GetValue(Of String)("Input")) Then
                                user.Status = "Active"
                                Dim result = Await Task.Run(Function() _userService.UpdateUser(user))

                                If Not result Then
                                    Await PopUp.Information($"Failed", "Theres an error while activating the user, please check error log")
                                    Return
                                End If

                                Exit While
                            Else
                                Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)")
                            End If
                        End If
                    End While
                End If

                Await PopUp.Information("Success", "Login Success").ConfigureAwait(True)
                _securityService.ResetAttempts()
                _sessionManager.Login(user)

                Try
                    Await Application.Current.Dispatcher.InvokeAsync(Sub() _regionManager.RequestNavigate("MainRegion", "DashboardView"))
                Catch ex As Exception
                    Debug.WriteLine($"[DEBUG] Error while navigating: {ex.Message}")
                End Try
            Else
                _securityService.RecordAttempts()
                Await PopUp.Information("Failed", "Wrong password.").ConfigureAwait(True)
                ForgotPasswordVisibility = Visibility.Visible
                Password = Nothing
            End If

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error while signing in: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Sub OnGuestLogin()
        Try
            Dim guestUser As New Users() With {
            .Id = 0,
            .Username = "Guest",
            .Name = "Guest User",
            .Role = "Guest",
            .Status = "Active"
        }

            Await Task.Run(Sub()
                               _sessionManager.Login(guestUser)
                           End Sub).ConfigureAwait(True)
            Await PopUp.Information("Success", "Logged in as Guest.").ConfigureAwait(True)
            _regionManager.RequestNavigate("MainRegion", "DashboardView")
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnBack()
        ForgotPasswordSectionVisibility = Visibility.Collapsed
        SignInSectionVisibility = Visibility.Visible
    End Sub

    Private Sub OnSignUp()
        _regionManager.RequestNavigate("AuthenticationRegion", "SignUpView")
    End Sub

    Private Async Function OnForgotPassword() As Task
        Try
            Loading.Show()

            SignInSectionVisibility = Visibility.Collapsed
            ForgotPasswordSectionVisibility = Visibility.Visible

            Dim user = Await Task.Run(Function() _userService.GetUserByUsername(New Users With {.Username = Username})).ConfigureAwait(True)

            SecurityQuestion1 = user.SecurityQuestion1
            SecurityQuestion2 = user.SecurityQuestion2
            SecurityQuestion3 = user.SecurityQuestion3
        Catch ex As Exception
            Debug.WriteLine($"[SignInViewModel] OnForgotPassword Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnContinue() As Task
        Try
            Loading.Show()

            Dim user = New Users With {
                .Username = Username,
                .SecurityAnswer1 = AnswerOneText,
                .SecurityAnswer2 = AnswerTwoText,
                .SecurityAnswer3 = AnswerThreeText
            }

            If Await Task.Run(Function() _userService.CheckSecurityAnswers(user)) Then
                ForgotPasswordSectionVisibility = Visibility.Collapsed
                ChangePasswordSectionVisibility = Visibility.Visible
            Else
                Await PopUp.Information("Failed", "Security answers do not match.").ConfigureAwait(True)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[SignInViewModel] OnContinue Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Async Function OnEnter() As Task
        If String.IsNullOrEmpty(Username) OrElse
            String.IsNullOrEmpty(NewPassword) OrElse
            String.IsNullOrEmpty(ReNewPassword) Then

            Await PopUp.Information("Failed", "Please fill all the boxes").ConfigureAwait(True)
            Return

        End If

        If NewPassword <> ReNewPassword Then
            Await PopUp.Information("Failed", "Password and re-enter password do not match.").ConfigureAwait(True)
            Return
        End If

        Try
            Loading.Show()

            Dim user = Await Task.Run(Function() _userService.GetUserByUsername(New Users With {.Username = Username})).ConfigureAwait(True)
            user.PasswordHash = HashPassword(NewPassword)
            Dim result = Await Task.Run(Function() _userService.UpdateUser(user)).ConfigureAwait(True)

            If result Then
                Await PopUp.Information("Success", "Password changed successfully.").ConfigureAwait(True)
                SignInSectionVisibility = Visibility.Visible
                ChangePasswordSectionVisibility = Visibility.Collapsed
                ForgotPasswordSectionVisibility = Visibility.Collapsed
                ForgotPasswordVisibility = Visibility.Collapsed
            Else
                Await PopUp.Information("Failed", "Failed to change password.").ConfigureAwait(True)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[SignInViewModel] OnEnter Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property
End Class