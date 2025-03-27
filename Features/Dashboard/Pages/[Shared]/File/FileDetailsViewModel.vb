Imports DryIoc.FastExpressionCompiler.LightExpression
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions
Imports System.IO
Imports System.Windows

Public Class FileDetailsViewModel
    Inherits BindableBase
    Implements INavigationAware
    Implements IRegionMemberLifetime

    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _fileDownloadService As IFileDownloadService
    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _navigationService As INavigationService

    Private _parameters As NavigationParameters
    Private _file As FilesShared

    Private _fileNameText As String
    Private _saveChangesButtonVisibility As Visibility
    Private _removeAccessButtonVisibility As Visibility
    Private _downloadButtonVisibility As Visibility
    Private _detailsButtonVisibility As Visibility
    Private _settingsButtonVisibility As Visibility
    Private _dangerZoneButtonVisibility As Visibility
    Private _selectedFileId As Integer

    Public Property FileNameText As String
        Get
            Return _fileNameText
        End Get
        Set(value As String)
            SetProperty(_fileNameText, value)
        End Set
    End Property

    Public Property SaveChangesButtonVisibility As Visibility
        Get
            Return _saveChangesButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_saveChangesButtonVisibility, value)
        End Set
    End Property

    Public Property RemoveAccessButtonVisibility As Visibility
        Get
            Return _removeAccessButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_removeAccessButtonVisibility, value)
        End Set
    End Property

    Public Property DownloadButtonVisibility As Visibility
        Get
            Return _downloadButtonVisibility
        End Get
        Set(value As Visibility)
            SetProperty(_downloadButtonVisibility, value)
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

    Public ReadOnly Property BackCommand As ICommand
    Public ReadOnly Property DetailsCommand As ICommand
    Public ReadOnly Property SettingsCommand As ICommand
    Public ReadOnly Property DangerZoneCommand As ICommand
    Public ReadOnly Property DownloadCommand As ICommand

    Public Sub New(sessionManager As ISessionManager,
                    fileDataService As IFileDataService,
                    fileDownloadService As IFileDownloadService,
                    regionManager As IRegionManager,
                    navigationService As INavigationService)
        _sessionManager = sessionManager
        _fileDataService = fileDataService
        _fileDownloadService = fileDownloadService
        _regionManager = regionManager
        _navigationService = navigationService

        _parameters = New NavigationParameters()

        BackCommand = New DelegateCommand(AddressOf OnBack)
        DetailsCommand = New DelegateCommand(AddressOf OnDetailsSelected)
        SettingsCommand = New DelegateCommand(AddressOf OnSettingSelected)
        DangerZoneCommand = New DelegateCommand(AddressOf OnDangerZoneSelected)
        DownloadCommand = New DelegateCommand(AddressOf OnDownload)
    End Sub

    ' Command implementations
    Private Sub OnBack()
        _navigationService.GoBack()
    End Sub

    Public Sub OnDetailsSelected()
        Try
            ChangeButtonVisibility("Details")

            _regionManager.RequestNavigate("FileDetailsRegion", "FileDetailsContentView", _parameters)
        Catch ex As Exception

        End Try
    End Sub

    Public Sub OnSettingSelected()
        ' Navigate to Details view in the FileDetailsPage region
        ChangeButtonVisibility("Settings")

        '_regionManager.RequestNavigate("FileDetailsPage", "FileDetailsSettingsView", _parameters)
    End Sub

    Public Sub OnDangerZoneSelected()
        ' Navigate to Danger Zone view
        ChangeButtonVisibility("Danger Zone")

        '_regionManager.RequestNavigate("FileDetailsPage", "FileDetailsDangerZoneView", _parameters)
    End Sub

    Public Sub OnDownload()
        '_fileDownloadService.DownloadFile(_selectedFileId)
    End Sub

    Private Sub ChangeButtonVisibility(Page As String)
        Try
            SaveChangesButtonVisibility = Visibility.Collapsed
            RemoveAccessButtonVisibility = Visibility.Collapsed
            DownloadButtonVisibility = Visibility.Collapsed

            If _sessionManager.CurrentUser.Id = _file.UploadedBy Then
                Select Case Page
                    Case "Details"
                        DownloadButtonVisibility = Visibility.Visible
                    Case "Settings"
                        SaveChangesButtonVisibility = Visibility.Visible
                    Case "None"
                        Return
                End Select

                Return
            End If

            If _sessionManager.CurrentUser.Role = "Admin" Then
                Select Case Page
                    Case "Details"
                        DownloadButtonVisibility = Visibility.Visible
                    Case "None"
                        Return
                End Select
                Return
            End If

            If Not _sessionManager.CurrentUser.Id = _file.UploadedBy Then
                Select Case Page
                    Case "Details"
                        DownloadButtonVisibility = Visibility.Visible
                        RemoveAccessButtonVisibility = Visibility.Visible
                    Case "None"
                        Return
                End Select
            End If
        Catch ex As Exception

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
            Loading.Show()

            If navigationContext.Parameters.ContainsKey("fileId") Then
                Dim file = New FilesShared With {
                    .Id = navigationContext.Parameters.GetValue(Of Integer)("fileId")
                }

                _file = Await Task.Run(Function() _fileDataService.GetFileById(file)).ConfigureAwait(True)

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
        Finally
            Loading.Hide()
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