Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows.Input
Imports Prism.Mvvm
Imports Prism.Commands
Imports Prism.Navigation.Regions
Imports Prism.Navigation

#Disable Warning
Public Class SharedFilesViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _userService As IUserService

    Private _sharedFiles As List(Of FilesShared)

    Private _searchInput As String
    Private _resultCount As String
    Private _fromDate As DateTime?
    Private _fromMinDate As DateTime?
    Private _fromMaxDate As DateTime?
    Private _toDate As DateTime?
    Private _toMinDate As DateTime?
    Private _toMaxDate As DateTime?
    Private _dataGridFiles As ObservableCollection(Of FilesShared)
    Private _isBothSelected As Boolean? = True
    Private _isPublicSelected As Boolean
    Private _isPrivateSelected As Boolean
    Private _isAllTypesSelected As Boolean? = True
    Private _isCompressedSelected As Boolean? = True
    Private _isDocsSelected As Boolean? = True
    Private _isImageSelected As Boolean? = True
    Private _isMediaSelected As Boolean? = True
    Private _isExeSelected As Boolean? = True
    Private _isTextSelected As Boolean? = True

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

    Public Property DataGridFiles As ObservableCollection(Of FilesShared)
        Get
            Return _dataGridFiles
        End Get
        Set(value As ObservableCollection(Of FilesShared))
            SetProperty(_dataGridFiles, value)
        End Set
    End Property

    Public Property IsBothSelected As Boolean?
        Get
            Return _isBothSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isBothSelected, value) AndAlso value Then
                IsPublicSelected = False
                IsPrivateSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsPublicSelected As Boolean
        Get
            Return _isPublicSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isPublicSelected, value) AndAlso value Then
                _isBothSelected = False
                IsPrivateSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsPrivateSelected As Boolean
        Get
            Return _isPrivateSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isPrivateSelected, value) AndAlso value Then
                _isBothSelected = False
                IsPublicSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsAllTypesSelected As Boolean?
        Get
            Return _isAllTypesSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isAllTypesSelected, value) Then
                If value.HasValue Then
                    IsDocsSelected = value.Value
                    IsMediaSelected = value.Value
                    IsCompressedSelected = value.Value
                    IsExeSelected = value.Value
                    IsTextSelected = value.Value
                    IsImageSelected = value.Value
                End If
            End If
        End Set
    End Property

    Public Property IsCompressedSelected As Boolean?
        Get
            Return _isCompressedSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isCompressedSelected, value) Then
                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsDocsSelected As Boolean?
        Get
            Return _isDocsSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isDocsSelected, value) Then

                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsImageSelected As Boolean?
        Get
            Return _isImageSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isImageSelected, value) Then
                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsMediaSelected As Boolean?
        Get
            Return _isMediaSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isMediaSelected, value) Then

                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsExeSelected As Boolean?
        Get
            Return _isExeSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isExeSelected, value) Then

                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsTextSelected As Boolean?
        Get
            Return _isTextSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isTextSelected, value) Then

                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property SearchCommand As DelegateCommand
    Public Property ShareFileCommand As DelegateCommand
    Public Property ViewCommand As DelegateCommand(Of Integer?)

    Public Sub New(fileDataService As IFileDataService,
                   navigationService As INavigationService,
                   sessionManager As ISessionManager,
                   userService As IUserService)
        _fileDataService = fileDataService
        _navigationService = navigationService
        _sessionManager = sessionManager
        _userService = userService

        DataGridFiles = New ObservableCollection(Of FilesShared)
        SearchCommand = New DelegateCommand(AddressOf OnSearchCommand)
        ShareFileCommand = New DelegateCommand(AddressOf OnShareFileCommand)
        ViewCommand = New DelegateCommand(Of Integer?)(AddressOf OnViewCommand)

        LoadData()
    End Sub

    Private Async Sub OnViewCommand(fileId As Integer?)
        Try
            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not _userService.CheckStatus Then
                _sessionManager.Logout()
                Await PopUp.Information("Warning", "Your account has been banned.").ConfigureAwait(True)
                RestartApplication()
                Return
            End If

            If Not fileId.HasValue Then
                Debug.WriteLine("[WARN] Tried to view a file with NULL ID")
                Return
            End If

            Debug.WriteLine($"[DEBUG] Viewing file ID: {fileId.Value}")

            Dim parameters = New NavigationParameters From {
                {"fileId", fileId.Value},
                {"openedFrom", "HomeView"}
            }

            _navigationService.Go("PageRegion", "FileDetailsView", "Shared Files", parameters)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub OnSearchCommand()
        ApplyFilters()
    End Sub

    Private Sub OnShareFileCommand()
        _navigationService.Go("PageRegion", "ShareFilesView", "Shared Files")
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

            _navigationService.Start("PageRegion", "SharedFilesView", "Shared Files")

            _sharedFiles = Await Task.Run(Function() _fileDataService.GetSharedFiles(_sessionManager.CurrentUser)).ConfigureAwait(True)

            If _sharedFiles IsNot Nothing AndAlso _sharedFiles.Count > 0 Then
                Dim orderedDates = _sharedFiles.Select(Function(f) f.CreatedAt).OrderBy(Function(d) d).ToList()

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

            IsAllTypesSelected = True
            IsBothSelected = True
            IsDocsSelected = True
            IsMediaSelected = True
            IsImageSelected = True
            IsCompressedSelected = True
            IsExeSelected = True
            IsTextSelected = True

            ApplyFilters()
        Catch ex As Exception
            Debug.WriteLine($"[ERROR] LoadData: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Private Sub UpdateAllCheckboxState()
        Dim allTypesChecked = IsCompressedSelected AndAlso IsDocsSelected AndAlso
                            IsMediaSelected AndAlso IsExeSelected AndAlso
                            IsTextSelected AndAlso IsImageSelected

        If allTypesChecked Then
            IsAllTypesSelected = True
        ElseIf Not IsCompressedSelected AndAlso Not IsDocsSelected AndAlso
               Not IsMediaSelected AndAlso Not IsExeSelected AndAlso
               Not IsTextSelected AndAlso Not IsImageSelected Then
            IsAllTypesSelected = False
        Else
            IsAllTypesSelected = Nothing
        End If
    End Sub

    Private Sub ApplyFilters()
        If _sharedFiles Is Nothing Then
            Debug.WriteLine("[FILTER] No files loaded")
            Return
        End If

        Dim filtered = _sharedFiles.AsEnumerable()
        Debug.WriteLine($"[FILTER] Starting with {filtered.Count()} files")

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

        Dim privacyFilter As New List(Of String)
        If IsBothSelected Then
            privacyFilter.AddRange({"Public", "Private"})
        ElseIf IsPublicSelected Then
            privacyFilter.Add("Public")
        ElseIf IsPrivateSelected Then
            privacyFilter.Add("Private")
        End If
        filtered = filtered.Where(Function(f) privacyFilter.Contains(f.Privacy))

        Dim typeConditions As New List(Of String)
        If IsDocsSelected Then typeConditions.Add("DOCS")
        If IsMediaSelected Then typeConditions.Add("MEDIA")
        If IsImageSelected Then typeConditions.Add("IMAGE")
        If IsCompressedSelected Then typeConditions.Add("COMPRESSED")
        If IsExeSelected Then typeConditions.Add("EXE")
        If IsTextSelected Then typeConditions.Add("TEXT")

        If IsAllTypesSelected Then
            ' This is a placeholder
        Else
            If typeConditions.Count > 0 Then
                filtered = filtered.Where(Function(f)
                                              Dim category = FileTypeModel.GetCategoryByExtension(f.FileType)
                                              Return typeConditions.Contains(category)
                                          End Function)
            Else
                ResultCount = 0
                DataGridFiles = New ObservableCollection(Of FilesShared)()
                RaisePropertyChanged(NameOf(DataGridFiles))
                RaisePropertyChanged(NameOf(ResultCount))
                Return
            End If
        End If

        ResultCount = filtered.Count()
        DataGridFiles = New ObservableCollection(Of FilesShared)(filtered.ToList())
        RaisePropertyChanged(NameOf(DataGridFiles))
        RaisePropertyChanged(NameOf(ResultCount))
    End Sub

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property
End Class