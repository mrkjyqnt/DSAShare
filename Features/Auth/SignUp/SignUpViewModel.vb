Imports System.Windows.Threading
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports CommunityToolkit.Mvvm.Input

Public Class SignUpViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _dispatcher As Dispatcher
    Private ReadOnly _registrationService As IRegistrationService
    Private ReadOnly _loadingService As ILoadingService

    Public ReadOnly Property SignUpCommand As IAsyncRelayCommand
    Public ReadOnly Property SignInCommand As ICommand

    Private _name As String
    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            SetProperty(_name, value)
        End Set
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

    Private _rePassword As String
    Public Property RePassword As String
        Get
            Return _rePassword
        End Get
        Set(value As String)
            SetProperty(_rePassword, value)
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

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False ' View will not be kept alive
        End Get
    End Property

    ''' <summary>
    ''' Initializes a new instance of the <see cref="SignUpViewModel"/> class.
    ''' </summary>
    ''' <param name="regionManager"></param>
    ''' <param name="sessionManager"></param>
    Public Sub New(regionManager As IRegionManager, sessionManager As ISessionManager, registrationService As IRegistrationService)
        _regionManager = regionManager
        _registrationService = registrationService
        
        _dispatcher = Application.Current.Dispatcher
        _loadingService = New LoadingService(regionManager, _dispatcher)

        SignUpCommand = New AsyncRelayCommand(AddressOf OnSignUp)
        SignInCommand = New DelegateCommand(AddressOf OnSignIn)
    End Sub

    Private Async Function OnSignUp() As Task
        _loadingService.ShowLoading
        Status = ""

        If String.IsNullOrEmpty(Name) OrElse String.IsNullOrEmpty(Username) OrElse String.IsNullOrEmpty(Password) OrElse String.IsNullOrEmpty(RePassword) Then
            Status = "Username, password and re-enter password are required."
            Return
        End If

        If Password <> RePassword Then
            Status = "Password and re-enter password do not match."
            Return
        End If

        Try
            If Await Task.Run(Function() _registrationService.CheckUsername(Username)).ConfigureAwait(False) Then
                Status = "Username already exists."
                Return
            End If

            If Await Task.Run(Function() _registrationService.Register(Name, Username, Password)).ConfigureAwait(False) Then
                Status = "Registration successful."
                _dispatcher.Invoke(Sub()
                                       _regionManager.RequestNavigate("AuthenticationRegion", "SignInView")
                                       _regionManager.Regions("AuthenticationRegion").Remove("SignUpView")
                                   End Sub)
                Return
            Else
                Status = "An error occurred during login."
            End If
        Catch ex As Exception
            Status = "An error occurred during login."
            ErrorHandler.SetError(ex.Message)
        Finally
            _dispatcher.Invoke(Sub()
                _loadingService.HideLoading()
            End Sub)
        End Try

    End Function

    Private Sub OnSignIn()
        _regionManager.RequestNavigate("AuthenticationRegion", "SignInView")
    End Sub


End Class