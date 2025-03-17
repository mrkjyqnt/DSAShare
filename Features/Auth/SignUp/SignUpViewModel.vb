Imports System.Windows.Threading
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports CommunityToolkit.Mvvm.Input

Public Class SignUpViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _registrationService As IRegistrationService

    Public ReadOnly Property SignUpCommand As IAsyncRelayCommand
    Public ReadOnly Property SignInCommand As ICommand

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

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

    ''' <summary>
    ''' Initializes a new instance of the <see cref="SignUpViewModel"/> class.
    ''' </summary>
    ''' <param name="regionManager"></param>
    ''' <param name="sessionManager"></param>
    Public Sub New(regionManager As IRegionManager, 
                   sessionManager As ISessionManager, 
                   registrationService As IRegistrationService,
                   popupService As IPopupService)

        _regionManager = regionManager
        _registrationService = registrationService
       
        SignUpCommand = New AsyncRelayCommand(AddressOf OnSignUp)
        SignInCommand = New DelegateCommand(AddressOf OnSignIn)
    End Sub

    Private Async Function OnSignUp() As Task
        If String.IsNullOrEmpty(Name) OrElse String.IsNullOrEmpty(Username) OrElse String.IsNullOrEmpty(Password) OrElse String.IsNullOrEmpty(RePassword) Then
            PopUp.Information("Failed", "Please fill all the boxes")
            Return
        End If

        If Password <> RePassword Then
            PopUp.Information("Failed", "Password and re-enter password do not match.")
            Return
        End If

        Try
            Loading.Show()            

            If Await Task.Run(Function() _registrationService.CheckUsername(Username)).ConfigureAwait(True) Then
                PopUp.Information("Failed", "Username already exists.")
                Return
            End If

            If Await Task.Run(Function() _registrationService.Register(Name, Username, Password)).ConfigureAwait(True) Then
                PopUp.Information("Success", "Registration successful.")
                _regionManager.RequestNavigate("AuthenticationRegion", "SignInView")
                _regionManager.Regions("AuthenticationRegion").Remove("SignUpView")
                Return
            Else
                PopUp.Information("Error", "An error occurred during login.")
            End If
        Catch ex As Exception
            PopUp.Information("Error", "An error occurred during login.")
            ErrorHandler.SetError(ex.Message)
        Finally
            Loading.Hide()
        End Try

    End Function

    Private Sub OnSignIn()
        _regionManager.RequestNavigate("AuthenticationRegion", "SignInView")
    End Sub


End Class