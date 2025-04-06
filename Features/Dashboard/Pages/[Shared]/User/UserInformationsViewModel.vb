Imports Microsoft.VisualBasic.ApplicationServices
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions

#Disable Warning CRR0029
Public Class UserInformationsViewModel
    Inherits BindableBase
    Implements INavigationAware
    Implements IRegionMemberLifetime

    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _userService As IUserService
    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationService As INavigationService

    Private _openedFrom As String
    Private _parameters As NavigationParameters
    Private _userDetails As Users

    Private _defaultSettingsVisibility As Visibility
    Private _otherSettingsVisibility As Visibility
    Private _dangerZoneButtonVisibility As Visibility
    Private _nameText As String

    Public Property DefaultSettingsVisibility As Visibility
        Get
            Return _defaultSettingsVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_defaultSettingsVisibility, value)
        End Set
    End Property

    Public Property OtherSettingsVisibility As Visibility
        Get
            Return _otherSettingsVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_otherSettingsVisibility, value)
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

    Public Property DangerZoneButtonVisibility As Visibility
        Get
            Return _dangerZoneButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_dangerZoneButtonVisibility, value)
        End Set
    End Property

    Public ReadOnly Property BackCommand As DelegateCommand
    Public ReadOnly Property InformationCommand As DelegateCommand
    Public ReadOnly Property DangerZoneCommand As DelegateCommand

    Public Sub New(sessionManager As ISessionManager,
                    userService As IUserService,
                    regionManager As IRegionManager,
                    navigationService As INavigationService)
        _sessionManager = sessionManager
        _userService = userService
        _regionManager = regionManager
        _navigationService = navigationService

        _parameters = New NavigationParameters()

        BackCommand = New DelegateCommand(AddressOf OnBack)
        InformationCommand = New DelegateCommand(AddressOf OnInformationSelected)
        DangerZoneCommand = New DelegateCommand(AddressOf OnDangerZoneSelected)
    End Sub

    ' Command implementations
    Private Async Sub OnBack()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() _navigationService.GoBack())
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationViewModel] OnBack Error: {ex.Message}")
        End Try
    End Sub

    Public Async Sub OnInformationSelected()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() _regionManager.RequestNavigate("UserPageRegion", "UserInformationView", _parameters))
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] OnDetailsSelected Error navigating to FileDetailsContentView")
        End Try
    End Sub

    Public Async Sub OnDangerZoneSelected()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() _regionManager.RequestNavigate("UserPageRegion", "UserInformationView", _parameters))
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] OnDangerZoneSelected Error navigating to FileDangerZoneView")
        End Try
    End Sub

    Private Sub Load()
        Try
            If Not _openedFrom = "ManageUsersView" Then
                NameText = _userDetails.Name

                If _sessionManager.CurrentUser.Id = _userDetails.Id Then
                    OtherSettingsVisibility = Visibility.Collapsed
                Else
                    DefaultSettingsVisibility = Visibility.Collapsed
                End If

                If _sessionManager.CurrentUser.Role = "Guest" Then
                    DangerZoneButtonVisibility = Visibility.Collapsed
                End If

            End If

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error loading navigation")
            Debug.WriteLine($"[DEBUG] Message: {ex.Message}")
        Finally
            RaisePropertyChanged(NameOf(NameText))
            RaisePropertyChanged(NameOf(DangerZoneButtonVisibility))
            RaisePropertyChanged(NameOf(DefaultSettingsVisibility))
            RaisePropertyChanged(NameOf(OtherSettingsVisibility))
        End Try
    End Sub

    ' Navigation implementation
    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo
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

            If _parameters Is Nothing Then
                _parameters = New NavigationParameters()
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
                Else
                    _userDetails = _sessionManager.CurrentUser
                End If

                _parameters.Add("userId", navigationContext.Parameters.GetValue(Of Integer)("userId"))
                _parameters.Add("openedFrom", navigationContext.Parameters.GetValue(Of String)("openedFrom"))

                Await Application.Current.Dispatcher.InvokeAsync(Sub() Load())
                Await Task.Delay(1000).ConfigureAwait(True)
                OnInformationSelected()
                Loading.Hide()
            End If
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationsViewModel] Error: {ex.Message}")
        End Try
    End Sub

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedFrom
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements INavigationAware.IsNavigationTarget
        Return True
    End Function

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property
End Class
