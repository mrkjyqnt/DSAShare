Imports System.Windows.Threading
Imports Prism.Commands
Imports Prism.Events
Imports Prism.Mvvm
Imports Prism.Navigation.Regions
Imports System.Collections.ObjectModel
Imports DryIoc.FastExpressionCompiler.LightExpression
Imports System.IO
Imports Prism.Navigation
Imports System.Windows.Interop

#Disable Warning
Public Class HomeViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _fallBackService As IFallbackService
    Private ReadOnly _userService As IUserService

    Private _isInitialized As Boolean = False
    Private _sharedText As String
    Private _accessedText As String
    Private _dataGridActivities As ObservableCollection(Of Activities)
    Private _isProcessingSelection As Boolean = False
    Private _selectedActivity As Activities

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Property SharedText As String
        Get
            Return _sharedText
        End Get
        Set(value As String)
            SetProperty(_sharedText, value)
        End Set
    End Property

    Public Property AccessedText As String
        Get
            Return _accessedText
        End Get
        Set(value As String)
            SetProperty(_accessedText, value)
        End Set
    End Property

    Public Property DataGridActivities As ObservableCollection(Of Activities)
        Get
            Return _dataGridActivities
        End Get
        Set(value As ObservableCollection(Of Activities))
            SetProperty(_dataGridActivities, value)
        End Set
    End Property

    Public Property SelectedActivity As Activities
        Get
            Return _selectedActivity
        End Get
        Set(value As Activities)
            If _isProcessingSelection Then Return

            _isProcessingSelection = True
            Try
                If SetProperty(_selectedActivity, value) AndAlso value IsNot Nothing Then
                    If SelectedActivityCommand.CanExecute(value) Then
                        SelectedActivityCommand.Execute(value)
                    End If
                End If
            Finally
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
                   navigationService As INavigationService,
                   fallBackService As IFallbackService,
                   userService As IUserService)

        _fileDataService = fileDataService
        _activityService = activityService
        _sessionManager = sessionManager
        _navigationService = navigationService
        _fallBackService = fallBackService
        _userService = userService

        ' Initialize commands
        DataGridActivities = New ObservableCollection(Of Activities)()
        SelectedActivityCommand = New DelegateCommand(Of Activities)(AddressOf OnActivitySelected)
        ShareFilesCommand = New DelegateCommand(AddressOf OnShareFilesCommand)
        AccessFilesCommand = New DelegateCommand(AddressOf OnAccessFilesCommand)

        ' Load data
        Load()
    End Sub

    ''' <summary>
    ''' Load data
    ''' </summary>
    Private Async Sub Load()
        _navigationService.Start("PageRegion", "HomeView", "Home")

        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(100)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            Dim ShareCount = Await Task.Run(Function() _fileDataService.GetSharedFiles(_sessionManager.CurrentUser)).ConfigureAwait(True)
            Dim AccessCount = Await Task.Run(Function() _fileDataService.GetAccessedFiles(_sessionManager.CurrentUser)).ConfigureAwait(True)

            SharedText = ShareCount?.Where(Function(f) f.Privacy <> "Deleted" AndAlso f.Privacy <> "Blocked").ToList.Count
            AccessedText = AccessCount?.Count

            Dim activities = Await Task.Run(Function() _activityService?.GetUserActivity().Take(5).ToList).ConfigureAwait(True)

            If activities IsNot Nothing Then
                DataGridActivities = New ObservableCollection(Of Activities)(activities)
            End If

        Catch ex As Exception
            Debug.WriteLine($"[HomeViewModel] Error loading data: {ex.StackTrace}")
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
    Private Async Function OnAccessFilesCommand() As Task
        Try
            Dim file As FilesShared

            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(50)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            Dim popUpResult As PopupResult = Await PopUp.Selection()

            If popUpResult Is Nothing Then
                Await PopUp.Information("Cancelled", "File deletion was cancelled.").ConfigureAwait(True)
                Return
            End If

            Dim input = popUpResult.GetValue(Of String)("Input")
            Dim selectedOption = popUpResult.GetValue(Of String)("SelectedOption")

            Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(_sessionManager.CurrentUser)).ConfigureAwait(True)
            If Not hasPermission Then
                Await PopUp.Information("Failed", "You do not have permission to access this file.").ConfigureAwait(True)
                Return
            End If

            Dim fileShare = New FilesShared With {
                .ShareType = selectedOption,
                .ShareValue = input,
                .UploadedBy = _sessionManager.CurrentUser.Id
            }

            file = Await Task.Run(Function() _fileDataService.GetSharedFileByPrivate(fileShare)).ConfigureAwait(True)

            If file Is Nothing Then
                Await PopUp.Information("Failed", "File not found.").ConfigureAwait(True)
                Return
            End If

            Dim parameters = New NavigationParameters From {
                {"fileId", file.Id},
                {"openedFrom", "AccessedFilesView"}
            }

            _navigationService.Go("PageRegion", "FileDetailsView", "Accessed Files", parameters)

        Catch ex As Exception
            Debug.WriteLine($"[AccessedFilesViewModel] OnAccessFileCommand Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

    ''' <summary>
    ''' On activity selected
    ''' </summary>
    ''' <param name="selectedActivity"></param>
    Private Async Sub OnActivitySelected(selectedActivity As Activities)
        Dim parameters = New NavigationParameters()

        Try
            If selectedActivity Is Nothing OrElse selectedActivity.Action Is Nothing Then
                PopUp.Information("Failed", "No activity was selected or activity data is incomplete")
                Return
            End If

            If selectedActivity.Action = "No recent" Then
                Return
            End If

            If selectedActivity.Action = "Deleted a file" Then
                PopUp.Information("Information", "This file has been deleted")
                Return
            End If

            If selectedActivity.Action = "Deleted a user" Then
                PopUp.Information("Information", "This user has been deleted")
                Return
            End If

            If selectedActivity.Action = "Removed Access a file" Then
                Debug.WriteLine($"[DEBUG] Removed access file: {selectedActivity.FileId}")
                PopUp.Information("Failed", "Referenced file access was removed ")
                Return
            End If

            Dim userAccountInfo = Await Task.Run(Function() _userService.GetUserById(New Users With {.Id = selectedActivity.AccountId})).ConfigureAwait(True)

            If selectedActivity.Action = "Updated a user" OrElse
                selectedActivity.Action = "Unbanned a user" OrElse
                selectedActivity.Action = "Banned a user" OrElse
                selectedActivity.Action = "Change role a user" OrElse
                selectedActivity.Action = "Change information a user" OrElse
                selectedActivity.Action = "Change password a user" Then

                If userAccountInfo Is Nothing Then
                    PopUp.Information("Failed", "Referenced user was removed")
                    Return
                End If

                parameters = New NavigationParameters()
                parameters.Add("userId", selectedActivity.AccountId)

                If _sessionManager.CurrentUser.Role = "Admin" And Not userAccountInfo?.Id = _sessionManager.CurrentUser.Id Then
                    parameters.Add("openedFrom", "ManageUsersView")
                    _navigationService.Go("PageRegion", "UserInformationsView", "Manage Users", parameters)
                Else
                    PopUp.Information("Failed", "You dont have permission to access")
                End If

                Return
            End If

            If selectedActivity.Action = "Change information" OrElse
                selectedActivity.Action = "Change role" OrElse
                selectedActivity.Action = "Change password" Then

                If userAccountInfo Is Nothing Then
                    PopUp.Information("Failed", "Referenced user was removed")
                    Return
                End If

                parameters = New NavigationParameters()
                parameters.Add("userId", selectedActivity.AccountId)
                parameters.Add("openedFrom", "AccountView")
                _navigationService.Go("PageRegion", "UserInformationsView", "Account", parameters)

                Return
            End If

            Dim fileShared = New FilesShared With {
                .Id = selectedActivity.FileId
            }

            Dim fileAccessed = New FilesAccessed With {
                .UserId = _sessionManager.CurrentUser.Id,
                .FileId = selectedActivity.FileId
            }

            Dim fileSharedInfo = Await Task.Run(Function() _fileDataService.GetSharedFileById(fileShared)).ConfigureAwait(True)
            Dim fileAccessedInfo = Await Task.Run(Function() _fileDataService.GetAccessedFileByUserFile(fileAccessed)).ConfigureAwait(True)

            If Not _sessionManager.CurrentUser.Id = fileSharedInfo?.UploadedBy And
                Not _sessionManager.CurrentUser.Role = "Admin" Then

                If fileSharedInfo.Availability = "Disabled" Then
                    PopUp.Information("Failed", "Referenced file has been disabled by the uploader")
                    Return
                End If

            End If

            If selectedActivity.Action = "Shared a file" OrElse
                selectedActivity.Action = "Enabled a file" OrElse
                selectedActivity.Action = "Disabled a file" OrElse
                selectedActivity.Action = "Blocked a file" OrElse
                selectedActivity.Action = "Updated a file" Then
                parameters = New NavigationParameters()
                parameters.Add("fileId", selectedActivity.FileId)

                If fileSharedInfo Is Nothing Then
                    PopUp.Information("Failed", "Referenced file was been removed")
                    Return
                End If

                If fileSharedInfo.Privacy = "Deleted" Then
                    PopUp.Information("Failed", "Referenced file was been removed")
                    Return
                End If

                If fileSharedInfo.Privacy = "Blocked" Then
                    PopUp.Information("Failed", "Referenced file was been blocked by an admin")
                    Return
                End If

                If _sessionManager.CurrentUser.Role = "Admin" And Not fileSharedInfo.UploadedBy = _sessionManager.CurrentUser.Id Then
                    parameters.Add("openedFrom", "ManageFilesView")
                    _navigationService.Go("PageRegion", "FileDetailsView", "Manage Files", parameters)
                Else
                    parameters.Add("openedFrom", "HomeView")
                    _navigationService.Go("PageRegion", "FileDetailsView", "Shared Files", parameters)
                End If
            End If

            If selectedActivity.Action = "Save Access a file" OrElse
                selectedActivity.Action = "Remove Access a file" Then
                parameters = New NavigationParameters()
                parameters.Add("fileId", selectedActivity.FileId)

                If fileAccessedInfo Is Nothing Then
                    PopUp.Information("Failed", "Referenced file access was removed")
                    Return
                End If

                parameters.Add("openedFrom", "HomeView")
                _navigationService.Go("PageRegion", "FileDetailsView", "Shared Files", parameters)

                Return
            End If

            If selectedActivity.Action = "Resolved a report" Then
                parameters = New NavigationParameters()
                parameters.Add("reportId", selectedActivity.FileId)

                If fileAccessedInfo Is Nothing Then
                    PopUp.Information("Failed", "Referenced report was removed")
                    Return
                End If

                parameters.Add("openedFrom", "HomeView")
                _navigationService.Go("PageRegion", "ReportDetailsView", "Reports", parameters)

                Return
            End If
        Catch ex As Exception
            Debug.WriteLine($"[ERROR] Activity selection failed: {ex.Message}")
            PopUp.Information("Error", "An unexpected error occurred while processing your request.")
        End Try
    End Sub
End Class
