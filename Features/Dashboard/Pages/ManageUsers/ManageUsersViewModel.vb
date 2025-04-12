Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions
Imports System.Collections.ObjectModel

#Disable Warning CRR0029
Public Class ManageUsersViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _userService As IUserService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _sessionManager As ISessionManager

    Private _users As List(Of Users)

    Private _searchInput As String
    Private _resultCount As String
    Private _fromDate As DateTime?
    Private _fromMinDate As DateTime?
    Private _fromMaxDate As DateTime?
    Private _toDate As DateTime?
    Private _toMinDate As DateTime?
    Private _toMaxDate As DateTime?
    Private _dataGridUsers As ObservableCollection(Of Users)
    Private _isMemberSelected As Boolean? = True
    Private _isBothSelected As Boolean
    Private _isActiveSelected As Boolean
    Private _isBannedSelected As Boolean

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

    Public Property DataGridUsers As ObservableCollection(Of Users)
        Get
            Return _dataGridUsers
        End Get
        Set(value As ObservableCollection(Of Users))
            SetProperty(_dataGridUsers, value)
        End Set
    End Property

    Public Property IsMemberSelected As Boolean?
        Get
            Return _isMemberSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isMemberSelected, value) AndAlso value Then
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsBothSelected As Boolean
        Get
            Return _isBothSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isBothSelected, value) AndAlso value Then
                IsActiveSelected = False
                IsBannedSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsActiveSelected As Boolean
        Get
            Return _isActiveSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isActiveSelected, value) AndAlso value Then
                IsBothSelected = False
                IsBannedSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsBannedSelected As Boolean
        Get
            Return _isBannedSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isBannedSelected, value) AndAlso value Then
                IsBothSelected = False
                IsActiveSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property SearchCommand As DelegateCommand
    Public Property ViewCommand As DelegateCommand(Of Integer?)

    Public Sub New(userService As IUserService, navigationService As INavigationService, sessionManager As ISessionManager)
        _userService = userService
        _navigationService = navigationService
        _sessionManager = sessionManager

        DataGridUsers = New ObservableCollection(Of Users)
        SearchCommand = New DelegateCommand(AddressOf OnSearchCommand)
        ViewCommand = New DelegateCommand(Of Integer?)(AddressOf OnViewCommand)

        LoadData()
    End Sub

    Private Sub OnViewCommand(Id As Integer?)
        If Not Id.HasValue Then
            Debug.WriteLine("[WARN] Tried to view a user with NULL ID")
            Return
        End If

        Debug.WriteLine($"[DEBUG] Viewing user ID: {Id.Value}")

        Dim parameters = New NavigationParameters From {
            {"userId", Id.Value},
            {"openedFrom", "ManageUsersView"}
        }

        _navigationService.Go("PageRegion", "UserInformationsView", "Manage Users", parameters)
    End Sub

    Private Sub OnSearchCommand()
        ApplyFilters()
    End Sub

    Private Async Sub LoadData()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(50)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            _navigationService.Start("PageRegion", "ManageUsersView", "Manage Users")

            IsBothSelected = True
            _users = Await Task.Run(Function() _userService.GetAllUsers()).ConfigureAwait(True)

            If _users IsNot Nothing AndAlso _users.Count > 0 Then
                Dim orderedDates = _users.Select(Function(f) f.CreatedAt).OrderBy(Function(d) d).ToList()

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

            ApplyFilters()
        Catch ex As Exception
            Debug.WriteLine($"[ERROR] LoadData: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Private Sub ApplyFilters()
        If _users Is Nothing Then
            Debug.WriteLine("[FILTER] No users loaded")
            Return
        End If

        Dim filtered = _users.AsEnumerable()
        Debug.WriteLine($"[FILTER] Starting with {filtered.Count()} user")

        If Not String.IsNullOrEmpty(SearchInput) Then
            filtered = filtered.Where(Function(f) f.Name.Contains(SearchInput, StringComparison.OrdinalIgnoreCase))
            Debug.WriteLine($"[FILTER] After search: {filtered.Count()}")
        End If

        If FromSelectedDate.HasValue Then
            Dim fromDate = FromSelectedDate.Value.Date
            filtered = filtered.Where(Function(f) f.CreatedAt >= fromDate)
        End If

        If ToSelectedDate.HasValue Then
            Dim toDate = ToSelectedDate.Value.Date.AddDays(1)
            filtered = filtered.Where(Function(f) f.CreatedAt < toDate)
        End If

        Dim roleFilter As New List(Of String)
        If IsMemberSelected Then
            roleFilter.Add("Member")
        End If
        filtered = filtered.Where(Function(f) roleFilter.Contains(f.Role))

        Dim statusFilter As New List(Of String)
        If IsBothSelected Then
            statusFilter.AddRange({"Active", "Banned"})
        ElseIf IsActiveSelected Then
            statusFilter.Add("Active")
        ElseIf IsBannedSelected Then
            statusFilter.Add("Banned")
        End If
        filtered = filtered.Where(Function(f) statusFilter.Contains(f.Status))

        ResultCount = filtered.Count
        DataGridUsers = New ObservableCollection(Of Users)(filtered.ToList())
        RaisePropertyChanged(NameOf(DataGridUsers))
        RaisePropertyChanged(NameOf(ResultCount))
    End Sub

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property
End Class
