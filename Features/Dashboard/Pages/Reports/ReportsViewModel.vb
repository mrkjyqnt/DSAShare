Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation
Imports Prism.Navigation.Regions
Imports System.Collections.ObjectModel

#Disable Warning
Public Class ReportsViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime
    Implements INavigationAware

    Private ReadOnly _reportsService As IReportsService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _fallBackService As IFallbackService

    Private _reports As List(Of Reports)

    Private _searchInput As String
    Private _resultCount As String
    Private _fromDate As DateTime?
    Private _fromMinDate As DateTime?
    Private _fromMaxDate As DateTime?
    Private _toDate As DateTime?
    Private _toMinDate As DateTime?
    Private _toMaxDate As DateTime?
    Private _isBothSelected As Boolean? = True
    Private _isPendingSelected As Boolean
    Private _isResolvedSelected As Boolean

    Private _dataGridReports As ObservableCollection(Of Reports)
    Private _isProcessingSelection As Boolean = False
    Private _selectedReport As Reports

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

    Public Property IsPendingSelected As Boolean
        Get
            Return _isPendingSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isPendingSelected, value) AndAlso value Then
                IsBothSelected = False
                IsResolvedSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property IsResolvedSelected As Boolean
        Get
            Return _isResolvedSelected
        End Get
        Set(value As Boolean)
            If SetProperty(_isResolvedSelected, value) AndAlso value Then
                IsBothSelected = False
                IsPendingSelected = False
                ApplyFilters()
            End If
        End Set
    End Property

    Public Property DataGridReports As ObservableCollection(Of Reports)
        Get
            Return _dataGridReports
        End Get
        Set(value As ObservableCollection(Of Reports))
            SetProperty(_dataGridReports, value)
        End Set
    End Property

    Public Property SelectedReport As Reports
        Get
            Return _selectedReport
        End Get
        Set(value As Reports)
            If _isProcessingSelection Then Return

            _isProcessingSelection = True
            Try
                If SetProperty(_selectedReport, value) AndAlso value IsNot Nothing Then
                    If SelectedReportCommand.CanExecute(value) Then
                        SelectedReportCommand.Execute(value)
                    End If
                End If
            Finally
                Application.Current.Dispatcher.BeginInvoke(Sub()
                                                               SetProperty(_selectedReport, Nothing)
                                                               _isProcessingSelection = False
                                                           End Sub)
            End Try
        End Set
    End Property

    Public Sub New(reportsService As IReportsService,
                  sessionManager As ISessionManager,
                  navigationService As INavigationService,
                  fallBackService As IFallbackService)

        _reportsService = reportsService
        _sessionManager = sessionManager
        _navigationService = navigationService
        _fallBackService = fallBackService

        ' Initialize commands
        DataGridReports = New ObservableCollection(Of Reports)()
        SelectedReportCommand = New DelegateCommand(Of Reports)(AddressOf OnReportSelected)
    End Sub

    Private Async Sub Load()
        Try
            Loading.Show()

            If Not Await Fallback.CheckConnection() Then
                Return
            End If

            _navigationService.Start("PageRegion", "ReportsView", "Reports")
            _reports = Await Task.Run(Function() _reportsService.GetReports()).ConfigureAwait(True)

            If _reports IsNot Nothing AndAlso _reports.Count > 0 Then
                Dim orderedDates = _reports.Select(Function(f) f.ReportedAt).OrderBy(Function(d) d).ToList()

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
            IsPendingSelected = False
            IsResolvedSelected = False

            ApplyFilters()
        Catch ex As Exception
            Debug.WriteLine($"[ReportsViewModel] Load Error: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Private Async Sub OnReportSelected(selectedReport As Reports)
        Dim parameters = New NavigationParameters()

        Try
            If selectedReport Is Nothing Then
                Await PopUp.Information("Failed", "No report was selected")
                Return
            End If

            ' Handle different report statuses
            Select Case selectedReport.Status?.ToLower()
                Case "pending"
                    parameters.Add("reportId", selectedReport.Id)
                    parameters.Add("fileId", selectedReport.FileId)
                    _navigationService.Go("PageRegion", "ReportDetailsView", "Reports", parameters)

                Case "resolved"
                    parameters.Add("reportId", selectedReport.Id)
                    parameters.Add("fileId", selectedReport.FileId)
                    _navigationService.Go("PageRegion", "ReportDetailsView", "Reports", parameters)

                Case Else
                    Await PopUp.Information("Report Info", $"File ID: {selectedReport.FileId}{Environment.NewLine}Reporter: {selectedReport.ReporterId}{Environment.NewLine}Status: {selectedReport.Status}")
            End Select

        Catch ex As Exception
            Debug.WriteLine($"[ReportsViewModel] Report selection failed: {ex.Message}")
            PopUp.Information("Error", "An unexpected error occurred while processing the report.")
        End Try
    End Sub

    Private Sub ApplyFilters()
        If _reports Is Nothing Then
            Debug.WriteLine("[FILTER] No reports loaded")
            Return
        End If

        Dim filtered = _reports.AsEnumerable()
        Debug.WriteLine($"[FILTER] Starting with {filtered.Count()} reports")

        If Not String.IsNullOrEmpty(SearchInput) Then
            filtered = filtered.Where(Function(f)
                                          Return (f.ReporterDescription?.Contains(SearchInput, StringComparison.OrdinalIgnoreCase) = True OrElse
                                                 (f.AdminDescription?.Contains(SearchInput, StringComparison.OrdinalIgnoreCase) = True))
                                      End Function)
            Debug.WriteLine($"[FILTER] After search: {filtered.Count()}")
        End If

        If FromSelectedDate.HasValue Then
            Dim fromDate = FromSelectedDate.Value.Date
            filtered = filtered.Where(Function(f) f.ReportedAt >= fromDate)
        End If

        If ToSelectedDate.HasValue Then
            Dim toDate = ToSelectedDate.Value.Date.AddDays(1)
            filtered = filtered.Where(Function(f) f.ReportedAt < toDate)
        End If

        If IsBothSelected Then
            ' Show all reports
            filtered = filtered.Where(Function(f) True)
        ElseIf IsPendingSelected Then
            ' Show only pending reports
            filtered = filtered.Where(Function(f) f.Status?.ToLower() = "pending")
        ElseIf IsResolvedSelected Then
            ' Show only resolved reports
            filtered = filtered.Where(Function(f) f.Status?.ToLower() = "resolved")
        End If

        ResultCount = filtered.Count()
        DataGridReports = New ObservableCollection(Of Reports)(filtered.ToList())
        RaisePropertyChanged(NameOf(DataGridReports))
        RaisePropertyChanged(NameOf(ResultCount))
    End Sub

    Public Async Sub OnNavigatedTo(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedTo
        Try
            Load()
        Catch ex As Exception
            Debug.WriteLine($"[ReportsViewModel] OnNavigatedTo Error: {ex.Message}")
        End Try
    End Sub

    Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements IRegionAware.IsNavigationTarget
        Return True
    End Function

    Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements IRegionAware.OnNavigatedFrom
    End Sub

    Public ReadOnly Property SelectedReportCommand As ICommand

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property
End Class