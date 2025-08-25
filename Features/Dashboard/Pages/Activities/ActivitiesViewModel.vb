Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions
Imports System.Collections.ObjectModel

#Disable Warning
Public Class ActivitiesViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _fallBackService As IFallbackService
    Private ReadOnly _userService As IUserService

    Private _openedFrom As String
    Private _activities As List(Of Activities)

    Private _searchInput As String
    Private _resultCount As String
    Private _fromDate As DateTime?
    Private _fromMinDate As DateTime?
    Private _fromMaxDate As DateTime?
    Private _toDate As DateTime?
    Private _toMinDate As DateTime?
    Private _toMaxDate As DateTime?
    Private _isBothSelected As Boolean? = True
    Private _isAccountSelected As Boolean
    Private _isFilesSelected As Boolean

    Private _dataGridActivities As ObservableCollection(Of Activities)
    Private _isProcessingSelection As Boolean = False
    Private _selectedActivity As Activities

    Public Property ResultCount As String
        Get
            Return _resultCount
        End Get
        Set(value As String)
            SetProperty(_resultCount, value)
        End Set
    End Property

    Public Property SearchInput As String
        Get
            Return _searchInput
        End Get
        Set(value As String)
            SetProperty(_searchInput, value)
            ApplyFilters()
        End Set
    End Property

    Public Property FromSelectedDate As DateTime?
        Get
            Return _fromDate
        End Get
        Set(value As DateTime?)
            SetProperty(_fromDate, value)
            ApplyFilters()
        End Set
    End Property

    Public Property FromMinDate As DateTime?
        Get
            Return _fromMinDate
        End Get
        Set(value As DateTime?)
            SetProperty(_fromMinDate, value)
        End Set
    End Property

    Public Property FromMaxDate As DateTime?
        Get
            Return _fromMaxDate
        End Get
        Set(value As DateTime?)
            SetProperty(_fromMaxDate, value)
        End Set
    End Property

    Public Property ToSelectedDate As DateTime?
        Get
            Return _toDate
        End Get
        Set(value As DateTime?)
            SetProperty(_toDate, value)
            ApplyFilters()
        End Set
    End Property

    Public Property ToMinDate As DateTime?
        Get
            Return _toMinDate
        End Get
        Set(value As DateTime?)
            SetProperty(_toMinDate, value)
        End Set
    End Property

    Public Property ToMaxDate As DateTime?
        Get
            Return _toMaxDate
        End Get
        Set(value As DateTime?)
            SetProperty(_toMaxDate, value)
        End Set
    End Property

    Public Property IsBothSelected As Boolean?
        Get
            Return _isBothSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isBothSelected, value) AndAlso value Then
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsAccountSelected As Boolean
        Get
            Return _isAccountSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isAccountSelected, value) AndAlso value Then
                IsBothSelected = False
                IsFilesSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsFilesSelected As Boolean
        Get
            Return _isFilesSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isFilesSelected, value) AndAlso value Then
                IsBothSelected = False
                IsAccountSelected = False
                ApplyFilters()
            End If
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

        Load()
    End Sub

    Private Async Sub Load()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            _navigationService.Start("PageRegion", "ActivitiesView", "Activities")
            _activities = Await Task.Run(Function() _activityService.GetUserActivity(_sessionManager.CurrentUser)).ConfigureAwait(True)

            If _activities IsNot Nothing AndAlso _activities.Count > 0 Then
                Dim orderedDates = _activities.Select(Function(f) f.ActionAt).OrderBy(Function(d) d).ToList()

                FromMinDate = orderedDates.First()
                FromMaxDate = orderedDates.Last()
                ToMinDate = orderedDates.First()
                ToMaxDate = orderedDates.Last()

                FromSelectedDate = orderedDates.First()
                ToSelectedDate = orderedDates.Last()
            Else
                FromMinDate = DateTime.Today.AddYears(-1)
                FromMaxDate = DateTime.Today
                ToMinDate = DateTime.Today.AddYears(-1)
                ToMaxDate = DateTime.Today
            End If

            IsBothSelected = True
            IsAccountSelected = False
            IsFilesSelected = False

            ApplyFilters()
        Catch ex As Exception
            Debug.WriteLine($"[UserActivitiesViewModel] Load Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    ''' <summary>
    ''' On activity selected
    ''' </summary>
    ''' <param name="selectedActivity"></param>
    Private Async Sub OnActivitySelected(selectedActivity As Activities)
        Dim parameters = New NavigationParameters()

        Try
            If selectedActivity Is Nothing OrElse selectedActivity.Action Is Nothing Then
                Await PopUp.Information("Failed", "No activity was selected or activity data is incomplete")
                Return
            End If

            If selectedActivity.Action = "No recent" Then
                Return
            End If

            If selectedActivity.Action = "Deleted a file" Then
                Await PopUp.Information("Information", "This file has been deleted")
                Return
            End If

            If selectedActivity.Action = "Deleted a user" Then
                Await PopUp.Information("Information", "This user has been deleted")
                Return
            End If

            If selectedActivity.Action = "Removed Access a file" Then
                Debug.WriteLine($"[DEBUG] Removed access file: {selectedActivity.FileId}")
                Await PopUp.Information("Failed", "Referenced file access was removed ")
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
                    Await PopUp.Information("Failed", "Referenced user was removed")
                    Return
                End If

                parameters = New NavigationParameters()
                parameters.Add("userId", selectedActivity.AccountId)

                If _sessionManager.CurrentUser.Role = "Admin" And Not userAccountInfo?.Id = _sessionManager.CurrentUser.Id Then
                    parameters.Add("openedFrom", "ManageUsersView")
                    _navigationService.Go("PageRegion", "UserInformationsView", "Manage Users", parameters)
                Else
                    Await PopUp.Information("Failed", "You dont have permission to access")
                End If

                Return
            End If

            If selectedActivity.Action = "Change information" OrElse
                selectedActivity.Action = "Change role" OrElse
                selectedActivity.Action = "Change password" Then

                If userAccountInfo Is Nothing Then
                    Await PopUp.Information("Failed", "Referenced user was removed")
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
                    Await PopUp.Information("Failed", "Referenced file has been disabled by the uploader")
                    Return
                End If

            End If

            If selectedActivity.Action = "Shared a file" OrElse
                selectedActivity.Action = "Enabled a file" OrElse
                selectedActivity.Action = "Disabled a file" OrElse
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
                    parameters.Add("openedFrom", "ActivitiesView")
                    _navigationService.Go("PageRegion", "FileDetailsView", "Shared Files", parameters)
                End If
            End If

            If selectedActivity.Action = "Save Access a file" Then
                parameters = New NavigationParameters()
                parameters.Add("fileId", selectedActivity.FileId)

                If fileAccessedInfo Is Nothing Then
                    Await PopUp.Information("Failed", "Referenced file access was removed")
                    Return
                End If

                If fileSharedInfo Is Nothing Then
                    Await PopUp.Information("Failed", "Referenced file access was removed")
                    Return
                End If

                parameters.Add("openedFrom", "ActivitiesViewModel")
                _navigationService.Go("PageRegion", "FileDetailsView", "Shared Files", parameters)

                Return
            End If
        Catch ex As Exception
            Debug.WriteLine($"[UserActivitiesViewModel] Activity selection failed: {ex.Message}")
            PopUp.Information("Error", "An unexpected error occurred while processing your request.")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Private Sub ApplyFilters()
        If _activities Is Nothing Then
            Debug.WriteLine("[FILTER] No activities loaded")
            Return
        End If

        Dim filtered = _activities.AsEnumerable()
        Debug.WriteLine($"[FILTER] Starting with {filtered.Count()} activity")

        If Not String.IsNullOrEmpty(SearchInput) Then
            filtered = filtered.Where(Function(f) f.Name.Contains(SearchInput, StringComparison.OrdinalIgnoreCase))
            Debug.WriteLine($"[FILTER] After search: {filtered.Count()}")
        End If

        If FromSelectedDate.HasValue Then
            Dim fromDate = FromSelectedDate.Value.Date
            filtered = filtered.Where(Function(f) f.ActionAt >= fromDate)
        End If

        If ToSelectedDate.HasValue Then
            Dim toDate = ToSelectedDate.Value.Date.AddDays(1)
            filtered = filtered.Where(Function(f) f.ActionAt < toDate)
        End If

        If IsBothSelected Then
            filtered = filtered.Where(Function(f) f.FileId IsNot Nothing OrElse f.AccountId IsNot Nothing)
        ElseIf IsAccountSelected Then
            filtered = filtered.Where(Function(f) f.AccountId IsNot Nothing)
        ElseIf IsFilesSelected Then
            filtered = filtered.Where(Function(f) f.FileId IsNot Nothing)
        End If

        ResultCount = filtered.Count
        DataGridActivities = New ObservableCollection(Of Activities)(filtered.ToList())
        RaisePropertyChanged(NameOf(DataGridActivities))
        RaisePropertyChanged(NameOf(ResultCount))
    End Sub

    Public ReadOnly Property SelectedActivityCommand As ICommand

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property
End Class
