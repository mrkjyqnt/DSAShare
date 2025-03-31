Imports System.Windows.Threading
Imports Prism.Commands
Imports Prism.Events
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports System.Collections.ObjectModel
Imports DryIoc.FastExpressionCompiler.LightExpression
Imports System.IO
Imports Prism.Navigation

#Disable Warning
Public Class HomeViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _navigationService As INavigationService

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
    Private _isProcessingSelection As Boolean = False

    Public Property SelectedActivity As ActivityServiceModel
        Get
            Return _selectedActivity
        End Get
        Set(value As ActivityServiceModel)
            If _isProcessingSelection Then Return

            _isProcessingSelection = True
            Try
                If SetProperty(_selectedActivity, value) AndAlso value IsNot Nothing Then
                    If SelectedActivityCommand.CanExecute(value) Then
                        SelectedActivityCommand.Execute(value)
                    End If
                End If
            Finally
                ' Delay the clearing to avoid interrupting navigation
                Application.Current.Dispatcher.BeginInvoke(Sub()
                                                               SetProperty(_selectedActivity, Nothing)
                                                               _isProcessingSelection = False
                                                           End Sub)
            End Try
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
                   sessionManager As ISessionManager,
                   navigationService As INavigationService)

        _fileDataService = fileDataService
        _activityService = activityService
        _sessionManager = sessionManager
        _navigationService = navigationService

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

            _navigationService.Start("PageRegion", "HomeView", "Home")

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error loading data: {ex.Message}")
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
    Private Async Sub OnActivitySelected(selectedActivity As ActivityServiceModel)
        Try
            Dim file = New FilesShared With {
                .Id = selectedActivity.FileId
            }
            Dim fileInfo = Await Task.Run(Function() _fileDataService.GetFileById(file)).ConfigureAwait(True)

            ' Validate the activity exists
            If selectedActivity Is Nothing OrElse selectedActivity.Action Is Nothing Then
                PopUp.Information("Failed", "No activity was selected or activity data is incomplete")
                Return
            End If

            ' Handle special cases
            If selectedActivity.Action = "No recent" Then
                PopUp.Information("Information", "No recent activities found")
                Return
            End If

            If selectedActivity.Action = "Deleted a file" Then
                PopUp.Information("Information", "This file has been deleted")
                Return
            End If

            If fileInfo Is Nothing Then
                PopUp.Information("Failed", "Selected file was either removed or changed")
                Return
            End If

            If Not _sessionManager.CurrentUser.Id = fileInfo.UploadedBy Then

                If fileInfo.Availability = "Disabled" Then
                    PopUp.Information("Failed", "Referenced file has been disabled by the uploader")
                    Return
                End If

                If fileInfo.Availability = "Deleted" Then
                    PopUp.Information("Failed", "Referenced file has been deleted by the uploader")
                    Return
                End If

            End If

            ' Validate FileId for actions that require it
            If (selectedActivity.Action = "Accessed a file" OrElse
            selectedActivity.Action = "Shared a file" OrElse selectedActivity.Action = "Updated a file") AndAlso
            (selectedActivity.FileId Is Nothing OrElse selectedActivity.FileId Is Nothing) Then
                PopUp.Information("Failed", "The selected file is no longer available")
                Return
            End If

            ' Prepare navigation
            Dim parameters = New NavigationParameters()
            parameters.Add("fileId", selectedActivity.FileId)

            ' Navigate with error handling
            _navigationService.Go("PageRegion", "FileDetailsView", "Shared Files", parameters)

        Catch ex As Exception
            Debug.WriteLine($"[ERROR] Activity selection failed: {ex.Message}")
            PopUp.Information("Error", "An unexpected error occurred while processing your request.")
        End Try
    End Sub

End Class
