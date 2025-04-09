Imports System.Data

Public Class FileAccessedRepository
    Private ReadOnly _connection As Connection

    Public Sub New(connection As Connection)
        _connection = connection
    End Sub

    ''' <summary>
    ''' Get one access file by file ID
    ''' </summary>
    ''' <param name="filesAccessed"></param>
    ''' <returns></returns>
    Public Function GetByFileId(filesAccessed As FilesAccessed) As FilesAccessed
        Dim accessList As New List(Of FilesAccessed)()

        _connection.Prepare("SELECT * FROM files_accessed WHERE file_id = @file_id")
        _connection.AddParam("@file_id", filesAccessed.FileId)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileAccessedRepository] GetByFileId error: {_connection.ErrorMessage}")
            Return New FilesAccessed
        End If

        If _connection.HasRecord Then
            Return New FilesAccessed() With {
            .Id = _connection.DataRow("id"),
            .UserId = _connection.DataRow("user_id"),
            .FileId = _connection.DataRow("file_id"),
            .AccessedAt = _connection.DataRow("accessed_at")
        }
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Get all access files by User ID
    ''' </summary>
    ''' <param name="filesAccessed"></param>
    ''' <returns></returns>
    Public Function GetAllByUserId(filesAccessed As FilesAccessed) As List(Of FilesAccessed)
        Dim accessList As New List(Of FilesAccessed)()

        _connection.Prepare("SELECT * FROM files_accessed WHERE user_id = @user_id ORDER BY id DESC")
        _connection.AddParam("@user_id", filesAccessed.UserId)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileAccessedRepository] GetAllByUserId error: {_connection.ErrorMessage}")
            Return New List(Of FilesAccessed)
        End If

        Dim records = _connection.FetchAll()

        For Each record As DataRow In records
            Dim access As New FilesAccessed() With {
                .Id = record("id"),
                .UserId = record("user_id"),
                .FileId = record("file_id"),
                .AccessedAt = record("accessed_at")
            }

            accessList.Add(access)
        Next

        Return accessList
    End Function

    ''' <summary>
    ''' Read all Accessed File Data
    ''' </summary>
    ''' <returns></returns>
    Public Function Read() As List(Of FilesAccessed)
        Dim accessList As New List(Of FilesAccessed)()

        _connection.Prepare("SELECT * FROM files_accessed ORDER BY id DESC")
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileAccessedRepository] Theres an error: {_connection.ErrorMessage}")
            Return New List(Of FilesAccessed)
        End If

        Dim records = _connection.FetchAll()

        For Each record As DataRow In records
            Dim access As New FilesAccessed() With {
                .Id = record("id"),
                .UserId = record("user_id"),
                .FileId = record("file_id"),
                .AccessedAt = record("accessed_at")
            }

            accessList.Add(access)
        Next

        Return accessList
    End Function


    ''' <summary>
    ''' Insert Accessed Files for recording
    ''' </summary>
    ''' <param name="filesAccessed"></param>
    ''' <returns></returns>
    Public Function Insert(filesAccessed As FilesAccessed) As Boolean

        _connection.Prepare("SELECT * FROM files_accessed WHERE user_id = @user_id AND file_id = @file_id")
        _connection.AddParam("@user_id", filesAccessed.UserId)
        _connection.AddParam("@file_id", filesAccessed.FileId)
        _connection.Execute()

        If _connection.HasRecord Then
            Return False
        End If

        _connection.Prepare("INSERT INTO files_accessed (user_id, file_id, accessed_at) " &
                              "VALUES (@user_id, @file_id, @accessed_at)")
        _connection.AddParam("@user_id", filesAccessed.UserId)
        _connection.AddParam("@file_id", filesAccessed.FileId)
        _connection.AddParam("@accessed_at", filesAccessed.AccessedAt)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileAccessedRepository] Insert error: {_connection.ErrorMessage}")
            Return False
        End If

        If _connection.HasChanges Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Update Access Data
    ''' </summary>
    ''' <param name="filesAccessed"></param>
    ''' <returns></returns>
    Public Function Update(filesAccessed As FilesAccessed) As Boolean

        _connection.Prepare("SELECT * FROM files_accessed WHERE id = @id")
        _connection.AddParam("@id", filesAccessed.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileAccessedRepository] Update error: {_connection.ErrorMessage}")
            Return False
        End If

        If Not _connection.HasRecord Then
            Return False
        End If

        _connection.Prepare("UPDATE files_accessed" &
                              "SET @user_id WHERE id = @id ")
        _connection.AddParam("@user_id", filesAccessed.UserId)
        _connection.AddParam("@file_id", filesAccessed.FileId)
        _connection.AddParam("@accessed_at", filesAccessed.AccessedAt)
        _connection.AddParam("@id", filesAccessed.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileAccessedRepository] Update error: {_connection.ErrorMessage}")
            Return False
        End If

        If _connection.HasChanges Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Remove Accessed for certain data
    ''' </summary>
    ''' <param name="filesAccessed"></param>
    ''' <returns></returns>
    Public Function Delete(filesAccessed As FilesAccessed) As Boolean

        _connection.Prepare("SELECT * FROM files_accessed WHERE id = @id")
        _connection.AddParam("@id", filesAccessed.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileAccessedRepository] Delete error: {_connection.ErrorMessage}")
            Return False
        End If

        If Not _connection.HasRecord Then
            Return True
        End If

        _connection.Prepare("DELETE FROM files_accessed WHERE id = @id")
        _connection.AddParam("@id", filesAccessed.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileAccessedRepository] Delete error: {_connection.ErrorMessage}")
            Return False
        End If

        If _connection.HasChanges Then
            Return True
        End If

        Return False
    End Function

    Public Function DeleteAll(filesAccessed As FilesAccessed) As Boolean
        _connection.Prepare("DELETE FROM files_accessed WHERE user_id = @user_id")
        _connection.AddParam("@user_id", filesAccessed.UserId)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileAccessedRepository] DeleteAll error: {_connection.ErrorMessage}")
            Return False
        End If

        If _connection.HasChanges Then
            Return True
        End If

        Return False
    End Function
End Class
