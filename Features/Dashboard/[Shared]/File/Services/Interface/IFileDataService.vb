Public Interface IFileDataService
    Property PublicFilesCount As Integer
    Property SharedFilesCount As Integer
    Property AccessedFilesCount As Integer

    Sub GetAllCount()
    Function GetPublicFiles() As List(Of FilesShared)
    Function GetSharedFiles(users As Users) As List(Of FilesShared)
    Function GetAccessedFiles(users As Users) As List(Of FilesAccessed)
    Function GetSharedFileInfo(filesShared As FilesShared) As FilesShared
    Function GetSharedFileById(fileShared As FilesShared) As FilesShared
    Function SetAccessFile(filesAccesed As FilesAccessed) As Boolean
    Function GetAccessedFileByUserFile(filesAccesed As FilesAccessed) As FilesAccessed
    Function RemoveAccessedFile(filesAccesed As FilesAccessed) As Boolean
    Function GetAllAccessedFiles(filesAccessed As FilesAccessed) As List(Of FilesAccessed)
    Function GetSharedFileByPrivate(fileShared As FilesShared) As FilesShared
End Interface
