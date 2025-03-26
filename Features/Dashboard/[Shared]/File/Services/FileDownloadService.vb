Imports System.IO

Public Class FileDownloadService
    Implements IFileDownloadService

    Private ReadOnly _fileSharedRepository As FileSharedRepository
    Private _filesShared As FilesShared

    Public Sub New(fileSharedRepository As FileSharedRepository)
        _fileSharedRepository = fileSharedRepository
    End Sub

    ''' <summary>
    ''' Downloads a file from the shared folder to the destination folder.
    ''' </summary>
    ''' <param name="fileId"></param>
    ''' <param name="destinationFolder"></param>
    ''' <returns></returns>
    Public Function DownloadFile(fileId As Integer, destinationFolder As String) As Boolean Implements IFileDownloadService.DownloadFile
        
        _filesShared = New FilesShared With {
            .Id = fileId
        }   

        Try
            ' Fetch the file details from the database using the repository
            Dim filesShared As FilesShared = _fileSharedRepository.GetById(_filesShared)

            If filesShared IsNot Nothing Then
                ' Ensure the destination folder exists
                If Not Directory.Exists(destinationFolder) Then
                    Directory.CreateDirectory(destinationFolder)
                End If

                ' Copy the file to the destination folder
                Dim destinationPath As String = Path.Combine(destinationFolder, filesShared.FileName)
                File.Copy(filesShared.FilePath, destinationPath, True)

                ' Increment the download count in the database
                filesShared.DownloadCount += 1
                _fileSharedRepository.Update(filesShared)

                Return True
            Else
                ' File not found in the database
                Return False
            End If
        Catch ex As Exception
            ' Log the error
            Console.WriteLine($"Error downloading file: {ex.Message}")
            Return False
        End Try
    End Function
End Class