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
            PublicFilesCount = Value
        End Set
    End Property

    Private Property IFileDataService_SharedFilesCount As Integer Implements IFileDataService.SharedFilesCount
        Get
            Return SharedFilesCount
        End Get
        Set(value As Integer)
            SharedFilesCount = Value
        End Set
    End Property

    Private Property IFileDataService_AccessedFilesCount As Integer Implements IFileDataService.AccessedFilesCount
        Get
            Return AccessedFilesCount
        End Get
        Set(value As Integer)
            AccessedFilesCount = Value
        End Set
    End Property

    Public Sub New(fileSharedRepository As FileSharedRepository, fileAccessedRepository As FileAccessedRepository, sessionManager As ISessionManager)
        _fileAccessedRepository = fileAccessedRepository
        _fileSharedRepository = fileSharedRepository
        _sessionManager = sessionManager
    End Sub

    Public Sub GetAllCount() Implements IFileDataService.GetAllCount
        PublicFilesCount = GetPublicFiles().Count
        SharedFilesCount = GetSharedFiles(_sessionManager.CurrentUser).Count
        AccessedFilesCount = GetAccessedFiles(_sessionManager.CurrentUser).Count
    End Sub

    Public Function GetPublicFiles() As List(Of FilesShared) Implements IFileDataService.GetPublicFiles
        Dim _fileShared = New FilesShared With {
            .Privacy = "Public"}
        
        Return _fileSharedRepository.GetByPrivacy(_fileShared)
    End Function

    Public Function GetSharedFiles(users As Users) As List(Of FilesShared) Implements IFileDataService.GetSharedFiles
        Dim _fileShared = New FilesShared With {
            .UploadedBy = users.Id}
        
        Return _fileSharedRepository.GetByUploader(_fileShared)
    End Function

    Public Function GetAccessedFiles(users As Users) As List(Of FilesAccessed) Implements IFileDataService.GetAccessedFiles
        Dim usersAccessed = New FilesAccessed With {
            .UserId = users.Id}
        
        Return _fileAccessedRepository.GetByUserId(usersAccessed)
    End Function
End Class
