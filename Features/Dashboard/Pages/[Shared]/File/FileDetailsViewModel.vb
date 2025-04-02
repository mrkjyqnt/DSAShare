Imports DryIoc.FastExpressionCompiler.LightExpression
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions
Imports System.IO
Imports System.Net.WebRequestMethods
Imports System.Windows

Public Class FileDetailsViewModel
    Inherits BindableBase
    Implements INavigationAware
    Implements IRegionMemberLifetime

    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationService As INavigationService

    Private _parameters As NavigationParameters
    Private _file As FilesShared

    Private _fileNameText As String
    Private _detailsButtonVisibility As Visibility
    Private _settingsButtonVisibility As Visibility
    Private _dangerZoneButtonVisibility As Visibility

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
                    navigationService As INavigationService)
        _sessionManager = sessionManager
        _fileDataService = fileDataService
        _regionManager = regionManager
        _navigationService = navigationService

        _parameters = New NavigationParameters()

        BackCommand = New DelegateCommand(AddressOf OnBack)
        DetailsCommand = New DelegateCommand(AddressOf OnDetailsSelected)
        SettingsCommand = New DelegateCommand(AddressOf OnSettingSelected)
        DangerZoneCommand = New DelegateCommand(AddressOf OnDangerZoneSelected)
    End Sub

    ' Command implementations
    Private Sub OnBack()
        _navigationService.GoBack()
    End Sub

    Public Sub OnDetailsSelected()
        Try
            _regionManager.RequestNavigate("FileDetailsRegion", "FileDetailsContentView", _parameters)
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error navigating to FileDetailsContentView")
        End Try
    End Sub

    Public Sub OnSettingSelected()
        Try
            _regionManager.RequestNavigate("FileDetailsRegion", "FileSettingsView", _parameters)
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error navigating to FileSettingsView")
        End Try
    End Sub

    Public Sub OnDangerZoneSelected()
        Try
            _regionManager.RequestNavigate("FileDetailsRegion", "FileDangerZoneView", _parameters)
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error navigating to FileDangerZoneView")
        End Try
    End Sub

    Private Sub Load()
        Try
            If _sessionManager.CurrentUser.Role = "Admin" Then
                SettingsButtonVisibility = Visibility.Collapsed

                Return
            End If

            If Not _sessionManager.CurrentUser.Id = _file.UploadedBy Then
                SettingsButtonVisibility = Visibility.Collapsed
                DangerZoneButtonVisibility = Visibility.Collapsed

                Return
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error loading navigation")
            Debug.WriteLine($"[DEBUG] Message: {ex.Message}")
        End Try
    End Sub

    ' Navigation implementation
    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If navigationContext Is Nothing Then
                _navigationService.GoBack()
                Return
            End If

            If _parameters Is Nothing Then
                _parameters = New NavigationParameters()
            End If

            If navigationContext.Parameters.ContainsKey("fileId") Then
                Dim file = New FilesShared With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("fileId")
                }

                _file = Await Task.Run(Function() _fileDataService.GetSharedFileById(file)).ConfigureAwait(True)

                If IsNullOrEmpty(_file?.FileName) Then
                    Return
                End If

                FileNameText = _file.Name
                _parameters.Add("fileId", _file.Id)

                Load()
                OnDetailsSelected()
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error navigating to FileDetailsViewModel")
            _navigationService.GoBack()
        Finally
            Loading.Hide
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