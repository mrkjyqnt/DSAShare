Imports System.IO

Public Class FileUploadService
    Implements IFileUploadService

    Private ReadOnly _fileSharedRepository As FileSharedRepository

    Public Sub New(fileSharedRepository As FileSharedRepository)
        _fileSharedRepository = fileSharedRepository
    End Sub
    
    Public Function UploadFile(filesShared As FilesShared) As Boolean Implements IFileUploadService.UploadFile
        Try
            ' Ensure the shared folder exists
            Dim FolderPath As String = "\\192.168.8.10\Server Storage\"
            If Not Directory.Exists(FolderPath) Then
                Directory.CreateDirectory(FolderPath)
            End If

            ' Extract file name and extension
            Dim fileNameWithoutExt As String = Path.GetFileNameWithoutExtension(filesShared.FileName)
            Dim fileExtension As String = Path.GetExtension(filesShared.FileName)

            ' Generate a unique filename with Date (MMddyy) and Author
            Dim currentDate As String = DateTime.Now.ToString("MMddyy")
            Dim safeAuthorName As String = filesShared.UploadedBy.ToString
            Dim newFileName As String = $"{fileNameWithoutExt}_{currentDate}_{safeAuthorName}{fileExtension}"
            Dim destinationPath As String = Path.Combine(FolderPath, newFileName)

            ' Copy the file to the shared folder
            File.Copy(filesShared.FilePath, destinationPath, False)

            ' Save the file details to the database using the repository
            Dim _filesShared As New FilesShared() With {
                .FileName = filesShared.FileName,
                .FilePath = destinationPath,
                .FileSize = filesShared.FileSize,
                .FileType = filesShared.FileType,
                .UploadedBy = filesShared.UploadedBy,
                .ShareType = filesShared.ShareType,
                .ShareValue = filesShared.ShareValue,
                .ExpiryDate = filesShared.ExpiryDate,
                .Privacy = filesShared.Privacy,
                .DownloadCount = 0,
                .CreatedAt = DateTime.Now,
                .UpdatedAt = filesShared.UpdatedAt
            }

            Return _fileSharedRepository.Insert(_filesShared)
        Catch ex As Exception
            ' Log the error
            Console.WriteLine($"Error uploading file: {ex.Message}")
            Return False
        End Try
    End Function
End Class