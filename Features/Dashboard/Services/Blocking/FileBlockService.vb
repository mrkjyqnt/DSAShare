Imports System.IO

Public Class FileBlockService
    Implements IFileBlockService

    Private ReadOnly _repository As FileBlockedRepository

    Public Sub New(repository As FileBlockedRepository)
        _repository = repository
    End Sub

    ''' <summary>
    ''' Checks if a file is blocked using its hash.
    ''' </summary>
    Public Function IsBlocked(filePath As String) As Boolean Implements IFileBlockService.IsBlocked
        If Not File.Exists(filePath) Then Return False

        Dim hash = ComputeSHA256(filePath)

        Debug.WriteLine($"Checking if file is blocked: {filePath}")
        Return _repository.IsHashBlocked(hash)
    End Function

    ''' <summary>
    ''' Blocks a file by hashing its contents and storing the hash in the DB.
    ''' </summary>
    Public Function BlockFile(fileBlock As FilesBlocked) As Boolean Implements IFileBlockService.BlockFile
        If _repository.IsHashBlocked(fileBlock.FileHash) Then Return False
        Return _repository.Insert(fileBlock)
    End Function

    ''' <summary>
    ''' Unblocks a file by ID.
    ''' </summary>
    Public Function UnblockById(id As Integer) As Boolean Implements IFileBlockService.UnblockById
        Dim toDelete As New FilesBlocked With {.Id = id}
        Return _repository.Delete(toDelete)
    End Function

    ''' <summary>
    ''' Gets all blocked file hashes.
    ''' </summary>
    Public Function GetBlockedList() As List(Of FilesBlocked) Implements IFileBlockService.GetBlockedList
        Return _repository.Read()
    End Function
End Class
