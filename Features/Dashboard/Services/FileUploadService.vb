Imports System.IO

Public Class FileUploadService
    Implements IFileUploadService

    Private ReadOnly _fileSharedRepository As FileSharedRepository

    Public Sub New(fileSharedRepository As FileSharedRepository)
        _fileSharedRepository = fileSharedRepository
    End Sub

    ''' <summary>
    ''' Uploads a file to the shared folder and saves the file details to the database.
    ''' </summary>
    ''' <param name="filePath"></param>
    ''' <param name="uploadedBy"></param>
    ''' <param name="shareType"></param>
    ''' <param name="shareCode"></param>
    ''' <param name="expiryDate"></param>
    ''' <param name="privacy"></param>
    ''' <returns></returns>
    Public Function UploadFile(filePath As String, uploadedBy As Integer, shareType As String, Optional shareCode As String = Nothing, Optional expiryDate As DateTime? = Nothing, Optional privacy As String = "Public") As Boolean Implements IFileUploadService.UploadFile
        Try
            ' Ensure the shared folder exists
            Dim sharedFolderPath As String = "\\DSA-SERVER\SharedFiles\"
            If Not Directory.Exists(sharedFolderPath) Then
                Directory.CreateDirectory(sharedFolderPath)
            End If

            ' Get the file name and create the destination path
            Dim fileName As String = Path.GetFileName(filePath)
            Dim destinationPath As String = Path.Combine(sharedFolderPath, fileName)

            ' Copy the file to the shared folder
            File.Copy(filePath, destinationPath, True)

            ' Save the file details to the database using the repository
            Dim filesShared As New FilesShared() With {
                .FileName = fileName,
                .FilePath = destinationPath,
                .UploadedBy = uploadedBy,
                .ShareType = shareType,
                .ShareCode = shareCode,
                .ExpiryDate = expiryDate,
                .Privacy = privacy,
                .DownloadCount = 0,
                .CreatedAt = DateTime.Now
            }

            Return _fileSharedRepository.Insert(filesShared)
        Catch ex As Exception
            ' Log the error
            Console.WriteLine($"Error uploading file: {ex.Message}")
            Return False
        End Try
    End Function
End Class