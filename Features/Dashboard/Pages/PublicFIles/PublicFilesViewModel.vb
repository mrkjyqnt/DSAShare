Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions
Imports System.Collections.ObjectModel

#Disable Warning
Public Class PublicFilesViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _activityService As IActivityService
    Private ReadOnly _sessionManager As ISessionManager

    Private _publicFiles As List(Of FilesShared)
    Private _resultCount As String
    Private _searchInput As String
    Private _fromDate As DateTime?
    Private _fromMinDate As DateTime?
    Private _fromMaxDate As DateTime?
    Private _toDate As DateTime?
    Private _toMinDate As DateTime?
    Private _toMaxDate As DateTime?
    Private _dataGridFiles As ObservableCollection(Of FilesShared)
    Private _isPublicSelected As Boolean? = True
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


    Public Property IsPublicSelected As Boolean?
        Get
            Return _isPublicSelected
        End Get
        Set(value As Boolean?)
            If SetProperty(_isPublicSelected, value) AndAlso value Then
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
    Public Property ViewCommand As AsyncDelegateCommand(Of Integer?)

    Public Sub New(fileDataService As IFileDataService,
                   navigationService As INavigationService,
                   activityService As IActivityService,
                   sessionManager As ISessionManager)
        _fileDataService = fileDataService
        _navigationService = navigationService
        _activityService = activityService
        _sessionManager = sessionManager

        DataGridFiles = New ObservableCollection(Of FilesShared)
        SearchCommand = New DelegateCommand(AddressOf OnSearchCommand)
        ViewCommand = New AsyncDelegateCommand(Of Integer?)(AddressOf OnViewCommand)

        LoadData()
    End Sub

    Private Async Function OnViewCommand(fileId As Integer?) As Task
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(50)

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            If Not fileId.HasValue Then
                Debug.WriteLine("[PublicFilesViewModel] Tried to view a file with NULL ID")
                Return
            End If

            Dim fileShared = New FilesShared With {
                .Id = fileId
            }

            fileShared = Await Task.Run(Function() _fileDataService.GetSharedFileById(fileShared)).ConfigureAwait(True)

            If fileShared Is Nothing Then
                Debug.WriteLine("[PublicFiles] Retrieve nothing")
                Return
            End If

            ' Check if the user is not guest
            If Not _sessionManager.CurrentUser.Role = "Guest" And Not _sessionManager.CurrentUser.Id = fileShared.UploadedBy Then

                Dim accessedFile = New FilesAccessed With {
                    .UserId = _sessionManager.CurrentUser.Id,
                    .FileId = fileId,
                    .AccessedAt = Date.Now
                }

                'Check if theres recent user accessed files
                Dim accessedFilesList = Await Task.Run(Function() _fileDataService.GetAccessedFiles(_sessionManager.CurrentUser)).ConfigureAwait(True)

                If Not accessedFilesList.Count > 0 Then

                    'Check if file accessed already exist on accessed list
                    Dim _accessedFiles = Await Task.Run(Function() _fileDataService.GetAccessedFileByUserFile(accessedFile)).ConfigureAwait(True)

                    If _accessedFiles Is Nothing Then
                        If Not Await Task.Run(Function() _fileDataService.SetAccessFile(accessedFile)).ConfigureAwait(True) Then
                            PopUp.Information("Failed", "Theres a problem while accessing the file")
                            Return
                        End If

                        Dim activity = New Activities With {
                            .Action = "Accessed a file",
                            .ActionIn = "Accessed Files",
                            .ActionAt = Date.Now,
                            .FileId = fileShared.Id,
                            .FileName = $"{fileShared.FileName}{fileShared.FileType}",
                            .UserId = _sessionManager.CurrentUser.Id
                        }

                        If Await Task.Run(Function() _activityService.AddActivity(activity)).ConfigureAwait(True) Then
                            _navigationService.GoBack()
                        End If
                    End If
                End If
            End If

            Dim parameters = New NavigationParameters From {
                {"fileId", fileId.Value}
            }

            _navigationService.Go("PageRegion", "FileDetailsView", "Public Files", parameters)
        Catch ex As Exception
            Debug.WriteLine($"[PublicFilesView] OnViewCommand Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Function

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

            _navigationService.Start("PageRegion", "PublicFilesView", "Public Files")

            _publicFiles = Await Task.Run(Function() _fileDataService.GetPublicFiles()).ConfigureAwait(True)

            If _publicFiles IsNot Nothing AndAlso _publicFiles.Count > 0 Then
                Dim orderedDates = _publicFiles.Select(Function(f) f.CreatedAt).OrderBy(Function(d) d).ToList()

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
            IsPublicSelected = True
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
        If _publicFiles Is Nothing Then
            Debug.WriteLine("[FILTER] No files loaded")
            Return
        End If

        Dim filtered = _publicFiles.AsEnumerable()

        If Not String.IsNullOrEmpty(SearchInput) Then
            filtered = filtered.Where(Function(f) f.Name.Contains(SearchInput, StringComparison.OrdinalIgnoreCase))
        End If

        If FromSelectedDate.HasValue Then
            Dim fromDate = FromSelectedDate.Value.Date
            filtered = filtered.Where(Function(f) f.CreatedAt >= fromDate)
        End If

        If ToSelectedDate.HasValue Then
            Dim toDate = ToSelectedDate.Value.Date.AddDays(1)
            filtered = filtered.Where(Function(f) f.CreatedAt < toDate)
        End If

        Dim privacyFilter As New List(Of String) From {"Public"}
        filtered = filtered.Where(Function(f) privacyFilter.Contains(f.Privacy))

        Dim typeConditions As New List(Of String)
        If IsDocsSelected Then typeConditions.Add("DOCS")
        If IsMediaSelected Then typeConditions.Add("MEDIA")
        If IsImageSelected Then typeConditions.Add("IMAGE")
        If IsCompressedSelected Then typeConditions.Add("COMPRESSED")
        If IsExeSelected Then typeConditions.Add("EXE")
        If IsTextSelected Then typeConditions.Add("TEXT")

        If Not IsAllTypesSelected Then
            ResultCount = 0
            DataGridFiles = New ObservableCollection(Of FilesShared)()
            RaisePropertyChanged(NameOf(DataGridFiles))
            RaisePropertyChanged(NameOf(ResultCount))
            Return
        End If

        If typeConditions.Count > 0 Then
            filtered = filtered.Where(Function(f)
                                          Return typeConditions.Contains(FileTypeModel.GetCategoryByExtension(f.FileType))
                                      End Function)
        End If

        ResultCount = filtered.Count
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
