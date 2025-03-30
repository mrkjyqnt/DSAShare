Public Interface IFileService
    Function UploadFile(filesShared As FilesShared) As FileResult
    Function DownloadFile(filesShared As FilesShared) As FileResult
    Function UpdateFile(filesShared As FilesShared) As FileResult
End Interface