Imports System.Data

Public Class FileBlockedRepository
    Private ReadOnly _connection As Connection

    Public Sub New(connection As Connection)
        _connection = connection
    End Sub

    Public Function Read() As List(Of FilesBlocked)
        Dim blockedList As New List(Of FilesBlocked)()

        _connection.Prepare("SELECT * FROM files_blocked ORDER BY blocked_at DESC")
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileBlockedRepository] Read error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        Dim records = _connection.FetchAll()

        For Each record As DataRow In records
            Dim blocked As New FilesBlocked() With {
                .Id = If(record.IsNull("id"), Nothing, record("id")),
                .FileHash = If(record.IsNull("file_hash"), Nothing, record("file_hash").ToString()),
                .Reason = If(record.IsNull("reason"), Nothing, record("reason").ToString()),
                .BlockedBy = If(record.IsNull("blocked_by"), Nothing, record("blocked_by")),
                .BlockedAt = If(record.IsNull("blocked_at"), Nothing, record("blocked_at"))
            }

            blockedList.Add(blocked)
        Next

        Return blockedList
    End Function

    Public Function Insert(blocked As FilesBlocked) As Boolean
        _connection.Prepare("INSERT INTO files_blocked (file_hash, reason, blocked_by, blocked_at) " &
                            "VALUES (@file_hash, @reason, @blocked_by, @blocked_at)")

        _connection.AddParam("@file_hash", blocked.FileHash)
        _connection.AddParam("@reason", blocked.Reason)
        _connection.AddParam("@blocked_by", blocked.BlockedBy)
        _connection.AddParam("@blocked_at", blocked.BlockedAt)

        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileBlockedRepository] Insert error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasChanges
    End Function

    Public Function IsHashBlocked(fileHash As String) As Boolean
        _connection.Prepare("SELECT * FROM files_blocked WHERE file_hash = @file_hash")
        _connection.AddParam("@file_hash", fileHash)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileBlockedRepository] IsHashBlocked error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasRecord
    End Function

    Public Function Delete(blocked As FilesBlocked) As Boolean
        _connection.Prepare("SELECT * FROM files_blocked WHERE id = @id")
        _connection.AddParam("@id", blocked.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileBlockedRepository] Delete error: {_connection.ErrorMessage}")
            Return False
        End If

        If Not _connection.HasRecord Then
            Debug.WriteLine("[FileBlockedRepository] Delete Error: Record doesn't exist.")
            Return False
        End If

        _connection.Prepare("DELETE FROM files_blocked WHERE id = @id")
        _connection.AddParam("@id", blocked.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileBlockedRepository] Delete error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasChanges
    End Function
End Class
