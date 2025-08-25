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
    Private _activitiesButtonVisibility As Visibility
    Private _appearanceButtonVisibility As Visibility
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

    Public Property AppearanceButtonVisibility As Visibility
        Get
            Return _appearanceButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_appearanceButtonVisibility, value)
        End Set
    End Property
    Public Property ActivitiesButtonVisibility As Visibility
        Get
            Return _activitiesButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_activitiesButtonVisibility, value)
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
    Public ReadOnly Property AppearanceCommand As DelegateCommand
    Public ReadOnly Property DangerZoneCommand As DelegateCommand
    Public ReadOnly Property ActivitiesCommand As DelegateCommand

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
        AppearanceCommand = New DelegateCommand(AddressOf OnAppearanceSelected)
        DangerZoneCommand = New DelegateCommand(AddressOf OnDangerZoneSelected)
        ActivitiesCommand = New DelegateCommand(AddressOf OnActivitiesSelected)
    End Sub

    ' Command implementations
    Private Sub OnBack()
        Try
            _navigationService.GoBack()
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationViewModel] OnBack Error: {ex.Message}")
        End Try
    End Sub

    Private Async Sub OnInformationSelected()
        Try
            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            Await Application.Current.Dispatcher.InvokeAsync(Sub() _regionManager.RequestNavigate("UserPageRegion", "UserInformationView", _parameters))
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] OnInformationSelected Error navigating to UserInformationView: {ex.Message}")
        End Try
    End Sub

    Public Async Sub OnAppearanceSelected()
        Try
            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            Await Application.Current.Dispatcher.InvokeAsync(Sub() _regionManager.RequestNavigate("UserPageRegion", "UserAppearanceView", _parameters))
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] OnAppearanceSelected Error navigating to UserAppearanceView")
        End Try
    End Sub

    Public Async Sub OnDangerZoneSelected()
        Try
            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            Await Application.Current.Dispatcher.InvokeAsync(Sub() _regionManager.RequestNavigate("UserPageRegion", "UserDangerZoneView", _parameters))
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] OnDangerZoneSelected Error navigating to UserDangerZoneView")
        End Try
    End Sub

    Public Async Sub OnActivitiesSelected()
        Try
            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            Await Application.Current.Dispatcher.InvokeAsync(Sub() _regionManager.RequestNavigate("UserPageRegion", "UserActivitiesView", _parameters))
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] OnDangerZoneSelected Error navigating to UserActivitiesView")
        End Try
    End Sub

    Private Sub Load()
        Try
            If Not _openedFrom = "ManageUsersView" Then

                If _userDetails Is Nothing OrElse _sessionManager.CurrentUser.Id = _userDetails.Id OrElse _sessionManager.CurrentUser.Role = "Guest" Then
                    OtherSettingsVisibility = Visibility.Collapsed
                    ActivitiesButtonVisibility = Visibility.Collapsed

                    If _sessionManager.CurrentUser.Role = "Guest" Then
                        AppearanceButtonVisibility = Visibility.Collapsed
                        DangerZoneButtonVisibility = Visibility.Collapsed
                    End If

                End If
                Return
            End If

            NameText = _userDetails.Name
            DefaultSettingsVisibility = Visibility.Collapsed
            AppearanceButtonVisibility = Visibility.Collapsed
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationsViewModel] Load Error: {ex.Message}")
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

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
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

                End If

                _parameters.Add("userId", navigationContext.Parameters.GetValue(Of Integer)("userId"))
                _parameters.Add("openedFrom", navigationContext.Parameters.GetValue(Of String)("openedFrom"))

                Await Application.Current.Dispatcher.InvokeAsync(Sub()
                                                                     Load()
                                                                     OnInformationSelected()
                                                                 End Sub)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[UserInformationsViewModel] Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedFrom
        Try
            ' Remove pageregion view if UserInformationView exist
            Dim region = _regionManager.Regions("PageRegion")
            Dim view = region.Views.FirstOrDefault(Function(v) v.GetType() = GetType(UserInformationView))
            If view IsNot Nothing Then
                region.Remove(view)
            End If
        Catch ex As Exception

        End Try
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