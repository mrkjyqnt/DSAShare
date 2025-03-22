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
            Dim FolderPath As String = ConfigurationModule.FolderPath
            Dim credentials As NetworkCredential = ConfigurationModule.NetworkCredential

            ConnectToNetworkShare(FolderPath, credentials)

            If Not Directory.Exists(FolderPath) Then
                Directory.CreateDirectory(FolderPath)
            End If

            _fileInfoService.Extract(filesShared.FilePath)

            ' Generate a new file name
            Dim newFileName As String = $"{filesShared.FileName}_{_sessionManager.CurrentUser.Username}{_fileInfoService.Type}"
            Dim destinationPath As String = Path.Combine(FolderPath, newFileName)

            ' Copy the file to the shared folder
            File.Copy(filesShared.FilePath, destinationPath, False)

            ' Save the file details to the database using the repository
            Dim _filesShared = filesShared
            _filesShared.FilePath = destinationPath

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