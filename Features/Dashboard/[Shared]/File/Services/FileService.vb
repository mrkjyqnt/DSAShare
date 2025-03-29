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
        Try
            If Not ConnectToNetworkShare() Then
                Return New FileResult With {.Success = False, .FileExists = False, .Message = "Failed to connect to the Server"}
            End If

            If Not Directory.Exists(FolderPath) Then
                Directory.CreateDirectory(FolderPath)
            End If

            _fileInfoService.Extract(filesShared.FilePath)

            ' Generate a new file name
            Dim newFileName As String = $"{filesShared.FileName}_{_sessionManager.CurrentUser.Username}{_fileInfoService.Type}"
            Dim destinationPath As String = Path.Combine(FolderPath, newFileName)

            ' Check if file exists first
            If File.Exists(destinationPath) Then
                Return New FileResult With {.Success = False, .FileExists = True, .Message = "File already exist, try changing a file or file name"}
            End If

            ' Copy the file to the shared folder
            File.Copy(filesShared.FilePath, destinationPath, False)

            ' Save the file details to the database using the repository
            Dim _filesShared = filesShared
            _filesShared.FilePath = destinationPath

            ' Insert file details into the database
            Dim insertSuccess = _fileSharedRepository.Insert(_filesShared)
            Return New FileResult With {.Success = insertSuccess, .FileExists = False, .Message = "File uploaded sucessfully"}
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] an error happen: {ex.Message} ")
            Return New FileResult With {.Success = False, .FileExists = False, .Message = "Theres an error while uploading the file"}
        Finally
            ' Disconnect from the network share
            DisconnectFromNetworkShare(FolderPath)
        End Try
    End Function

    Public Function DownloadFile(filesShared As FilesShared) As FileResult Implements IFileService.DownloadFile
        Try
            Dim _filesShared As FilesShared = _fileSharedRepository.GetById(filesShared)

            ' Check if the file exists in database
            If _filesShared Is Nothing Then
                Return New FileResult With {.Success = False, .FileExists = False, .Message = "There's an error fetching the file from server"}
            End If

            ' Connect to the network share
            If Not ConnectToNetworkShare() Then
                Return New FileResult With {.Success = False, .FileExists = False, .Message = "Failed to connect to the Server"}
            End If

            ' Verify the source file exists
            If Not File.Exists(_filesShared.FilePath) Then
                Debug.WriteLine($"[DEBUG] File not found: {_filesShared.FilePath}")
                Return New FileResult With {.Success = False, .FileExists = False, .Message = "File doesn't exist on the server"}
            End If

            ' Ensure we have the correct filename with extension
            Dim sourceFileName As String = Path.GetFileName(_filesShared.FilePath)
            If String.IsNullOrEmpty(Path.GetExtension(sourceFileName)) Then
                Debug.WriteLine($"[DEBUG] File has no extension: {_filesShared.FilePath}")
                Return New FileResult With {.Success = False, .FileExists = False, .Message = "File has no extension"}
            End If

            ' Generate unique destination path in Downloads folder
            Dim downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) & "\Downloads"
            Dim destinationPath As String = GetUniqueFilePath(downloadsFolder, sourceFileName)

            ' Verify the source file before starting download
            Dim fileInfo As New FileInfo(_filesShared.FilePath)
            If fileInfo.Length = 0 Then
                Debug.WriteLine($"[DEBUG] File is empty: {_filesShared.FilePath}")
                Return New FileResult With {.Success = False, .FileExists = False, .Message = "File is empty"}
            End If

            ' Start the download using DownloadService
            Dim downloadId = _downloadService.StartDownloadAsync(_filesShared.FilePath, destinationPath).Result

            ' Update download count
            _filesShared.DownloadCount += 1
            _fileSharedRepository.Update(_filesShared)

            Return New FileResult With {
                .Success = True,
                .FileExists = True,
                .Message = $"Added to your Downloads as {Path.GetFileName(destinationPath)}"
            }

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error initiating download: {ex.Message}")
            Return New FileResult With {.Success = False, .FileExists = False, .Message = $"Error initiating download: {ex.Message}"}
        Finally
            ' Disconnect from the network share
            DisconnectFromNetworkShare(FolderPath)
        End Try
    End Function

End Class