Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports System.Threading.Tasks

Public Partial Class SignInViewModel
    Inherits ObservableObject

    Private ReadOnly _authenticationService As IAuthenticationService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _regionManager As IRegionManager

    Public ReadOnly Property SignInCommand As IAsyncRelayCommand
    Public ReadOnly Property SignUpCommand As DelegateCommand
    Public ReadOnly Property GuestLoginCommand As DelegateCommand

    Private _username As String
    Public Property Username As String
        Get
            Return _username
        End Get
        Set(value As String)
            SetProperty(_username, value)
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

    Private _loginStatus As String
    Public Property LoginStatus As String
        Get
            Return _loginStatus
        End Get
        Set(value As String)
            SetProperty(_loginStatus, value)
        End Set
    End Property

    Public Sub New(authenticationService As IAuthenticationService, sessionManager As ISessionManager, regionManager As IRegionManager)
        _authenticationService = authenticationService
        _sessionManager = sessionManager
        _regionManager = regionManager
        SignInCommand = New AsyncRelayCommand(AddressOf OnSignInAsync)
        SignUpCommand = New DelegateCommand(AddressOf OnSignUp)
        GuestLoginCommand = New DelegateCommand(AddressOf OnGuestLogin)
    End Sub

    Private Async Function OnSignInAsync() As Task
        ' Validate input
        If String.IsNullOrEmpty(Username) OrElse String.IsNullOrEmpty(Password) Then
            LoginStatus = "Username and password are required."
            Return
        End If

        ' Authenticate the user asynchronously
        Try
            If Await Task.Run(Function() _authenticationService.Authenticate(Username, Password)).ConfigureAwait(False) Then
                Application.Current.Dispatcher.Invoke(Sub()
                    LoginStatus = "Login successful!"
                    _regionManager.RequestNavigate("MainRegion", "DashboardView")
                End Sub)
            Else
                Application.Current.Dispatcher.Invoke(Sub()
                    LoginStatus = "Invalid credentials."
                End Sub)
            End If
        Catch ex As Exception
            LoginStatus = "An error occurred during login."
        End Try
    End Function

    Private Sub OnGuestLogin()
        ' Log in as a guest
        Dim guestUser As New Users() With {
            .Username = "Guest",
            .Role = "Guest",
            .Status = "Active"
        }

        ' Log the guest user in
        _sessionManager.Login(guestUser)
        LoginStatus = "Logged in as Guest."
        _regionManager.RequestNavigate("MainRegion", "DashboardView")
    End Sub

    Private Sub OnSignUp()
        _regionManager.RequestNavigate("AuthenticationRegion", "SignUpView")
    End Sub
End Class
