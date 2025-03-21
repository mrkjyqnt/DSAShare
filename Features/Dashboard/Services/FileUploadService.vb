Imports System.IO
Imports System.Net
Imports System.Diagnostics
Imports System.Reflection

Public Class FileUploadService
    Implements IFileUploadService

    Private ReadOnly _fileSharedRepository As FileSharedRepository
    Private ReadOnly _fileInfoService As IFileInfoService
    Private ReadOnly _sessionManager As ISessionManager

    Public Sub New(fileSharedRepository As FileSharedRepository, fileInfoService As IFileInfoService, sessionManager As ISessionManager)
        _fileSharedRepository = fileSharedRepository
        _fileInfoService = fileInfoService
        _sessionManager = sessionManager
    End Sub

    Public Function UploadFile(filesShared As FilesShared) As Boolean Implements IFileUploadService.UploadFile
        Try
            ' Retrieve configuration values securely
            Dim FolderPath As String = ConfigurationModule.FolderPath
            Dim credentials As NetworkCredential = ConfigurationModule.NetworkCredential

            ' Connect to the network share
            ConnectToNetworkShare(FolderPath, credentials)

            ' Ensure the shared folder exists
            If Not Directory.Exists(FolderPath) Then
                Directory.CreateDirectory(FolderPath)
            End If

            ' Extract file information
            _fileInfoService.Extract(filesShared.FilePath)

            ' Validate file name components
            If String.IsNullOrEmpty(filesShared.Name) Then
                Throw New ArgumentNullException("File name cannot be null or empty.")
            End If
            If _sessionManager.CurrentUser Is Nothing OrElse String.IsNullOrEmpty(_sessionManager.CurrentUser.Username) Then
                Throw New ArgumentNullException("Current user username cannot be null or empty.")
            End If
            If _fileInfoService Is Nothing OrElse String.IsNullOrEmpty(_fileInfoService.Type) Then
                Throw New ArgumentNullException("File type cannot be null or empty.")
            End If
            If filesShared.CreatedAt Is Nothing Then
                Throw New ArgumentNullException("CreatedAt cannot be null.")
            End If

            ' Generate a new file name
            Dim newFileName As String = $"{filesShared.FileName}_{_sessionManager.CurrentUser.Username}{_fileInfoService.Type}"
            Dim destinationPath As String = Path.Combine(FolderPath, newFileName)

            ' Copy the file to the shared folder
            File.Copy(filesShared.FilePath, destinationPath, False)

            ' Save the file details to the database using the repository
            Dim _filesShared As New FilesShared() With {
                .Name = filesShared.Name,
                .FileName = filesShared.FileName,
                .FilePath = destinationPath,
                .FileSize = filesShared.FileSize,
                .FileType = filesShared.FileType,
                .UploadedBy = filesShared.UploadedBy,
                .ShareType = filesShared.ShareType,
                .ShareValue = filesShared.ShareValue,
                .ExpiryDate = filesShared.ExpiryDate,
                .Privacy = filesShared.Privacy,
                .DownloadCount = filesShared.DownloadCount,
                .CreatedAt = filesShared.CreatedAt,
                .UpdatedAt = filesShared.UpdatedAt
            }

            For Each prop As PropertyInfo In _filesShared.GetType().GetProperties()
                Dim propName As String = prop.Name
                    Dim propValue As Object = prop.GetValue(_filesShared, Nothing)

                ' Display or process the property name and value
                Debug.WriteLine($"{propName}: {propValue}")
            Next

            ' Insert file details into the database
            Return _fileSharedRepository.Insert(_filesShared)
        Catch ex As Exception
            Return False
        Finally
            ' Disconnect from the network share
            DisconnectFromNetworkShare(FolderPath)
        End Try
    End Function
End Class