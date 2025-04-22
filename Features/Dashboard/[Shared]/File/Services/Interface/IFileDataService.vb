Public Interface IFileDataService

    Function GetPublicFiles() As List(Of FilesShared)
    Function GetSharedFiles(users As Users) As List(Of FilesShared)
    Function GetAccessedFiles(users As Users) As List(Of FilesAccessed)
    Function GetSharedFileInfo(filesShared As FilesShared) As FilesShared
    Function GetSharedFileById(fileShared As FilesShared) As FilesShared
    Function AddAccessFile(filesAccesed As FilesAccessed) As Boolean
    Function GetAccessedFileByUserFile(filesAccesed As FilesAccessed) As FilesAccessed
    Function RemoveAccessedFile(filesAccesed As FilesAccessed) As Boolean
    Function GetAllAccessedFiles(filesAccessed As FilesAccessed) As List(Of FilesAccessed)
    Function GetSharedFileByPrivate(fileShared As FilesShared) As FilesShared
    Function RemoveSharedFile(filesShared As FilesShared) As Boolean
    Function GetAllSharedFiles() As List(Of FilesShared)
    Function RemoveAllSharedFileAccess(fileShared As FilesShared) As Boolean
    Function GetSharedFileByUploader(fileShared As FilesShared) As List(Of FilesShared)
    Function UpdateSharedFile(filesShared As FilesShared) As Boolean
    Function DisableAllSharedFileByUploader(fileShared As FilesShared) As Boolean
End Interface
