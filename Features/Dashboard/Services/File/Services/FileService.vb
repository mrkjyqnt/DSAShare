Imports System.IO
Imports System.Diagnostics
Imports System.Threading
Imports System.Net

#Disable Warning
Public Class FileService
    Implements IFileService, IDisposable

    Private ReadOnly _folderPath As String = ConfigurationModule.GetSettings().Network.FolderPath
    Private ReadOnly _fileSharedRepository As FileSharedRepository
    Private ReadOnly _fileInfoService As IFileInfoService
    Private ReadOnly _fileDataService As IFileDataService
    Private ReadOnly _sessionManager As ISessionManager
    Private ReadOnly _downloadService As IDownloadService
    Private ReadOnly _reportsService As IReportsService

    ' Connection state tracking
    Private _isConnected As Boolean = False
    Private ReadOnly _connectionLock As New Object()
    Private _activeOperations As Integer = 0

    ' Buffer constants
    Private Const LargeFileThreshold As Long = 10 * 1024 * 1024 ' 10MB
    Private Const SmallBufferSize As Integer = 81920 ' 80KB
    Private Const LargeBufferSize As Integer = 1024 * 1024 ' 1MB

    Public Sub New(fileSharedRepository As FileSharedRepository,
                   fileInfoService As IFileInfoService,
                   fileDataService As IFileDataService,
                   sessionManager As ISessionManager,
                   downloadService As IDownloadService,
                   reportsService As IReportsService)

        _fileSharedRepository = fileSharedRepository
        _fileInfoService = fileInfoService
        _fileDataService = fileDataService
        _sessionManager = sessionManager
        _downloadService = downloadService
        _reportsService = reportsService

        If _folderPath.StartsWith("\\") Then
            ServicePointManager.UseNagleAlgorithm = False
            ServicePointManager.Expect100Continue = False
        End If
    End Sub

    '=======================
    ' CONNECTION MANAGEMENT
    '=======================
    Private Function BeginOperation() As Boolean
        SyncLock _connectionLock
            _activeOperations += 1
            If Not _isConnected Then
                _isConnected = ConnectToNetworkShare()
                If Not _isConnected Then
                    _activeOperations -= 1
                    Return False
                End If
            End If
            Return True
        End SyncLock
    End Function

    Private Sub EndOperation()
        SyncLock _connectionLock
            _activeOperations -= 1
            If _activeOperations <= 0 AndAlso _isConnected Then
                DisconnectFromNetworkShare(_folderPath)
                _isConnected = False
            End If
        End SyncLock
    End Sub

    '=======================
    ' UPLOAD
    '=======================
    Public Function UploadFile(fileShared As FilesShared, progress As IProgress(Of Integer)) As FileResult Implements IFileService.UploadFile
        Dim destinationPath As String = ""

        If Not BeginOperation() Then
            Return New FileResult With {.Success = False, .Message = "Failed to connect to the Server"}
        End If

        Try
            ' === Validation ===
            If fileShared Is Nothing OrElse String.IsNullOrEmpty(fileShared.FilePath) Then
                Return New FileResult With {.Success = False, .Message = "Invalid file data"}
            End If

            If Not File.Exists(fileShared.FilePath) Then
                Return New FileResult With {.Success = False, .Message = "Source file does not exist"}
            End If

            If Not VirusScanner.ScanFile(fileShared.FilePath) Then
                Return New FileResult With {.Success = False, .Message = "Virus detected"}
            End If

            ' === File Prep ===
            _fileInfoService.Extract(fileShared.FilePath)

            Dim fileName = If(String.IsNullOrEmpty(fileShared.FileName),
                              Path.GetFileNameWithoutExtension(fileShared.FilePath),
                              fileShared.FileName)

            Dim fileType = If(String.IsNullOrEmpty(fileShared.FileType),
                              _fileInfoService.Type,
                              fileShared.FileType)

            Dim newFileName = $"{fileName}_{_sessionManager.CurrentUser.Username}_{DateTime.Now.Ticks}{fileType}"
            destinationPath = Path.Combine(_folderPath, newFileName)

            If Not Directory.Exists(_folderPath) Then Directory.CreateDirectory(_folderPath)

            ' === Copy File with Progress ===
            Dim fileSize = New FileInfo(fileShared.FilePath).Length
            Dim bufferSize = If(fileSize > LargeFileThreshold, LargeBufferSize, SmallBufferSize)

            Using sourceStream As New FileStream(fileShared.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan),
                  destStream As New FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, FileOptions.WriteThrough)

                Dim buffer(bufferSize - 1) As Byte
                Dim totalBytes As Long = 0
                Dim bytesRead As Integer
                Dim lastPercent As Integer = -1

                Do
                    bytesRead = sourceStream.Read(buffer, 0, buffer.Length)
                    If bytesRead > 0 Then
                        destStream.Write(buffer, 0, bytesRead)
                        totalBytes += bytesRead
                        Dim percent = CInt((totalBytes * 100L) \ fileSize)
                        If percent > lastPercent Then
                            lastPercent = percent
                            progress?.Report(percent)
                        End If
                    End If
                Loop While bytesRead > 0
            End Using

            fileShared.FilePath = newFileName
            If Not _fileSharedRepository.Insert(fileShared) Then
                File.Delete(destinationPath)
                Return New FileResult With {.Success = False, .Message = "Failed to save to database"}
            End If

            Return New FileResult With {.Success = True, .Message = "File uploaded successfully"}

        Catch ex As Exception
            If Not String.IsNullOrEmpty(destinationPath) AndAlso File.Exists(destinationPath) Then
                File.Delete(destinationPath)
            End If
            Return New FileResult With {.Success = False, .Message = $"Upload failed: {ex.Message}"}
        Finally
            EndOperation()
        End Try
    End Function

    '=======================
    ' DOWNLOAD
    '=======================
    Public Function DownloadFile(filesShared As FilesShared) As FileResult Implements IFileService.DownloadFile
        Dim destinationPath As String = ""

        If Not BeginOperation() Then
            Return New FileResult With {.Success = False, .Message = "Failed to connect to the Server"}
        End If

        Try
            If filesShared Is Nothing OrElse String.IsNullOrEmpty(filesShared.FilePath) Then
                Return New FileResult With {.Success = False, .Message = "Invalid file data"}
            End If

            Dim fullPath = Path.Combine(_folderPath, filesShared.FilePath)
            If Not File.Exists(fullPath) Then
                Return New FileResult With {.Success = False, .Message = "File not found on server"}
            End If

            destinationPath = GetUniqueFilePath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) & "\Downloads", filesShared.FilePath)
            _downloadService.StartDownloadAsync(fullPath, destinationPath)

            filesShared.DownloadCount += 1
            If Not _fileSharedRepository.Update(filesShared) Then
                Return New FileResult With {.Success = True, .Message = "Downloaded, but failed to update count"}
            End If

            Return New FileResult With {.Success = True, .Message = $"Downloaded: {destinationPath}"}
        Catch ex As Exception
            If File.Exists(destinationPath) Then File.Delete(destinationPath)
            Return New FileResult With {.Success = False, .Message = $"Download failed: {ex.Message}"}
        Finally
            EndOperation()
        End Try
    End Function


    Public Function UpdateFile(filesShared As FilesShared) As FileResult Implements IFileService.UpdateFile
        If Not BeginOperation() Then
            Return New FileResult With {.Success = False, .Message = "Failed to connect to the Server"}
        End If

        Try
            If filesShared Is Nothing Then
                Return New FileResult With {.Success = False, .Message = "File data is null"}
            End If

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
        Finally
            EndOperation()
        End Try
    End Function

    ''' <summary>
    ''' Deletes File
    ''' </summary>
    ''' <param name="filesShared"></param>
    ''' <returns></returns>
    Public Function DeleteFile(filesShared As FilesShared) As FileResult Implements IFileService.DeleteFile
        If Not BeginOperation() Then
            Return New FileResult With {.Success = False, .Message = "Failed to connect to the Server"}
        End If

        Try
            If filesShared Is Nothing Then
                Return New FileResult With {.Success = False, .Message = "File data is null"}
            End If

            Dim accessFiles = _fileDataService.GetAllAccessedFiles(New FilesAccessed With {.FileId = filesShared.Id})
            If accessFiles IsNot Nothing Then
                For Each accessFile In accessFiles
                    _fileDataService.RemoveAccessedFile(accessFile)
                Next
            End If

            If Not _fileDataService.UpdateSharedFile(filesShared) Then
                Return New FileResult With {.Success = False, .Message = "Failed to delete file in repository"}
            End If

            Dim fullPath = Path.Combine(_folderPath, filesShared.FilePath)
            If File.Exists(fullPath) Then
                File.Delete(fullPath)
            End If

            Return New FileResult With {.Success = True, .Message = "File successfully removed"}

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Delete error: {ex.Message}")
            Return New FileResult With {.Success = False, .Message = $"Error deleting file: {ex.Message}"}
        Finally
            EndOperation()
        End Try
    End Function

    Public Function DeleteAllFileByUser(user As Users) As FileResult Implements IFileService.DeleteAllFileByUser
        If Not BeginOperation() Then
            Return New FileResult With {.Success = False, .Message = "Failed to connect to the Server"}
        End If

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

            Dim reports = _reportsService.GetReportsByUserId(user.Id)
            If reports IsNot Nothing Then
                For Each report In reports
                    _reportsService.DeleteReport(report.Id)
                Next
            End If

            Dim sharedFiles = _fileDataService.GetSharedFiles(New Users With {.Id = user.Id})
            If sharedFiles IsNot Nothing AndAlso sharedFiles.Count > 0 Then
                For Each files In sharedFiles
                    _fileDataService.RemoveSharedFile(files)

                    reports = _reportsService.GetReportsByFileId(files.Id)
                    If reports IsNot Nothing Then
                        For Each report In reports
                            _reportsService.DeleteReport(report.Id)
                        Next
                    End If

                    accessFiles = _fileDataService.GetAllAccessedFiles(New FilesAccessed With {.FileId = files.Id})
                    If accessFiles IsNot Nothing Then
                        For Each accessFile In accessFiles
                            _fileDataService.RemoveAccessedFile(accessFile)
                        Next
                    End If

                    Dim filePath = Path.Combine(_folderPath, files.FilePath)
                    If File.Exists(filePath) Then
                        File.Delete(filePath)
                    End If
                Next
            End If

            Return New FileResult With {.Success = True, .Message = "File successfully removed"}

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Delete error: {ex.Message}")
            Return New FileResult With {.Success = False, .Message = $"Error deleting file: {ex.Message}"}
        Finally
            EndOperation()
        End Try
    End Function

    Private Function GetUniqueFilePath(folderPath As String, originalFileName As String) As String
        Dim fileName = Path.GetFileNameWithoutExtension(originalFileName)
        Dim extension = Path.GetExtension(originalFileName)
        Dim destPath = Path.Combine(folderPath, fileName & extension)
        Dim counter = 1

        While File.Exists(destPath)
            destPath = Path.Combine(folderPath, $"{fileName}({counter}){extension}")
            counter += 1
        End While

        Return destPath
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        SyncLock _connectionLock
            If _isConnected Then
                DisconnectFromNetworkShare(_folderPath)
                _isConnected = False
            End If
        End SyncLock
    End Sub
End Class