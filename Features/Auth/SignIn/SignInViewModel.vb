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

    Public ReadOnly Property SignInCommand As AsyncRelayCommand
    Public ReadOnly Property SignUpCommand As DelegateCommand
    Public ReadOnly Property GuestLoginCommand As DelegateCommand

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False ' View will not be kept alive
        End Get
    End Property

    Private _username As String
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

    Private _password As String
    Public Property Password As String
        Get
            Return _password
        End Get
        Set(value As String)
            SetProperty(_password, value)
        End Set
    End Property

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
                                    PopUp.Information($"Failed", "Theres an error while activating the user, please check error log")
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

    Private Sub OnSignUp()
        _regionManager.RequestNavigate("AuthenticationRegion", "SignUpView")
    End Sub
End Class