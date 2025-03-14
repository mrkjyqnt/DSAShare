Imports System.Windows.Threading
Imports CommunityToolkit.Mvvm.Input
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Partial Class SignInViewModel
    Inherits BindableBase

    Private ReadOnly _authenticationService As IAuthenticationService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _dispatcher As Dispatcher

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

    Private _status As String
    Public Property Status As String
        Get
            Return _status
        End Get
        Set(value As String)
            SetProperty(_status, value)
        End Set
    End Property

    Public Sub New(authenticationService As IAuthenticationService, sessionManager As ISessionManager, regionManager As IRegionManager)
        _authenticationService = authenticationService
        _sessionManager = sessionManager
        _regionManager = regionManager
        _dispatcher = Application.Current.Dispatcher

        SignInCommand = New AsyncRelayCommand(AddressOf OnSignInAsync)
        SignUpCommand = New DelegateCommand(AddressOf OnSignUp)
        GuestLoginCommand = New DelegateCommand(AddressOf OnGuestLogin)
    End Sub

    Private Async Function OnSignInAsync() As Task
        ' Validate input
        If String.IsNullOrEmpty(Username) OrElse String.IsNullOrEmpty(Password) Then
            Status = "Username and password are required."
            Return
        End If

        ' Authenticate the user asynchronously
        Try
            If Await Task.Run(Function() _authenticationService.Authenticate(Username, Password)).ConfigureAwait(False) Then
                _dispatcher.Invoke(Sub()
                                       Status = "Login successful!"
                                       _regionManager.RequestNavigate("MainRegion", "DashboardView")
                                   End Sub)
            Else
                _dispatcher.Invoke(Sub()
                                       Status = "Invalid credentials."
                                   End Sub)
            End If
        Catch ex As Exception
            Status = "An error occurred during login."
            ErrorHandler.SetError(ex.Message)
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
        Status = "Logged in as Guest."
        _regionManager.RequestNavigate("MainRegion", "DashboardView")
    End Sub

    Private Sub OnSignUp()
        _regionManager.RequestNavigate("AuthenticationRegion", "SignUpView")
    End Sub
End Class
