Imports System.IO

Public Class FileDataService
    Implements IFileDataService

    Private ReadOnly _fileSharedRepository As FileSharedRepository
    Private ReadOnly _fileAccessedRepository As FileAccessedRepository
    Private ReadOnly _sessionManager As ISessionManager

    Public Property PublicFilesCount As Integer
    Public Property SharedFilesCount As Integer
    Public Property AccessedFilesCount As Integer

    Private Property IFileDataService_PublicFilesCount As Integer Implements IFileDataService.PublicFilesCount
        Get
            Return PublicFilesCount
        End Get
        Set(value As Integer)
            PublicFilesCount = value
        End Set
    End Property

    Private Property IFileDataService_SharedFilesCount As Integer Implements IFileDataService.SharedFilesCount
        Get
            Return SharedFilesCount
        End Get
        Set(value As Integer)
            SharedFilesCount = value
        End Set
    End Property

    Private Property IFileDataService_AccessedFilesCount As Integer Implements IFileDataService.AccessedFilesCount
        Get
            Return AccessedFilesCount
        End Get
        Set(value As Integer)
            AccessedFilesCount = value
        End Set
    End Property

    Public Sub New(fileSharedRepository As FileSharedRepository, fileAccessedRepository As FileAccessedRepository, sessionManager As ISessionManager)
        _fileAccessedRepository = fileAccessedRepository
        _fileSharedRepository = fileSharedRepository
        _sessionManager = sessionManager
    End Sub

    Public Sub GetAllCount() Implements IFileDataService.GetAllCount
        Try
            PublicFilesCount = GetPublicFiles()?.Count
            SharedFilesCount = GetSharedFiles(_sessionManager.CurrentUser)?.Count
            AccessedFilesCount = GetAccessedFiles(_sessionManager.CurrentUser)?.Count
        Catch ex As Exception
            PublicFilesCount = 0
            SharedFilesCount = 0
            AccessedFilesCount = 0
        End Try
    End Sub

    Public Function GetPublicFiles() As List(Of FilesShared) Implements IFileDataService.GetPublicFiles
        Try
            Dim _fileShared = New FilesShared With {
                .Privacy = "Public"
            }
            Return _fileSharedRepository.GetByPrivacy(_fileShared)
        Catch ex As Exception
            Debug.WriteLine("[DEBUG] Failed to retrieve public files. " & ex.Message)
            Return New List(Of FilesShared)() ' Return empty list instead of Nothing
        End Try
    End Function

    Public Function GetSharedFiles(users As Users) As List(Of FilesShared) Implements IFileDataService.GetSharedFiles
        Try
            If users Is Nothing Then
                Debug.WriteLine("[DEBUG] GetSharedFiles: User information is missing.")
                Return New List(Of FilesShared)()
            End If

            Dim _fileShared = New FilesShared With {
                .UploadedBy = users.Id
            }

            Return _fileSharedRepository.GetByUploader(_fileShared)
        Catch ex As Exception
            Debug.WriteLine("[DEBUG] Failed to retrieve shared files. " & ex.Message)
            Return New List(Of FilesShared)()
        End Try
    End Function

    Public Function GetAccessedFiles(users As Users) As List(Of FilesAccessed) Implements IFileDataService.GetAccessedFiles
        Try
            If users Is Nothing Then
                Debug.WriteLine("[DEBUG] GetAccessedFiles: User information is missing.")
                Return New List(Of FilesAccessed)()
            End If

            Dim usersAccessed = New FilesAccessed With {
                .UserId = users.Id
            }

            Return New List(Of FilesAccessed)()
        Catch ex As Exception
            Debug.WriteLine("[DEBUG] Failed to retrieve accessed files. " & ex.Message)
            Return New List(Of FilesAccessed)()
        End Try
    End Function

    ''' <summary>
    ''' Get the shared file information.
    ''' </summary>
    ''' <param name="filesShared"></param>
    ''' <returns></returns>
    Public Function GetFileInfo(filesShared As FilesShared) As FilesShared Implements IFileDataService.GetFileInfo
        Try
            If filesShared Is Nothing Then
                Debug.WriteLine("[DEBUG] User information is missing.")
                Return Nothing
            End If

            Return _fileSharedRepository.GetByExact(filesShared)
        Catch ex As Exception
            Debug.WriteLine("[DEBUG] User information is missing.")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Get the accessed file information.
    ''' </summary>
    ''' <param name="filesAccessed"></param>
    ''' <returns></returns>
    Public Function GetFileById(fileShared As FilesShared) As FilesShared Implements IFileDataService.GetFileById
        Try
            If fileShared Is Nothing Then
                Debug.WriteLine("[DEBUG] User information is missing.")
                Return Nothing
            End If
            Return _fileSharedRepository.GetById(fileShared)
        Catch ex As Exception
            Debug.WriteLine("[DEBUG] User information is missing.")
            Return Nothing
        End Try
    End Function
End Class
