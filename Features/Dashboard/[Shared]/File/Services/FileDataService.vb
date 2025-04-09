Imports System.IO

Public Class FileDataService
    Implements IFileDataService

    Private ReadOnly _fileSharedRepository As FileSharedRepository
    Private ReadOnly _fileAccessedRepository As FileAccessedRepository
    Private ReadOnly _sessionManager As ISessionManager

    Public Property PublicFilesCount As Integer
    Public Property SharedFilesCount As Integer
    Public Property AccessedFilesCount As Integer

    Public Sub New(fileSharedRepository As FileSharedRepository, fileAccessedRepository As FileAccessedRepository, sessionManager As ISessionManager)
        _fileAccessedRepository = fileAccessedRepository
        _fileSharedRepository = fileSharedRepository
        _sessionManager = sessionManager
    End Sub

    Public Function GetPublicFiles() As List(Of FilesShared) Implements IFileDataService.GetPublicFiles
        Try
            Dim _fileShared = New FilesShared With {
                .Privacy = "Public"
            }

            Return _fileSharedRepository.GetByPrivacy(_fileShared).Where(Function(f) f.Availability = "Available").ToList()
        Catch ex As Exception
            Debug.WriteLine("[FileDataService] Failed to retrieve public files: " & ex.Message)
            Return New List(Of FilesShared)() ' Return empty list instead of Nothing
        End Try
    End Function

    Public Function GetSharedFiles(users As Users) As List(Of FilesShared) Implements IFileDataService.GetSharedFiles
        Try
            If users Is Nothing Then
                Debug.WriteLine("[FileDataService] GetSharedFiles: User information is missing.")
                Return New List(Of FilesShared)()
            End If

            Dim _fileShared = New FilesShared With {
                .UploadedBy = users.Id
            }

            Dim result = _fileSharedRepository.GetByUploader(_fileShared)

            If result.Count = 0 Or result Is Nothing Then
                result = _fileSharedRepository.GetByUploader(_fileShared)
            End If

            Return result
        Catch ex As Exception
            Debug.WriteLine("[FileDataService] Failed to retrieve shared files. " & ex.Message)
            Return New List(Of FilesShared)()
        End Try
    End Function

    Public Function GetAccessedFiles(users As Users) As List(Of FilesAccessed) Implements IFileDataService.GetAccessedFiles
        Try
            If users Is Nothing Then
                Debug.WriteLine("[FileDataService] GetAccessedFiles: User information is missing.")
                Return New List(Of FilesAccessed)()
            End If

            Dim filesAccessed = New FilesAccessed With {
                .UserId = users.Id
            }

            Dim result = _fileAccessedRepository.GetAllByUserId(filesAccessed)

            If result.Count = 0 Or result Is Nothing Then
                result = _fileAccessedRepository.GetAllByUserId(filesAccessed)
            End If

            Return result
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] GetAccessedFiles Error: {ex.Message}")
            Return New List(Of FilesAccessed)()
        End Try
    End Function

    ''' <summary>
    ''' Get the shared file information.
    ''' </summary>
    ''' <param name="filesShared"></param>
    ''' <returns></returns>
    Public Function GetSharedFileInfo(filesShared As FilesShared) As FilesShared Implements IFileDataService.GetSharedFileInfo
        Try
            If filesShared Is Nothing Then
                Debug.WriteLine("[FileDataService] GetSharedFileInfo: filesShared is nothing")
                Return Nothing
            End If

            Return _fileSharedRepository.GetByExact(filesShared)
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] GetSharedFileInfo Error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Get the accessed file information.
    ''' </summary>
    ''' <param name="filesAccessed"></param>
    ''' <returns></returns>
    Public Function GetSharedFileById(fileShared As FilesShared) As FilesShared Implements IFileDataService.GetSharedFileById
        Try
            If fileShared Is Nothing Then
                Debug.WriteLine("[FileDataService] GetSharedFileInfo: fileShared is nothing")
                Return Nothing
            End If
            Return _fileSharedRepository.GetById(fileShared)
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] GetSharedFileById Error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Function GetSharedFileByPrivate(fileShared As FilesShared) As FilesShared Implements IFileDataService.GetSharedFileByPrivate
        Try
            If fileShared Is Nothing Then
                Debug.WriteLine("[FileDataService] GetSharedFileByPrivate: fileShared is nothing")
                Return Nothing
            End If

            Return _fileSharedRepository.GetByShareType(fileShared)
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] GetSharedFileByPrivate Error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Function GetAllSharedFiles() As List(Of FilesShared) Implements IFileDataService.GetAllSharedFiles
        Try
            Dim result = _fileSharedRepository.Read()
            If result.Count = 0 Then
                Debug.WriteLine("[FileDataService] GetAllSharedFiles: No files shared.")
                Return Nothing
            End If
            Return result
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] GetAllSharedFiles Error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Function RemoveSharedFile(filesShared As FilesShared) As Boolean Implements IFileDataService.RemoveSharedFile
        Try
            If filesShared Is Nothing Then
                Debug.WriteLine("[FileDataService] RemoveSharedFile: filesShared is nothing.")
                Return False
            End If

            Return _fileSharedRepository.Delete(filesShared)
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] RemoveAccessedFile Error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Function SetAccessFile(filesAccesed As FilesAccessed) As Boolean Implements IFileDataService.SetAccessFile
        Try
            If filesAccesed Is Nothing Then
                Debug.WriteLine("[FileDataService] SetAccessFile: filesAccesed is nothing.")
                Return False
            End If
            Return _fileAccessedRepository.Insert(filesAccesed)
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] SetAccessFile Error: {ex.Message}")
            Return False
        End Try
    End Function

    Public Function GetAccessedFileByUserFile(filesAccesed As FilesAccessed) As FilesAccessed Implements IFileDataService.GetAccessedFileByUserFile
        Try
            If filesAccesed Is Nothing Then
                Debug.WriteLine("[FileDataService] GetAccessedFileByUserFile: filesAccesed is nothing.")
                Return Nothing
            End If

            Dim result = _fileAccessedRepository.GetAllByUserId(filesAccesed)

            If result.Count > 0 Then
                Return result.Where(Function(i) i.FileId = filesAccesed.FileId).FirstOrDefault
            End If

            Return Nothing
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] GetAccessedFileByUserFile Error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Function GetAccessedFileByFileId(filesAccesed As FilesAccessed) As FilesAccessed Implements IFileDataService.GetAccessedFileByFileId
        Try
            If filesAccesed Is Nothing Then
                Debug.WriteLine("[FileDataService] GetAccessedFileByUserFile: filesAccesed is nothing.")
                Return Nothing
            End If

            Return _fileAccessedRepository.GetByFileId(filesAccesed)
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] GetAccessedFileByUserFile Error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Function RemoveAccessedFile(filesAccesed As FilesAccessed) As Boolean Implements IFileDataService.RemoveAccessedFile
        Try
            If filesAccesed Is Nothing Then
                Debug.WriteLine("[FileDataService] RemoveAccessedFile: filesAccesed is nothing.")
                Return False
            End If

            Return _fileAccessedRepository.Delete(filesAccesed)
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] RemoveAccessedFile Error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Function GetAllAccessedFiles(filesAccessed As FilesAccessed) As List(Of FilesAccessed) Implements IFileDataService.GetAllAccessedFiles
        Try
            Dim result = _fileAccessedRepository.Read()

            If result.Count = 0 Then
                Debug.WriteLine("[FileDataService] GetAllAccessedFiles: No files accessed.")
                Return Nothing
            End If

            Return If(result.Where(Function(f) f.FileId = filesAccessed.FileId).ToList(), Nothing)
        Catch ex As Exception
            Debug.WriteLine($"[FileDataService] GetAllAccessedFiles Error: {ex.Message}")
            Return Nothing
        End Try
    End Function
End Class
