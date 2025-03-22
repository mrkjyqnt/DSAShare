Imports System.Windows.Threading
Imports Prism.Commands
Imports Prism.Events
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Class HomeViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _fileDataService As IFileDataService
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

    Public ReadOnly Property ShareFilesCommand As DelegateCommand
    Public ReadOnly Property AccessFilesCommand As DelegateCommand

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sessionManager"></param>
    ''' <param name="regionManager"></param>
    ''' <param name="fileDataService"></param>
    Public Sub New(fileDataService As IFileDataService, 
                   navigationService As INavigationService)

        _fileDataService = fileDataService
        _navigationService = navigationService

        ShareFilesCommand = New DelegateCommand(AddressOf OnShareFilesCommand)
        AccessFilesCommand = New DelegateCommand(AddressOf OnAccessFilesCommand)

        _publicText = "0"
        _accessedText = "0"
        _sharedText = "0"

        Load()
    End Sub

    Private Async Sub Load()
        Try
            Loading.Show

            Await Task.Run(Sub() _fileDataService.GetAllCount()).ConfigureAwait(True)

            PublicText = _fileDataService.PublicFilesCount.ToString()
            SharedText = _fileDataService.SharedFilesCount.ToString()
            AccessedText = _fileDataService.AccessedFilesCount.ToString()
        Catch ex As Exception
            PopUp.Information("Error", ex.Message)
        Finally
            Loading.Hide
        End Try
    End Sub

    Private Sub OnShareFilesCommand()
        _navigationService.Go("PageRegion", "ShareFilesView", "Shared Files")
    End Sub

    Private Sub OnAccessFilesCommand()
        _navigationService.Go("PageRegion", "AccessFilesView", "Accessed Files")
    End Sub

End Class
