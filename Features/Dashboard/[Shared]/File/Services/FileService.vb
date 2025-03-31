Imports System.IO
Imports System.Net
Imports System.Diagnostics
Imports System.Reflection
Imports System.Security.Policy

Public Class FileService
    Implements IFileService

    Private ReadOnly _fileSharedRepository As FileSharedRepository
    Private ReadOnly _fileInfoService As IFileInfoService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _downloadService As IDownloadService

    Public Property Success As Boolean

    Public Sub New(fileSharedRepository As FileSharedRepository, fileInfoService As IFileInfoService, sessionManager As ISessionManager, downloadService As IDownloadService)
        _fileSharedRepository = fileSharedRepository
        _fileInfoService = fileInfoService
        _sessionManager = sessionManager
        _downloadService = downloadService
    End Sub

    Public Function UploadFile(filesShared As FilesShared) As FileResult Implements IFileService.UploadFile
        Dim destinationPath As String = ""

        Try
            If Not ConnectToNetworkShare() Then
                Return New FileResult With {.Success = False, .FileExists = False, .Message = "Failed to connect to the Server"}
            End If

            If Not Directory.Exists(FolderPath) Then
                Directory.CreateDirectory(FolderPath)
            End If

            _fileInfoService.Extract(filesShared.FilePath)

            Dim newFileName As String = $"{filesShared.FileName}_{_sessionManager.CurrentUser.Username}{_fileInfoService.Type}"
            destinationPath = Path.Combine(FolderPath, newFileName)

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
            DisconnectFromNetworkShare(FolderPath)
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
            DisconnectFromNetworkShare(FolderPath)
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

            If _fileSharedRepository.Update(filesShared) Then
                Debug.WriteLine($"[File Service - UpdateFile] File updated successfully")
                Return New FileResult With {.Success = True, .Message = "File updated successfully"}
            Else
                Debug.WriteLine($"[File Service - UpdateFile] Failed to update file in repository")
                Return New FileResult With {.Success = False, .Message = "Failed to update file"}
            End If
        Catch ex As Exception
            Debug.WriteLine($"[File Service - UpdateFile] Error: {ex.Message}")
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

            If Not _fileSharedRepository.Delete(filesShared) Then
                Return New FileResult With {.Success = False, .Message = "Failed to delete file in repository"}
            End If

            If Not File.Exists(filesShared.FilePath) Then
                Return New FileResult With {.Success = False, .Message = "File not found on server"}
            End If

            File.Delete(filesShared.FilePath)

            Return New FileResult With {.Success = True, .Message = "File successfully removed"}

        Catch ex As Exception
            _fileSharedRepository.Insert(filesShared)

            Debug.WriteLine($"[DEBUG] Delete error: {ex.Message}")
            Return New FileResult With {.Success = False, .Message = $"Error deleting file: {ex.Message}"}
        Finally
            DisconnectFromNetworkShare(FolderPath)
        End Try
    End Function

End Class