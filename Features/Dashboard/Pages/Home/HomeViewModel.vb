Imports System.Windows.Threading
Imports Prism.Commands
Imports Prism.Events
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports System.Collections.ObjectModel
Imports DryIoc.FastExpressionCompiler.LightExpression
Imports System.IO
Imports Prism.Navigation

Public Class HomeViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _sessionManager As ISessionManager

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Private _publicText As String
    Public Property PublicText As String
        Get
            Return _publicText
        End Get
        Set(value As String)
            SetProperty(_publicText, value)
        End Set
    End Property

    Private _sharedText As String
    Public Property SharedText As String
        Get
            Return _sharedText
        End Get
        Set(value As String)
            SetProperty(_sharedText, value)
        End Set
    End Property

    Private _accessedText As String
    Public Property AccessedText As String
        Get
            Return _accessedText
        End Get
        Set(value As String)
            SetProperty(_accessedText, value)
        End Set
    End Property


    Private _dataGridActivities As ObservableCollection(Of ActivityServiceModel)
    Public Property DataGridActivities As ObservableCollection(Of ActivityServiceModel)
        Get
            Return _dataGridActivities
        End Get
        Set(value As ObservableCollection(Of ActivityServiceModel))
            SetProperty(_dataGridActivities, value)
        End Set
    End Property

    Private _selectedActivity As ActivityServiceModel
    Public Property SelectedActivity As ActivityServiceModel
        Get
            Return _selectedActivity
        End Get
        Set(value As ActivityServiceModel)
            If SetProperty(_selectedActivity, value) AndAlso value IsNot Nothing Then
                ' Execute the command
                If SelectedActivityCommand.CanExecute(value) Then
                    SelectedActivityCommand.Execute(value)
                End If
                ' Clear the selection to deselect the DataGrid row
                SetProperty(_selectedActivity, Nothing)
            End If
        End Set
    End Property

    Public ReadOnly Property SelectedActivityCommand As ICommand
    Public ReadOnly Property ShareFilesCommand As DelegateCommand
    Public ReadOnly Property AccessFilesCommand As DelegateCommand

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sessionManager"></param>
    ''' <param name="regionManager"></param>
    ''' <param name="fileDataService"></param>
    Public Sub New(fileDataService As IFileDataService,
                   activityService As IActivityService,
                   navigationService As INavigationService,
                   sessionManager As ISessionManager)

        _fileDataService = fileDataService
        _activityService = activityService
        _navigationService = navigationService
        _sessionManager = sessionManager

        ' Initialize commands
        DataGridActivities = New ObservableCollection(Of ActivityServiceModel)()
        SelectedActivityCommand = New DelegateCommand(Of ActivityServiceModel)(AddressOf OnActivitySelected)
        ShareFilesCommand = New DelegateCommand(AddressOf OnShareFilesCommand)
        AccessFilesCommand = New DelegateCommand(AddressOf OnAccessFilesCommand)

        ' Load data
        Load()
    End Sub

    ''' <summary>
    ''' Load data
    ''' </summary>
    Private Async Sub Load()
        Try
            Loading.Show()

            ' Get file data
            Await Task.Run(Sub() _fileDataService.GetAllCount()).ConfigureAwait(True)
            PublicText = _fileDataService.PublicFilesCount.ToString()
            SharedText = _fileDataService.SharedFilesCount.ToString()
            AccessedText = _fileDataService.AccessedFilesCount.ToString()

            DataGridActivities = New ObservableCollection(Of ActivityServiceModel)(
                Await Task.Run(Function() _activityService.GetUserActivity().Take(5).ToList).ConfigureAwait(True)
            )

        Catch ex As Exception
            PopUp.Information("Error", ex.Message)
        Finally
            Loading.Hide()
        End Try
    End Sub

    ''' <summary>
    ''' Share files command
    ''' </summary>
    Private Sub OnShareFilesCommand()
        _navigationService.Go("PageRegion", "ShareFilesView", "Shared Files")
    End Sub

    ''' <summary>
    ''' Access files command
    ''' </summary>
    Private Sub OnAccessFilesCommand()
        _navigationService.Go("PageRegion", "AccessFilesView", "Accessed Files")
    End Sub

    ''' <summary>
    ''' On activity selected
    ''' </summary>
    ''' <param name="selectedActivity"></param>
    Private Sub OnActivitySelected(selectedActivity As ActivityServiceModel)
        Try
            Dim parameters = New NavigationParameters()

            If selectedActivity.Action = "No recent" Then
                PopUp.Information("Failed", "No activity was selected")
                Return
            End If

            If selectedActivity.Action = "Remove a file" Then
                PopUp.Information("Information", "This file has been removed.")
                Return
            End If

            If selectedActivity.Action = "Accessed a file" Then
                parameters.Add("fileId", selectedActivity.FileId)
                _navigationService.Go("PageRegion", "FileDetailsView", "Accessed Files", parameters)
            End If

            If selectedActivity.Action = "Shared a file" Then
                parameters.Add("fileId", selectedActivity.FileId)
                _navigationService.Go("PageRegion", "FileDetailsView", "Shared Files", parameters)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Theres an error fetching the selected file")
            Debug.WriteLine($"[DEBUG] Message: {ex.Message}")
        End Try
    End Sub

End Class
