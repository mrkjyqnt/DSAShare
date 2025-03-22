Imports System.Windows.Threading
Imports CommunityToolkit.Mvvm.Input
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Partial Class SignInViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _authenticationService As IAuthenticationService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _regionManager As IRegionManager

    Public ReadOnly Property SignInCommand As IAsyncRelayCommand
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

    Public Sub New(authenticationService As IAuthenticationService,
                   sessionManager As ISessionManager, 
                   regionManager As IRegionManager)

        _authenticationService = authenticationService
        _sessionManager = sessionManager
        _regionManager = regionManager

        SignInCommand = New AsyncRelayCommand(AddressOf OnSignInAsync)
        SignUpCommand = New DelegateCommand(AddressOf OnSignUp)
        GuestLoginCommand = New DelegateCommand(AddressOf OnGuestLogin)

    End Sub

    Private Async Function OnSignInAsync() As Task
        If String.IsNullOrEmpty(Username) OrElse String.IsNullOrEmpty(Password) Then
            PopUp.Information("Failed", "Please fill the Username and Password")
            Return
        End If

        Try
            Loading.Show()

            Dim isAuthenticated = Await Task.Run(Function() _authenticationService.Authenticate(Username, Password)).ConfigureAwait(True)

            If isAuthenticated Then
                PopUp.Information("Success", "Login Success")

                Try
                    _regionManager.RequestNavigate("MainRegion", "DashboardView")
                Catch ex As Exception
                    PopUp.Information("Navigation Error", ex.Message)
                End Try
            Else
                PopUp.Information("Failed", "Invalid credentials.")
            End If

        Catch ex As Exception
            PopUp.Information("Error", ex.Message)
        Finally
            Loading.Hide()
        End Try
    End Function

    Private Sub OnGuestLogin()
        Dim guestUser As New Users() With {
            .Username = "Guest",
            .Role = "Guest",
            .Status = "Active"
        }

        _sessionManager.Login(guestUser)
        PopUp.Information("Success", "Logged in as Guest.")
        _regionManager.RequestNavigate("MainRegion", "DashboardView")
    End Sub

    Private Sub OnSignUp()
        _regionManager.RequestNavigate("AuthenticationRegion", "SignUpView")
    End Sub
End Class