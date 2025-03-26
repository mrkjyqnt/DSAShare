Public Interface IFileDownloadService
    Function DownloadFile(fileId As Integer, destinationFolder As String) As Boolean
End Interface