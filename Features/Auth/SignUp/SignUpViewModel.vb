Imports System.Windows.Threading
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports CommunityToolkit.Mvvm.Input
Imports System.Collections.ObjectModel
Imports Microsoft.VisualBasic.ApplicationServices

#Disable Warning
Public Class SignUpViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _registrationService As IRegistrationService

    Private _name As String
    Private _username As String
    Private _password As String
    Private _rePassword As String
    Private _status As String
    Private _answerOneText As String
    Private _answerTwoText As String
    Private _answerThreeText As String

    Private _availableQuestions As New ObservableCollection(Of String)()
    Private _selectedQuestion1 As String
    Private _selectedQuestion2 As String
    Private _selectedQuestion3 As String

    Private _stepOneSectionVisibility As Visibility = Visibility.Visible
    Private _stepTwoSectionVisibility As Visibility = Visibility.Collapsed

    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            SetProperty(_name, value)
        End Set
    End Property

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

    Public Property RePassword As String
        Get
            Return _rePassword
        End Get
        Set(value As String)
            SetProperty(_rePassword, value)
        End Set
    End Property

    Public Property Status As String
        Get
            Return _status
        End Get
        Set(value As String)
            SetProperty(_status, value)
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

    Public Property AvailableQuestions As ObservableCollection(Of String)
        Get
            Return _availableQuestions
        End Get
        Set(value As ObservableCollection(Of String))
            SetProperty(_availableQuestions, value)
        End Set
    End Property

    Public Property SelectedQuestion1 As String
        Get
            Return _selectedQuestion1
        End Get
        Set(value As String)
            If SetProperty(_selectedQuestion1, value) Then
                RaisePropertyChanged(NameOf(FilteredQuestions2))
                RaisePropertyChanged(NameOf(FilteredQuestions3))
            End If
        End Set
    End Property

    Public Property SelectedQuestion2 As String
        Get
            Return _selectedQuestion2
        End Get
        Set(value As String)
            If SetProperty(_selectedQuestion2, value) Then
                RaisePropertyChanged(NameOf(FilteredQuestions1))
                RaisePropertyChanged(NameOf(FilteredQuestions3))
            End If
        End Set
    End Property

    Public Property SelectedQuestion3 As String
        Get
            Return _selectedQuestion3
        End Get
        Set(value As String)
            If SetProperty(_selectedQuestion3, value) Then
                RaisePropertyChanged(NameOf(FilteredQuestions1))
                RaisePropertyChanged(NameOf(FilteredQuestions2))
            End If
        End Set
    End Property

    Public ReadOnly Property FilteredQuestions1 As IEnumerable(Of String)
        Get
            Return AvailableQuestions.Where(Function(q) q <> SelectedQuestion2 AndAlso q <> SelectedQuestion3).ToList()
        End Get
    End Property

    Public ReadOnly Property FilteredQuestions2 As IEnumerable(Of String)
        Get
            Return AvailableQuestions.Where(Function(q) q <> SelectedQuestion1 AndAlso q <> SelectedQuestion3).ToList()
        End Get
    End Property

    Public ReadOnly Property FilteredQuestions3 As IEnumerable(Of String)
        Get
            Return AvailableQuestions.Where(Function(q) q <> SelectedQuestion1 AndAlso q <> SelectedQuestion2).ToList()
        End Get
    End Property

    Public Property StepOneSectionVisibility As Visibility
        Get
            Return _stepOneSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_stepOneSectionVisibility, value)
        End Set
    End Property

    Public Property StepTwoSectionVisibility As Visibility
        Get
            Return _stepTwoSectionVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_stepTwoSectionVisibility, value)
        End Set
    End Property

    Public ReadOnly Property SignUpCommand As AsyncRelayCommand
    Public ReadOnly Property SignInCommand As DelegateCommand
    Public ReadOnly Property NextCommand As DelegateCommand
    Public ReadOnly Property BackCommand As DelegateCommand

    Public Sub New(regionManager As IRegionManager,
                   sessionManager As ISessionManager,
                   registrationService As IRegistrationService,
                   popupService As IPopupService)

        _regionManager = regionManager
        _registrationService = registrationService

        AvailableQuestions.Add("What was your first pet's name?")
        AvailableQuestions.Add("What was your mother's maiden name?")
        AvailableQuestions.Add("What was the name of your first school?")
        AvailableQuestions.Add("What city were you born in?")
        AvailableQuestions.Add("What was your childhood nickname?")

        SignUpCommand = New AsyncRelayCommand(AddressOf OnSignUp)
        SignInCommand = New DelegateCommand(AddressOf OnSignIn)
        NextCommand = New DelegateCommand(AddressOf OnNext)
        BackCommand = New DelegateCommand(AddressOf OnBack)
    End Sub
    Private Async Function OnSignUp() As Task
        If String.IsNullOrEmpty(Name) OrElse
        String.IsNullOrEmpty(Username) OrElse
        String.IsNullOrEmpty(Password) OrElse
        String.IsNullOrEmpty(RePassword) OrElse
        String.IsNullOrEmpty(SelectedQuestion1) OrElse
        String.IsNullOrEmpty(SelectedQuestion1) OrElse
        String.IsNullOrEmpty(SelectedQuestion1) OrElse
                    String.IsNullOrEmpty(AnswerOneText) OrElse
                    String.IsNullOrEmpty(AnswerTwoText) OrElse
                    String.IsNullOrEmpty(AnswerThreeText) Then
            Await PopUp.Information("Failed", "Please fill all the boxes").ConfigureAwait(True)
            Return
        End If

        If Password <> RePassword Then
            Await PopUp.Information("Failed", "Password and re-enter password do not match.").ConfigureAwait(True)
            Return
        End If

        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Await Task.Run(Function() _registrationService.CheckUsername(Username)).ConfigureAwait(True) Then
                Await PopUp.Information("Failed", "Username already exists.").ConfigureAwait(True)
                Return
            End If

            Dim user = New Users() With {
                .Name = Name,
                .Username = Username,
                .PasswordHash = HashPassword(Password),
                .SecurityQuestion1 = SelectedQuestion1,
                .SecurityQuestion2 = SelectedQuestion2,
                .SecurityQuestion3 = SelectedQuestion3,
                .SecurityAnswer1 = LCase(AnswerOneText),
                .SecurityAnswer2 = LCase(AnswerTwoText),
                .SecurityAnswer3 = LCase(AnswerThreeText),
                .Role = "Member",
                .Status = "Active",
                .CreatedAt = DateTime.Now
            }

            If Await Task.Run(Function() _registrationService.Register(user)).ConfigureAwait(True) Then
                Await PopUp.Information("Success", "Registration successful.").ConfigureAwait(True)
                _regionManager.RequestNavigate("AuthenticationRegion", "SignInView")
                _regionManager.Regions("AuthenticationRegion").Remove("SignUpView")
                Return
            Else
                Await PopUp.Information("Error", "An error occurred during registration.").ConfigureAwait(True)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] There was an error while signing up: {ex.Message}")
        Finally
            Loading.Hide()
        End Try

    End Function

    Private Sub OnSignIn()
        _regionManager.RequestNavigate("AuthenticationRegion", "SignInView")
    End Sub

    Private Sub OnBack()
        StepOneSectionVisibility = Visibility.Visible
        StepTwoSectionVisibility = Visibility.Collapsed
    End Sub

    Private Sub OnNext()
        StepOneSectionVisibility = Visibility.Collapsed
        StepTwoSectionVisibility = Visibility.Visible
    End Sub

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

End Class