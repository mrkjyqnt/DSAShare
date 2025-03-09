Imports System.Data
Imports System.IO
Imports System.Net.WebRequestMethods

Public Class FileSharedRepository
    Private ReadOnly _connection As Connection

    Public Sub New(connection As Connection)
        _connection = connection
    End Sub

    ''' <summary>
    ''' Updates the download count of a file shared record.
    ''' </summary>
    ''' <param name="filesShared">The FilesShared object containing updated data.</param>
    ''' <returns></returns>
    Public Function GetById(fileShared As FilesShared) As FilesShared

        _connection.Prepare("SELECT * FROM files_shared WHERE id = @file_id")
        _connection.AddParam("@file_id", fileShared.Id)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New FilesShared() With {
                .Id = _connection.DataRow("id"),
                .FileName = _connection.DataRow("file_name").ToString(),
                .FilePath = _connection.DataRow("file_path").ToString(),
                .UploadedBy = _connection.DataRow("uploaded_by"),
                .ShareType = _connection.DataRow("share_type").ToString(),
                .ShareCode = If(_connection.DataRow.IsNull("share_code"), Nothing, _connection.DataRow("share_code").ToString()),
                .ExpiryDate = If(_connection.DataRow.IsNull("expiry_date"), Nothing, _connection.DataRow("expiry_date")),
                .Privacy = _connection.DataRow("privacy").ToString(),
                .DownloadCount = _connection.DataRow("download_count"),
                .CreatedAt = _connection.DataRow("created_at")
            }
        End If

        Return Nothing
    End Function


    ''' <summary>
    ''' Updates the download count of a file shared record.
    ''' </summary>
    ''' <param name="filesShared">The FilesShared object containing updated data.</param>
    ''' <returns></returns>
    Public Function Read(fileShared As FilesShared) As FilesShared

        _connection.Prepare("SELECT * FROM files_shared")
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New FilesShared() With {
                .Id = _connection.DataRow("id"),
                .FileName = _connection.DataRow("file_name").ToString(),
                .FilePath = _connection.DataRow("file_path").ToString(),
                .UploadedBy = _connection.DataRow("uploaded_by"),
                .ShareType = _connection.DataRow("share_type").ToString(),
                .ShareCode = If(_connection.DataRow.IsNull("share_code"), Nothing, _connection.DataRow("share_code").ToString()),
                .ExpiryDate = If(_connection.DataRow.IsNull("expiry_date"), Nothing, _connection.DataRow("expiry_date")),
                .Privacy = _connection.DataRow("privacy").ToString(),
                .DownloadCount = _connection.DataRow("download_count"),
                .CreatedAt = _connection.DataRow("created_at")
            }
        End If

        Return Nothing
    End Function


    ''' <summary>
    ''' Inserts a new file shared record into the database.
    ''' </summary>
    ''' <param name="filesShared">The FilesShared object containing file data.</param>
    Public Function Insert(filesShared As FilesShared) As Boolean
        _connection.Prepare("INSERT INTO files_shared (file_name, file_path, uploaded_by, share_type, share_code, expiry_date, privacy, download_count, created_at) " &
                              "VALUES (@file_name, @file_path, @uploaded_by, @share_type, @share_code, @expiry_date, @privacy, @download_count, @created_at)")
        _connection.AddParam("@file_name", filesShared.FileName)
        _connection.AddParam("@file_path", filesShared.FilePath)
        _connection.AddParam("@uploaded_by", filesShared.UploadedBy)
        _connection.AddParam("@share_type", filesShared.ShareType)
        _connection.AddParam("@share_code", filesShared.ShareCode)
        _connection.AddParam("@expiry_date", filesShared.ExpiryDate)
        _connection.AddParam("@privacy", filesShared.Privacy)
        _connection.AddParam("@download_count", filesShared.DownloadCount)
        _connection.AddParam("@created_at", filesShared.CreatedAt)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If

        If _connection.HasChanges Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Updates the download count of a file shared record.
    ''' </summary>
    ''' <param name="filesShared">The FilesShared object containing updated data.</param>
    Public Function Update(filesShared As FilesShared) As Boolean

        _connection.Prepare("SELECT * 
                            FROM files_shared 
                            WHERE id = @id")
        _connection.AddParam("@id", filesShared.Id)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If


        If Not _connection.HasRecord Then
            _connection.ErrorMessage = "File doesn't exists."
            Return True
        End If

        _connection.Prepare("UPDATE files_shared 
                            SET file_name = @file_name, 
                                file_path = @file_path, 
                                uploaded_by = @uploaded_by,
                                share_type = @share_type, 
                                share_code = @share_code, 
                                expiry_date = @expiry_date, 
                                privacy = @privacy, 
                                download_count = @download_count, 
    q                           created_at = @created_at 
                            WHERE id = @file_id")
        _connection.AddParam("@file_name", filesShared.FileName)
        _connection.AddParam("@file_path", filesShared.FilePath)
        _connection.AddParam("@uploaded_by", filesShared.UploadedBy)
        _connection.AddParam("@share_type", filesShared.ShareType)
        _connection.AddParam("@share_code", filesShared.ShareCode)
        _connection.AddParam("@expiry_date", filesShared.ExpiryDate)
        _connection.AddParam("@privacy", filesShared.Privacy)
        _connection.AddParam("@download_count", filesShared.DownloadCount)
        _connection.AddParam("@created_at", filesShared.CreatedAt)
        _connection.Execute()

        If _connection.HasChanges Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Deletes a file shared record by its ID.
    ''' </summary>
    ''' <param name="fileId">The ID of the file shared record to delete.</param>
    Public Function Delete(filesShared As FilesShared) As Boolean

        _connection.Prepare("SELECT * FROM files_shared WHERE id = @file_id")
        _connection.AddParam("@file_id", filesShared.Id)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If


        If Not _connection.HasRecord Then
            _connection.ErrorMessage = "File doesn't exists."
            Return True
        End If

        _connection.Prepare("DELETE FROM files_shared WHERE id = @file_id")
        _connection.AddParam("@file_id", filesShared.Id)  
        _connection.Execute()

        If _connection.HasChanges Then
            Return True
        End If

        Return False
    End Function
End Class