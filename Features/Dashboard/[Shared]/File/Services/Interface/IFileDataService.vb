Public Interface IFileDataService
    Property PublicFilesCount As Integer
    Property SharedFilesCount As Integer
    Property AccessedFilesCount As Integer

    Sub GetAllCount()
    Function GetPublicFiles() As List(Of FilesShared)
    Function GetSharedFiles(users As Users) As List(Of FilesShared)
    Function GetAccessedFiles(users As Users) As List(Of FilesAccessed)
    Function GetSharedFileInfo(filesShared As FilesShared) As FilesShared
End Interface
