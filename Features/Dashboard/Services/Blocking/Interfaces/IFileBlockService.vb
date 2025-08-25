Public Interface IFileBlockService
    Function IsBlocked(filePath As String) As Boolean
    Function BlockFile(fileBlock As FilesBlocked) As Boolean
    Function UnblockById(id As Integer) As Boolean
    Function GetBlockedList() As List(Of FilesBlocked)
End Interface
