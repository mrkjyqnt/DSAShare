Imports System.IO
Imports System.Net
Imports System.Diagnostics
Imports System.Reflection
Imports System.Security.Policy
Imports System.Security.AccessControl

Public Class FileService
    Implements IFileService

    Private ReadOnly _folderPath As String = ConfigurationModule.GetSettings().Network.FolderPath
    Private ReadOnly _fileSharedRepository As FileSharedRepository
    Private ReadOnly _fileInfoService As IFileInfoService
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _downloadService As IDownloadService

    Public Property Success As Boolean

    Public Sub New(fileSharedRepository As FileSharedRepository, fileInfoService As IFileInfoService, fileDataService As IFileDataService, sessionManager As ISessionManager, downloadService As IDownloadService)
        _fileSharedRepository = fileSharedRepository
        _fileInfoService = fileInfoService
        _fileDataService = fileDataService
        _sessionManager = sessionManager
        _downloadService = downloadService
    End Sub

    Public Function UploadFile(filesShared As FilesShared) As FileResult Implements IFileService.UploadFile
        Dim destinationPath As String = ""

        Try
            If Not ConnectToNetworkShare() Then
                Return New FileResult With {.Success = False, .FileExists = False, .Message = "Failed to connect to the Server"}
            End If

            If Not Directory.Exists(_folderPath) Then
                Directory.CreateDirectory(_folderPath)
            End If

            _fileInfoService.Extract(filesShared.FilePath)

            Dim newFileName As String = $"{filesShared.FileName}_{_sessionManager.CurrentUser.Username}{_fileInfoService.Type}"
            destinationPath = Path.Combine(_folderPath, newFileName)

            If File.Exists(destinationPath) Then
                Return New FileResult With {.Success = False, .FileExists = True, .Message = "File already exists"}
            End If

            File.Copy(filesShared.FilePath, destinationPath, False)

            filesShared.FilePath = destinationPath

            Dim insertSuccess = _fileSharedRepository.Insert(filesShared)
            If Not insertSuccess Then
                ' Rollback file copy if DB insert fails
                File.Delete(destinationPath)
                Return New FileResult With {.Success = False, .FileExists = False, .Message = "Database error, upload failed"}
            End If

            Return New FileResult With {.Success = True, .FileExists = False, .Message = "File uploaded successfully"}
        Catch ex As Exception
            If Not String.IsNullOrEmpty(destinationPath) AndAlso File.Exists(destinationPath) Then
                File.Delete(destinationPath)
            End If

            Debug.WriteLine($"[DEBUG] Upload error: {ex.Message}")
            Return New FileResult With {.Success = False, .FileExists = False, .Message = "Error uploading file"}
        Finally
            DisconnectFromNetworkShare(_folderPath)
        End Try
    End Function


    Public Function DownloadFile(filesShared As FilesShared) As FileResult Implements IFileService.DownloadFile
        Dim destinationPath As String = ""

        Try
            ' Connect to the network share
            If Not ConnectToNetworkShare() Then
                Return New FileResult With {.Success = False, .Message = "Failed to connect to the Server"}
            End If

            ' Validate file exists in DB
            If filesShared Is Nothing OrElse String.IsNullOrEmpty(filesShared.FilePath) Then
                Return New FileResult With {.Success = False, .Message = "Invalid file data"}
            End If

            ' Verify if file exists physically
            If Not File.Exists(filesShared.FilePath) Then
                Return New FileResult With {.Success = False, .Message = "File does not exist on the server"}
            End If

            ' Generate a unique file name in Downloads folder
            Dim downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) & "\Downloads"
            Dim sourceFileName As String = Path.GetFileName(filesShared.FilePath)
            destinationPath = GetUniqueFilePath(downloadsFolder, sourceFileName)

            ' Attempt file download
            _downloadService.StartDownloadAsync(filesShared.FilePath, destinationPath)

            ' Only update DownloadCount after successful download
            filesShared.DownloadCount += 1
            If Not _fileSharedRepository.Update(filesShared) Then
                ' Rollback: Remove downloaded file if DB update fails
                If File.Exists(destinationPath) Then File.Delete(destinationPath)
                Return New FileResult With {.Success = False, .Message = "Download successful, but failed to update download count"}
            End If

            Return New FileResult With {.Success = True, .Message = $"File downloaded successfully: {destinationPath}"}

        Catch ex As Exception
            ' Rollback: Remove partially downloaded file
            If Not String.IsNullOrEmpty(destinationPath) AndAlso File.Exists(destinationPath) Then
                File.Delete(destinationPath)
            End If

            Debug.WriteLine($"[DEBUG] Download error: {ex.Message}")
            Return New FileResult With {.Success = False, .Message = $"Error downloading file: {ex.Message}"}

        Finally
            DisconnectFromNetworkShare(_folderPath)
        End Try
    End Function


    ''' <summary>
    ''' Update a file info
    ''' </summary>
    ''' <param name="filesShared"></param>
    ''' <returns></returns>
    Public Function UpdateFile(filesShared As FilesShared) As FileResult Implements IFileService.UpdateFile
        Try
            If filesShared Is Nothing Then
                Return New FileResult With {.Success = False, .Message = "File data is null"}
            End If

            ' Connect to the network share
            If Not ConnectToNetworkShare() Then
                Return New FileResult With {.Success = False, .Message = "Failed to connect to the Server"}
            End If

            ' Remove all the reference access on database
            Dim accessFiles = _fileDataService.GetAllAccessedFiles(New FilesAccessed With {.FileId = filesShared.Id})
            If accessFiles IsNot Nothing Then
                For Each accessFile In accessFiles
                    _fileDataService.RemoveAccessedFile(accessFile)
                Next
            End If

            If _fileSharedRepository.Update(filesShared) Then
                Debug.WriteLine($"[File Service] UpdateFile File updated successfully")
                Return New FileResult With {.Success = True, .Message = "File updated successfully"}
            Else
                Debug.WriteLine($"[File Service] UpdateFile Failed to update file in repository")
                Return New FileResult With {.Success = False, .Message = "Failed to update file"}
            End If
        Catch ex As Exception
            Debug.WriteLine($"[File Service] UpdateFile Error: {ex.Message}")
            Return New FileResult With {.Success = False, .Message = $"Error updating file: {ex.Message}"}
        End Try
    End Function

    Public Function DeleteFile(filesShared As FilesShared) As FileResult Implements IFileService.DeleteFile
        Try
            If filesShared Is Nothing Then
                Return New FileResult With {.Success = False, .Message = "File data is null"}
            End If

            If Not ConnectToNetworkShare() Then
                Return New FileResult With {.Success = False, .Message = "Failed to connect to the Server"}
            End If

            ' Remove all the reference access on database
            Dim accessFiles = _fileDataService.GetAllAccessedFiles(New FilesAccessed With {.FileId = filesShared.Id})
            If accessFiles IsNot Nothing Then
                For Each accessFile In accessFiles
                    _fileDataService.RemoveAccessedFile(accessFile)
                Next
            End If

            ' First delete the database reference regardless of file existence
            If Not _fileSharedRepository.Delete(filesShared) Then
                Return New FileResult With {.Success = False, .Message = "Failed to delete file in repository"}
            End If

            ' Then check if physical file exists and delete it
            If File.Exists(filesShared.FilePath) Then
                File.Delete(filesShared.FilePath)
            End If

            Return New FileResult With {.Success = True, .Message = "File successfully removed"}

        Catch ex As Exception
            ' Rollback: Reinsert the record if something went wrong during physical deletion
            If Not File.Exists(filesShared.FilePath) Then
                _fileSharedRepository.Insert(filesShared)
            End If

            Debug.WriteLine($"[DEBUG] Delete error: {ex.Message}")
            Return New FileResult With {.Success = False, .Message = $"Error deleting file: {ex.Message}"}
        Finally
            DisconnectFromNetworkShare(_folderPath)
        End Try
    End Function

    Public Function DeleteAllFileByUser(user As Users) As FileResult Implements IFileService.DeleteAllFileByUser
        Try
            If user Is Nothing Then
                Return New FileResult With {.Success = False, .Message = "File data is null"}
            End If

            If Not ConnectToNetworkShare() Then
                Return New FileResult With {.Success = False, .Message = "Failed to connect to the Server"}
            End If

            Dim accessFiles = _fileDataService.GetAccessedFiles(user)
            If accessFiles IsNot Nothing Then
                For Each accessFile In accessFiles
                    _fileDataService.RemoveAccessedFile(accessFile)
                Next
            End If

            ' Remove all the reference files on database
            Dim sharedFiles = _fileDataService.GetSharedFiles(New Users With {.Id = user.Id})
            If sharedFiles IsNot Nothing And sharedFiles?.Count > 0 Then
                For Each files In sharedFiles
                    _fileDataService.RemoveSharedFile(files)

                    _fileDataService.RemoveAccessedFile(_fileDataService.GetAccessedFileByUserFile(New FilesAccessed With {.FileId = files.Id}))

                    If File.Exists(files.FilePath) Then
                        File.Delete(files.FilePath)
                    End If
                Next
            End If

            Return New FileResult With {.Success = True, .Message = "File successfully removed"}

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Delete error: {ex.Message}")
            Return New FileResult With {.Success = False, .Message = $"Error deleting file: {ex.Message}"}
        Finally
            DisconnectFromNetworkShare(_folderPath)
        End Try
    End Function

End Class