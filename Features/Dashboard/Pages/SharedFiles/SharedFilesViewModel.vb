Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows.Input
Imports Prism.Mvvm
Imports Prism.Commands
Imports Prism.Navigation.Regions
Imports Prism.Navigation

Public Class SharedFilesViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _sessionManager As ISessionManager

    Private _userFiles As List(Of FilesShared)

    ' Properties
    Private _searchInput As String
    Public Property SearchInput As String
        Get
            Return _searchInput
        End Get
        Set(value As String)
            SetProperty(_searchInput, value)
            ApplyFilters()
        End Set
    End Property

    Private _fromDate As DateTime? = Nothing
    Public Property FromSelectedDate As DateTime?
        Get
            Return _fromDate
        End Get
        Set(value As DateTime?)
            SetProperty(_fromDate, value)
            ApplyFilters()
        End Set
    End Property

    Private _toDate As DateTime? = Nothing
    Public Property ToSelectedDate As DateTime?
        Get
            Return _toDate
        End Get
        Set(value As DateTime?)
            SetProperty(_toDate, value)
            ApplyFilters()
        End Set
    End Property

    Private _dataGridFiles As ObservableCollection(Of FilesShared)
    Public Property DataGridFiles As ObservableCollection(Of FilesShared)
        Get
            Return _dataGridFiles
        End Get
        Set(value As ObservableCollection(Of FilesShared))
            SetProperty(_dataGridFiles, value)
        End Set
    End Property

    Private _isBothSelected As Boolean = True
    Public Property IsBothSelected As Boolean
        Get
            Return _isBothSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isBothSelected, value) AndAlso value Then
                IsPublicSelected = False
                IsPrivateSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Private _isPublicSelected As Boolean
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

    Private _isPrivateSelected As Boolean
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


    ' Checkbox Properties
    Private _isAllTypesSelected As Boolean? = True
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

    Private _isCompressedSelected As Boolean
    Public Property IsCompressedSelected As Boolean
        Get
            Return _isCompressedSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isCompressedSelected, value) Then
                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Private _isDocsSelected As Boolean
    Public Property IsDocsSelected As Boolean
        Get
            Return _isDocsSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isDocsSelected, value) Then

                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Private _isImageSelected As Boolean
    Public Property IsImageSelected As Boolean
        Get
            Return _isImageSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isImageSelected, value) Then
                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Private _isMediaSelected As Boolean
    Public Property IsMediaSelected As Boolean
        Get
            Return _isMediaSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isMediaSelected, value) Then

                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Private _isExeSelected As Boolean
    Public Property IsExeSelected As Boolean
        Get
            Return _isExeSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isExeSelected, value) Then

                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    Private _isTextSelected As Boolean
    Public Property IsTextSelected As Boolean
        Get
            Return _isTextSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isTextSelected, value) Then

                UpdateAllCheckboxState()
                ApplyFilters()
            End If
        End Set
    End Property

    ' Commands
    Public Property SearchCommand As DelegateCommand
    Public Property ShareFileCommand As DelegateCommand
    Public Property ViewCommand As DelegateCommand(Of Integer?)

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Sub New(fileDataService As IFileDataService, navigationService As INavigationService, sessionManager As ISessionManager)
        _fileDataService = fileDataService
        _navigationService = navigationService
        _sessionManager = sessionManager

        DataGridFiles = New ObservableCollection(Of FilesShared)
        SearchCommand = New DelegateCommand(AddressOf OnSearchCommand)
        ShareFileCommand = New DelegateCommand(AddressOf OnShareFileCommand)
        ViewCommand = New DelegateCommand(Of Integer?)(AddressOf OnViewCommand)

        IsAllTypesSelected = True
        IsBothSelected = True
        IsDocsSelected = True
        IsMediaSelected = True
        IsImageSelected = True
        IsCompressedSelected = True
        IsExeSelected = True
        IsTextSelected = True

        _navigationService.Start("PageRegion", "SharedFilesView", "Shared Files")

        LoadData()
    End Sub

    Private Sub OnViewCommand(fileId As Integer?)
        If Not fileId.HasValue Then
            Debug.WriteLine("[WARN] Tried to view a file with NULL ID")
            Return
        End If

        Debug.WriteLine($"[DEBUG] Viewing file ID: {fileId.Value}")

        Dim parameters = New NavigationParameters From {
            {"fileId", fileId.Value}
        }

        _navigationService.Go("PageRegion", "FileDetailsView", "Shared Files", parameters)
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

    Private Sub OnSearchCommand()
        ApplyFilters()
    End Sub

    Private Sub OnShareFileCommand()
        _navigationService.Go("PageRegion", "ShareFilesView", "Shared Files")
    End Sub

    Private Async Sub LoadData()
        Try
            Loading.Show()

            _userFiles = Await Task.Run(Function() _fileDataService.GetSharedFiles(_sessionManager.CurrentUser)).ConfigureAwait(True)

            ' Add debug output
            Debug.WriteLine($"[DEBUG] Loaded {_userFiles?.Count} files")
            If _userFiles IsNot Nothing Then
                For Each file In _userFiles.Take(3)
                    Debug.WriteLine($"[SAMPLE] File: {file.Name} | Type: {file.FileType} | Privacy: {file.Privacy} | Date: {file.CreatedAt}")
                Next
            End If

            ApplyFilters()
        Catch ex As Exception
            Debug.WriteLine($"[ERROR] LoadData: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Private Sub ApplyFilters()
        If _userFiles Is Nothing Then
            Debug.WriteLine("[FILTER] No files loaded")
            Return
        End If

        Dim filtered = _userFiles.AsEnumerable()
        Debug.WriteLine($"[FILTER] Starting with {filtered.Count()} files")

        ' Search filter
        If Not String.IsNullOrEmpty(SearchInput) Then
            filtered = filtered.Where(Function(f) f.Name.Contains(SearchInput, StringComparison.OrdinalIgnoreCase))
            Debug.WriteLine($"[FILTER] After search: {filtered.Count()}")
        End If

        ' Date filter
        If FromSelectedDate.HasValue Then
            Dim fromDate = FromSelectedDate.Value.Date
            filtered = filtered.Where(Function(f) f.CreatedAt >= fromDate)
        End If

        If ToSelectedDate.HasValue Then
            Dim toDate = ToSelectedDate.Value.Date.AddDays(1)
            filtered = filtered.Where(Function(f) f.CreatedAt < toDate)
        End If

        If FromSelectedDate.HasValue AndAlso ToSelectedDate.HasValue Then
            If FromSelectedDate > ToSelectedDate Then
                Debug.WriteLine("[WARN] Invalid date range: FromDate > ToDate")
                Return
            End If
        End If

        ' Privacy filter (radio buttons)
        Dim privacyFilter As New List(Of String)
        If IsBothSelected Then
            privacyFilter.AddRange({"Public", "Private"})
        ElseIf IsPublicSelected Then
            privacyFilter.Add("Public")
        ElseIf IsPrivateSelected Then
            privacyFilter.Add("Private")
        End If
        filtered = filtered.Where(Function(f) privacyFilter.Contains(f.Privacy))

        ' Type filter (checkboxes)
        Dim typeConditions As New List(Of String)
        If IsDocsSelected Then typeConditions.Add("DOCS")
        If IsMediaSelected Then typeConditions.Add("MEDIA")
        If IsImageSelected Then typeConditions.Add("IMAGE")
        If IsCompressedSelected Then typeConditions.Add("COMPRESSED")
        If IsExeSelected Then typeConditions.Add("EXE")
        If IsTextSelected Then typeConditions.Add("TEXT")

        ' Only apply type filter if at least one checkbox is selected
        If typeConditions.Count > 0 Then
            filtered = filtered.Where(Function(f)
                                          Dim category = FileTypeModel.GetCategoryByExtension(f.FileType)
                                          Debug.WriteLine($"[TYPE] File: {f.Name} ({f.FileType}) → {category}")
                                          Return typeConditions.Contains(category)
                                      End Function)
        End If

        DataGridFiles = New ObservableCollection(Of FilesShared)(filtered.ToList())
        RaisePropertyChanged(NameOf(DataGridFiles))
        Debug.WriteLine($"[FILTER] Selected types: {String.Join(", ", typeConditions)}")
        Debug.WriteLine($"[DATES] From: {FromSelectedDate}, To: {ToSelectedDate}")
    End Sub
End Class