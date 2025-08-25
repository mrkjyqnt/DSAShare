Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions

#Disable Warning
Public Class FileDetailsViewModel
    Inherits BindableBase
    Implements INavigationAware
    Implements IRegionMemberLifetime

    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _userService As IUserService

    Private _openedFrom As String
    Private _parameters As NavigationParameters
    Private _file As FilesShared

    Private _fileNameText As String
    Private _detailsButtonVisibility As Visibility = Visibility.Collapsed
    Private _settingsButtonVisibility As Visibility = Visibility.Collapsed
    Private _dangerZoneButtonVisibility As Visibility = Visibility.Collapsed

    Public Property FileNameText As String
        Get
            Return _fileNameText
        End Get
        Set(value As String)
            SetProperty(_fileNameText, value)
        End Set
    End Property

    Public Property DetailsButtonVisibility As Visibility
        Get
            Return _detailsButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_detailsButtonVisibility, value)
        End Set
    End Property

    Public Property SettingsButtonVisibility As Visibility
        Get
            Return _settingsButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_settingsButtonVisibility, value)
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
    Public ReadOnly Property DetailsCommand As DelegateCommand
    Public ReadOnly Property SettingsCommand As DelegateCommand
    Public ReadOnly Property DangerZoneCommand As DelegateCommand

    Public Sub New(sessionManager As ISessionManager,
                    fileDataService As IFileDataService,
                    regionManager As IRegionManager,
                    navigationService As INavigationService,
                    userService As IUserService)
        _sessionManager = sessionManager
        _fileDataService = fileDataService
        _regionManager = regionManager
        _navigationService = navigationService
        _userService = userService

        _parameters = New NavigationParameters()

        BackCommand = New DelegateCommand(AddressOf OnBack)
        DetailsCommand = New DelegateCommand(AddressOf OnDetailsSelected)
        SettingsCommand = New DelegateCommand(AddressOf OnSettingSelected)
        DangerZoneCommand = New DelegateCommand(AddressOf OnDangerZoneSelected)
    End Sub

    ' Command implementations
    Private Async Sub OnBack()
        _navigationService.GoBack()
    End Sub

    Public Async Sub OnDetailsSelected()
        Try
            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            _regionManager.RequestNavigate("FileDetailsRegion", "FileDetailsContentView", _parameters)
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] OnDetailsSelected Error navigating to FileDetailsContentView")
        End Try
    End Sub

    Public Async Sub OnSettingSelected()
        Try
            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            _regionManager.RequestNavigate("FileDetailsRegion", "FileSettingsView", _parameters)
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] OnSettingSelected Error navigating to FileSettingsView")
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

            _regionManager.RequestNavigate("FileDetailsRegion", "FileDangerZoneView", _parameters)
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] OnDangerZoneSelected Error navigating to FileDangerZoneView")
        End Try
    End Sub

    Private Async Sub Load()
        Try
            If _sessionManager.CurrentUser.Id = _file.UploadedBy Then
                DetailsButtonVisibility = Visibility.Visible
                SettingsButtonVisibility = Visibility.Visible
                DangerZoneButtonVisibility = Visibility.Visible
            Else
                DetailsButtonVisibility = Visibility.Visible
            End If

            If _openedFrom = "ManageFilesView" Then
                DetailsButtonVisibility = Visibility.Visible
                DangerZoneButtonVisibility = Visibility.Visible
            End If

            OnDetailsSelected()
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error loading navigation")
            Debug.WriteLine($"[DEBUG] Message: {ex.Message}")
        End Try
    End Sub

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo
        Try
            Loading.Show()
            Await Task.Delay(1000).ConfigureAwait(True)

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
                _navigationService.GoBack()
                Return
            End If

            If _parameters Is Nothing Then
                _parameters = New NavigationParameters()
            End If

            If Not navigationContext.Parameters.ContainsKey("fileId") Then
                Await PopUp.Information("Failed", "File was empty")
                _navigationService.GoBack()
                Return
            End If

            If Not navigationContext.Parameters.ContainsKey("openedFrom") Then
                Await PopUp.Information("Failed", $"Navigation history is empty {navigationContext.Parameters.GetValue(Of String)("openedFrom")}")
                _navigationService.GoBack()
                Return
            End If

            If navigationContext.Parameters.ContainsKey("fileId") Then
                Dim file = New FilesShared With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("fileId")
                }

                _file = Await Task.Run(Function() _fileDataService.GetSharedFileById(file)).ConfigureAwait(False)

                If IsNullOrEmpty(_file?.FileName) Then
                    Return
                End If

                If navigationContext.Parameters.ContainsKey("openedFrom") Then
                    _openedFrom = navigationContext.Parameters.GetValue(Of String)("openedFrom")
                End If

                FileNameText = _file.Name

                _parameters.Add("fileId", navigationContext.Parameters.GetValue(Of Integer)("fileId"))
                _parameters.Add("openedFrom", navigationContext.Parameters.GetValue(Of String)("openedFrom"))

                Await Application.Current.Dispatcher.InvokeAsync(Sub() Load()).Task.ConfigureAwait(False)
            End If

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error navigating to FileDetailsViewModel")
            _navigationService.GoBack()
        Finally
            Loading.Hide()
        End Try
    End Sub

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedFrom
        Try
            Loading.Hide()
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error navigating from FileDetailsViewModel")
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