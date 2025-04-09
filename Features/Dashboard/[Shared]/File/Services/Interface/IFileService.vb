Public Interface IFileService
    Function UploadFile(filesShared As FilesShared) As FileResult
    Function DownloadFile(filesShared As FilesShared) As FileResult
    Function UpdateFile(filesShared As FilesShared) As FileResult
    Function DeleteFile(filesShared As FilesShared) As FileResult
    Function DeleteAllFileByUser(users As Users) As FileResult
End Interface