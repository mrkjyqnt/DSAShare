Imports System.Data
Imports System.IO
Imports System.Net.WebRequestMethods

Public Class FileSharedRepository
    Private ReadOnly _connection As Connection

    Public Sub New(connection As Connection)
        _connection = connection
    End Sub

    ''' <summary>
    ''' Get all the shared files by fileId
    ''' </summary>
    ''' <param name="filesShared">The FilesShared object containing updated data.</param>
    ''' <returns></returns>
    Public Function GetById(fileShared As FilesShared) As FilesShared

        _connection.Prepare("SELECT * FROM files_shared WHERE id = @file_id ORDER BY id DESC")
        _connection.AddParam("@file_id", fileShared.Id)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New FilesShared() With {
                .Id = _connection.DataRow("id"),
                .Name = _connection.DataRow("name").ToString(),
                .FileName = _connection.DataRow("file_name").ToString(),
                .FileDescription = _connection.DataRow("file_description").ToString(),
                .FilePath = _connection.DataRow("file_path").ToString(),
                .FileSize = _connection.DataRow("file_size").ToString(),
                .FileType = _connection.DataRow("file_type").ToString(),
                .UploadedBy = _connection.DataRow("uploaded_by").ToString(),
                .ShareType = If(_connection.DataRow.IsNull("share_type"), Nothing, _connection.DataRow("share_type").ToString()),
                .ShareValue = If(_connection.DataRow.IsNull("share_value"), Nothing, _connection.DataRow("share_value").ToString()),
                .ExpiryDate = If(_connection.DataRow.IsNull("expiry_date"), Nothing, _connection.DataRow("expiry_date")),
                .Privacy = _connection.DataRow("privacy").ToString(),
                .DownloadCount = _connection.DataRow("download_count"),
                .Availability = _connection.DataRow("availability").ToString(),
                .CreatedAt = _connection.DataRow("created_at"),
                .UpdatedAt = _connection.DataRow("updated_at")
            }
        End If

        Return Nothing
    End Function

    Public Function GetByExact(filesShared As FilesShared) As FilesShared
        _connection.Prepare("SELECT * FROM files_shared WHERE name = @name AND file_name = @file_name AND file_size = @file_size AND file_type = @file_type AND uploaded_by = @uploaded_by ORDER BY id DESC")
        _connection.AddParam("@name", filesShared.Name)
        _connection.AddParam("@file_name", filesShared.FileName)
        _connection.AddParam("@file_path", filesShared.FilePath)
        _connection.AddParam("@file_size", filesShared.FileSize)
        _connection.AddParam("@file_type", filesShared.FileType)
        _connection.AddParam("@uploaded_by", filesShared.UploadedBy)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New FilesShared() With {
                .Id = _connection.DataRow("id"),
                .Name = _connection.DataRow("name").ToString(),
                .FileName = _connection.DataRow("file_name").ToString(),
                .FileDescription = _connection.DataRow("file_description").ToString(),
                .FilePath = _connection.DataRow("file_path").ToString(),
                .FileSize = _connection.DataRow("file_size").ToString(),
                .FileType = _connection.DataRow("file_type").ToString(),
                .UploadedBy = _connection.DataRow("uploaded_by"),
                .ShareType = _connection.DataRow("share_type").ToString(),
                .ShareValue = If(_connection.DataRow.IsNull("share_value"), Nothing, _connection.DataRow("share_value").ToString()),
                .ExpiryDate = If(_connection.DataRow.IsNull("expiry_date"), Nothing, _connection.DataRow("expiry_date")),
                .Privacy = _connection.DataRow("privacy").ToString(),
                .DownloadCount = _connection.DataRow("download_count"),
                .Availability = _connection.DataRow("availability").ToString(),
                .CreatedAt = _connection.DataRow("created_at"),
                .UpdatedAt = _connection.DataRow("updated_at")
            }
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Retrieve FileShared Datas by Privacy
    ''' </summary>
    ''' <param name="fileShared"></param>
    ''' <returns></returns>
    Public Function GetByPrivacy(fileShared As FilesShared) As List(Of FilesShared)
        Dim filesList As New List(Of FilesShared)()

        _connection.Prepare("SELECT * FROM files_shared WHERE privacy = @privacy ORDER BY id DESC")
        _connection.AddParam("@privacy", fileShared.Privacy)
        _connection.AddParam("@uploaded_by", CInt(fileShared.UploadedBy))
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        Dim records = _connection.FetchAll()

        For Each record As DataRow In records
            Dim file As New FilesShared() With {
                .Id = record("id"),
                .Name = record("name").ToString(),
                .FileName = record("file_name").ToString(),
                .FileDescription = record("file_description").ToString(),
                .FilePath = record("file_path").ToString(),
                .FileSize = record("file_size").ToString(),
                .FileType = record("file_type").ToString().Trim(),
                .UploadedBy = record("uploaded_by"),
                .ShareType = record("share_type").ToString(),
                .ShareValue = If(record.IsNull("share_value"), Nothing, record("share_value").ToString()),
                .ExpiryDate = If(record.IsNull("expiry_date"), Nothing, record("expiry_date")),
                .Privacy = record("privacy").ToString(),
                .DownloadCount =record("download_count"),
                .Availability = record("availability").ToString(),
                .CreatedAt = record("created_at"),
                .UpdatedAt = record("updated_at")
            }

            filesList.Add(file)
        Next

        Return filesList
    End Function

    ''' <summary>
    ''' Retrieve FileShared Datas by Privacy
    ''' </summary>
    ''' <param name="fileShared"></param>
    ''' <returns></returns>
    Public Function GetByUploader(fileShared As FilesShared) As List(Of FilesShared)
        Dim filesList As New List(Of FilesShared)()

        _connection.Prepare("SELECT * FROM files_shared WHERE uploaded_by = @uploaded_by ORDER BY id DESC")
        _connection.AddParam("@uploaded_by", fileShared.UploadedBy)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        Dim records = _connection.FetchAll()

        For Each record As DataRow In records
            Dim file As New FilesShared() With {
                .Id = record("id"),
                .Name = record("name").ToString(),
                .FileName = record("file_name").ToString(),
                .FileDescription = record("file_description").ToString(),
                .FilePath = record("file_path").ToString(),
                .FileSize = record("file_size").ToString(),
                .FileType = record("file_type").ToString().Trim(),
                .UploadedBy = record("uploaded_by"),
                .ShareType = record("share_type").ToString(),
                .ShareValue = If(record.IsNull("share_value"), Nothing, record("share_value").ToString()),
                .ExpiryDate = If(record.IsNull("expiry_date"), Nothing, record("expiry_date")),
                .Privacy = record("privacy").ToString(),
                .DownloadCount =record("download_count"),
                .Availability = record("availability").ToString(),
                .CreatedAt = record("created_at"),
                .UpdatedAt = record("updated_at")
            }

            filesList.Add(file)
        Next

        Return filesList
    End Function


    ''' <summary>
    ''' Updates the download count of a file shared record.
    ''' </summary>
    ''' <param name="filesShared">The FilesShared object containing updated data.</param>
    ''' <returns></returns>
    Public Function Read() As List(Of FilesShared)
        Dim filesList As New List(Of FilesShared)()

        _connection.Prepare("SELECT * FROM files_shared ORDER BY id DESC")
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        ' Use FetchAll() to get all records
        Dim records = _connection.FetchAll()

        ' Iterate through each record using For Each
        For Each record As DataRow In records
            Dim file As New FilesShared() With {
                .Id = record("id"),
                .Name = record("name").ToString(),
                .FileName = record("file_name").ToString(),
                .FileDescription = record("file_description").ToString(),
                .FilePath = record("file_path").ToString(),
                .FileSize = record("file_size").ToString(),
                .FileType = record("file_type").ToString().Trim(),
                .UploadedBy = record("uploaded_by"),
                .ShareType = record("share_type").ToString(),
                .ShareValue = If(record.IsNull("share_value"), Nothing, record("share_value").ToString()),
                .ExpiryDate = If(record.IsNull("expiry_date"), Nothing, record("expiry_date")),
                .Privacy = record("privacy").ToString(),
                .DownloadCount =record("download_count"),
                .Availability = record("availability").ToString(),
                .CreatedAt = record("created_at"),
                .UpdatedAt = record("updated_at")
            }

            filesList.Add(file)
        Next

        Return filesList
    End Function



    ''' <summary>
    ''' Inserts a new file shared record into the database.
    ''' </summary>
    ''' <param name="filesShared">The FilesShared object containing file data.</param>
    Public Function Insert(filesShared As FilesShared) As Boolean
        _connection.Prepare("INSERT INTO files_shared (name, file_name, file_description, file_path, file_size, file_type, uploaded_by, share_type, share_value, expiry_date, privacy, download_count, availability, created_at, updated_at) " &
                              "VALUES (@name, @file_name, @file_description, @file_path, @file_size, @file_type, @uploaded_by, @share_type, @share_value, @expiry_date, @privacy, @download_count, @availability, @created_at, @updated_at)")
        _connection.AddParam("@name", filesShared.Name)
        _connection.AddParam("@file_name", filesShared.FileName)
        _connection.AddParam("@file_description", filesShared.FileDescription)
        _connection.AddParam("@file_path", filesShared.FilePath)
        _connection.AddParam("@file_size", filesShared.FileSize)
        _connection.AddParam("@file_type", filesShared.FileType)
        _connection.AddParam("@uploaded_by", filesShared.UploadedBy)
        _connection.AddParam("@share_type", filesShared.ShareType)
        _connection.AddParam("@share_value", filesShared.ShareValue)
        _connection.AddParam("@expiry_date", filesShared.ExpiryDate)
        _connection.AddParam("@privacy", filesShared.Privacy)
        _connection.AddParam("@download_count", filesShared.DownloadCount)
        _connection.AddParam("@availability", filesShared.Availability)
        _connection.AddParam("@created_at", filesShared.CreatedAt)
        _connection.AddParam("@updated_at", filesShared.UpdatedAt)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine(_connection.ErrorMessage)
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
            Debug.WriteLine($"[DEBUG] Error: {_connection.ErrorMessage}")
            Return False
        End If


        If Not _connection.HasRecord Then
            Debug.WriteLine($"[DEBUG] Error: File doesn't exist")
            Return True
        End If

        _connection.Prepare("UPDATE files_shared 
                            SET name = @name,
                                file_name = @file_name, 
                                file_description = @file_description, 
                                file_path = @file_path, 
                                file_size = @file_size, 
                                file_type = @file_type, 
                                uploaded_by = @uploaded_by,
                                share_type = @share_type, 
                                share_value = @share_value, 
                                expiry_date = @expiry_date, 
                                privacy = @privacy, 
                                download_count = @download_count, 
                                availability = @availability, 
                                created_at = @created_at,
                                updated_at = @updated_at 
                            WHERE id = @file_id")
        _connection.AddParam("@name", filesShared.Name)
        _connection.AddParam("@file_name", filesShared.FileName)
        _connection.AddParam("@file_description", filesShared.FileDescription)
        _connection.AddParam("@file_path", filesShared.FilePath)
        _connection.AddParam("@file_size", filesShared.FileSize)
        _connection.AddParam("@file_type", filesShared.FileType)
        _connection.AddParam("@uploaded_by", filesShared.UploadedBy)
        _connection.AddParam("@share_type", filesShared.ShareType)
        _connection.AddParam("@share_value", filesShared.ShareValue)
        _connection.AddParam("@expiry_date", filesShared.ExpiryDate)
        _connection.AddParam("@privacy", filesShared.Privacy)
        _connection.AddParam("@download_count", filesShared.DownloadCount)
        _connection.AddParam("@availability", filesShared.Availability)
        _connection.AddParam("@created_at", filesShared.CreatedAt)
        _connection.AddParam("@updated_at", filesShared.UpdatedAt)
        _connection.AddParam("@file_id", filesShared.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[DEBUG] Error: {_connection.ErrorMessage}")
            Return False
        End If


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