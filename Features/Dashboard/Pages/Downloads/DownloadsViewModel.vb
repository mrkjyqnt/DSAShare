Imports Prism.Commands
Imports Prism.Events
Imports Prism.Mvvm
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports Prism.Navigation.Regions
Imports System.Collections
Imports System.Collections.Specialized

#Disable Warning CRR0029
Public Class DownloadsViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _userService As IUserService
    Private ReadOnly _downloadService As IDownloadService
    Private ReadOnly _eventAggregator As IEventAggregator


    Private _downloadHistory As ObservableCollection(Of DownloadHistoryItem)
    Private _downloadCount As String

    Public ReadOnly Property DownloadHistory As ObservableCollection(Of DownloadHistoryItem)
        Get
            Return _downloadHistory
        End Get
    End Property

    Public Property DownloadCount As String
        Get
            Return _downloadCount
        End Get
        Set(value As String)
            SetProperty(_downloadCount, value)
        End Set
    End Property

    Public ReadOnly Property OpenFileCommand As DelegateCommand(Of DownloadHistoryItem)
    Public ReadOnly Property OpenFolderCommand As DelegateCommand(Of DownloadHistoryItem)
    Public ReadOnly Property DeleteFileCommand As AsyncDelegateCommand(Of DownloadHistoryItem)

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Public Sub New(downloadService As IDownloadService, eventAggregator As IEventAggregator, userService As IUserService)
        _downloadService = downloadService
        _eventAggregator = eventAggregator
        _userService = userService
        _downloadHistory = New ObservableCollection(Of DownloadHistoryItem)()

        OpenFileCommand = New DelegateCommand(Of DownloadHistoryItem)(AddressOf OpenFile, Function(item) item IsNot Nothing AndAlso item.IsFileExists)
        OpenFolderCommand = New DelegateCommand(Of DownloadHistoryItem)(AddressOf OpenFolder, Function(item) item IsNot Nothing AndAlso item.IsFileExists)
        DeleteFileCommand = New AsyncDelegateCommand(Of DownloadHistoryItem)(AddressOf OnDeleteFile, Function(item) item IsNot Nothing)

        _eventAggregator.GetEvent(Of DownloadCompletedEvent).Subscribe(AddressOf OnDownloadCompleted)

        LoadDownloadHistory()
        _downloadService.RefreshFileStatuses()
    End Sub

    Private Async Sub LoadDownloadHistory()
        Try
            Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Delay(50)

            Dim history = Await Task.Run(Function() _downloadService.DownloadHistory.ToList()).ConfigureAwait(True)
            DownloadCount = history.Count
            Application.Current.Dispatcher.Invoke(Sub()
                                                      _downloadHistory.Clear()
                                                      For Each item In history
                                                          item.UpdateFileStatus()
                                                          _downloadHistory.Add(item)
                                                      Next
                                                      NotifyCollectionChanged()
                                                  End Sub)

            RaisePropertyChanged(NameOf(DownloadHistory))
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error loading download history: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub

    Private Sub NotifyCollectionChanged()
        ' Create a new list with the same items
        Dim tempList = New ObservableCollection(Of DownloadHistoryItem)(_downloadHistory)
        _downloadHistory.Clear()
        For Each item In tempList
            _downloadHistory.Add(item)
        Next
        RaisePropertyChanged(NameOf(DownloadHistory))
    End Sub


    Private Sub OpenFile(item As DownloadHistoryItem)
        item.UpdateFileStatus()
        If Not item.IsFileExists Then
            ShowFileMissingError(item)
            Return
        End If
        Try
            Process.Start(New ProcessStartInfo(item.FilePath) With {.UseShellExecute = True})
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error opening file: {ex.Message}")
            ShowFileMissingError(item)
        End Try
    End Sub

    Private Sub OpenFolder(item As DownloadHistoryItem)
        item.UpdateFileStatus()
        If Not item.IsFileExists Then
            ShowFileMissingError(item)
            Return
        End If
        Try
            Process.Start("explorer.exe", $"/select,""{item.FilePath}""")
        Catch ex As Exception
            Debug.WriteLine($"Error opening folder: {ex.Message}")
            ShowFileMissingError(item)
        End Try
    End Sub

    Private Async Sub ShowFileMissingError(item As DownloadHistoryItem)
        Try
            item.UpdateFileStatus()
            OpenFileCommand.RaiseCanExecuteChanged()
            OpenFolderCommand.RaiseCanExecuteChanged()
            Await PopUp.Information("File Not Found", "The file has been moved or deleted.").ConfigureAwait(True)
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error showing file missing error: {ex.Message}")
        End Try
    End Sub

    Private Async Function OnDeleteFile(item As DownloadHistoryItem) As Task
        Try
            If Not item.IsFileExists Then
                _downloadService.Remove(item)
                RefreshCommands()
                Await PopUp.Information("Success", $"The file has been removed successfully").ConfigureAwait(True)
                LoadDownloadHistory()
                Return
            End If

            Dim maxAttempts As Integer = 3
            Dim attempts As Integer = 0
            Await PopUp.Information("Confirmation", "Please enter your password to confirm the deletion of the file.").ConfigureAwait(True)

            While attempts < maxAttempts
                attempts += 1
                Dim popUpResult As PopupResult = Await PopUp.Confirmation().ConfigureAwait(True)

                If popUpResult Is Nothing Then
                    Await PopUp.Information("Cancelled", "File deletion was cancelled.").ConfigureAwait(True)
                    Exit Function
                Else
                    Dim enteredPassword = popUpResult.GetValue(Of String)("Input")
                    Dim user = New Users With {.PasswordHash = HashPassword(enteredPassword)}

                    Dim hasPermission = Await Task.Run(Function() _userService.CheckPermission(user)).ConfigureAwait(True)
                    If hasPermission Then
                        _downloadService.Remove(item)
                        RefreshCommands()
                        Await PopUp.Information("Success", $"The file has been removed successfully").ConfigureAwait(True)
                        LoadDownloadHistory()
                        Exit While
                    Else
                        Await PopUp.Information("Failed", $"Invalid Password ({attempts}/{maxAttempts} attempts)").ConfigureAwait(True)
                    End If
                End If
            End While

            If attempts = maxAttempts Then
                Await PopUp.Information("Failed", "Maximum attempts reached. Deletion cancelled.").ConfigureAwait(True)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[DEBU] Error deleting the file: {ex.Message}")
        End Try
    End Function

    Private Sub RefreshCommands()
        OpenFileCommand.RaiseCanExecuteChanged()
        OpenFolderCommand.RaiseCanExecuteChanged()
        DeleteFileCommand.RaiseCanExecuteChanged()
    End Sub

    Private Sub OnDownloadCompleted(item As DownloadHistoryItem)
        LoadDownloadHistory()
    End Sub

    Public Async Function StartDownload(url As String, Optional destinationPath As String = Nothing) As Task
        If String.IsNullOrEmpty(destinationPath) Then
            destinationPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                Path.GetFileName(url))
        End If
        Try
            Await _downloadService.StartDownloadAsync(url, destinationPath).ConfigureAwait(True)
        Catch ex As Exception
            Debug.WriteLine($"Error starting download: {ex.Message}")
        End Try
    End Function
End Class