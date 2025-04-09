Imports System.Data
Imports System.IO
Imports System.Net.WebRequestMethods
Imports System.Windows.Media.Media3D

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
            Debug.WriteLine($"[FileSharedRepository] GetById error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New FilesShared() With {
                .Id = If(_connection.DataRow.IsNull("id"), Nothing, _connection.DataRow("id")),
                .Name = If(_connection.DataRow.IsNull("name"), Nothing, _connection.DataRow("name").ToString()),
                .FileName = If(_connection.DataRow.IsNull("file_name"), Nothing, _connection.DataRow("file_name").ToString()),
                .FileDescription = If(_connection.DataRow.IsNull("file_description"), Nothing, _connection.DataRow("file_description").ToString()),
                .FilePath = If(_connection.DataRow.IsNull("file_path"), Nothing, _connection.DataRow("file_path").ToString()),
                .FileSize = If(_connection.DataRow.IsNull("file_size"), Nothing, _connection.DataRow("file_size").ToString()),
                .FileType = If(_connection.DataRow.IsNull("file_type"), Nothing, _connection.DataRow("file_type").ToString().Trim()),
                .UploadedBy = If(_connection.DataRow.IsNull("uploaded_by"), Nothing, _connection.DataRow("uploaded_by")),
                .ShareType = If(_connection.DataRow.IsNull("share_type"), Nothing, _connection.DataRow("share_type").ToString()),
                .ShareValue = If(_connection.DataRow.IsNull("share_value"), Nothing, _connection.DataRow("share_value").ToString()),
                .ExpiryDate = If(_connection.DataRow.IsNull("expiry_date"), Nothing, _connection.DataRow("expiry_date")),
                .Privacy = If(_connection.DataRow.IsNull("privacy"), Nothing, _connection.DataRow("privacy").ToString()),
                .DownloadCount = If(_connection.DataRow.IsNull("download_count"), Nothing, _connection.DataRow("download_count")),
                .Availability = If(_connection.DataRow.IsNull("availability"), Nothing, _connection.DataRow("availability").ToString()),
                .CreatedAt = If(_connection.DataRow.IsNull("created_at"), Nothing, _connection.DataRow("created_at")),
                .UpdatedAt = If(_connection.DataRow.IsNull("updated_at"), Nothing, _connection.DataRow("updated_at"))
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
            Debug.WriteLine($"[FileSharedRepository] GetByExact error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New FilesShared() With {
                .Id = If(_connection.DataRow.IsNull("id"), Nothing, _connection.DataRow("id")),
                .Name = If(_connection.DataRow.IsNull("name"), Nothing, _connection.DataRow("name").ToString()),
                .FileName = If(_connection.DataRow.IsNull("file_name"), Nothing, _connection.DataRow("file_name").ToString()),
                .FileDescription = If(_connection.DataRow.IsNull("file_description"), Nothing, _connection.DataRow("file_description").ToString()),
                .FilePath = If(_connection.DataRow.IsNull("file_path"), Nothing, _connection.DataRow("file_path").ToString()),
                .FileSize = If(_connection.DataRow.IsNull("file_size"), Nothing, _connection.DataRow("file_size").ToString()),
                .FileType = If(_connection.DataRow.IsNull("file_type"), Nothing, _connection.DataRow("file_type").ToString().Trim()),
                .UploadedBy = If(_connection.DataRow.IsNull("uploaded_by"), Nothing, _connection.DataRow("uploaded_by")),
                .ShareType = If(_connection.DataRow.IsNull("share_type"), Nothing, _connection.DataRow("share_type").ToString()),
                .ShareValue = If(_connection.DataRow.IsNull("share_value"), Nothing, _connection.DataRow("share_value").ToString()),
                .ExpiryDate = If(_connection.DataRow.IsNull("expiry_date"), Nothing, _connection.DataRow("expiry_date")),
                .Privacy = If(_connection.DataRow.IsNull("privacy"), Nothing, _connection.DataRow("privacy").ToString()),
                .DownloadCount = If(_connection.DataRow.IsNull("download_count"), Nothing, _connection.DataRow("download_count")),
                .Availability = If(_connection.DataRow.IsNull("availability"), Nothing, _connection.DataRow("availability").ToString()),
                .CreatedAt = If(_connection.DataRow.IsNull("created_at"), Nothing, _connection.DataRow("created_at")),
                .UpdatedAt = If(_connection.DataRow.IsNull("updated_at"), Nothing, _connection.DataRow("updated_at"))
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
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileSharedRepository] GetByPrivacy error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        Dim records = _connection.FetchAll()

        For Each record As DataRow In records
            Dim file As New FilesShared() With {
                .Id = If(record.IsNull("id"), Nothing, record("id")),
                .Name = If(record.IsNull("name"), Nothing, record("name").ToString()),
                .FileName = If(record.IsNull("file_name"), Nothing, record("file_name").ToString()),
                .FileDescription = If(record.IsNull("file_description"), Nothing, record("file_description").ToString()),
                .FilePath = If(record.IsNull("file_path"), Nothing, record("file_path").ToString()),
                .FileSize = If(record.IsNull("file_size"), Nothing, record("file_size").ToString()),
                .FileType = If(record.IsNull("file_type"), Nothing, record("file_type").ToString().Trim()),
                .UploadedBy = If(record.IsNull("uploaded_by"), Nothing, record("uploaded_by")),
                .ShareType = If(record.IsNull("share_type"), Nothing, record("share_type").ToString()),
                .ShareValue = If(record.IsNull("share_value"), Nothing, record("share_value").ToString()),
                .ExpiryDate = If(record.IsNull("expiry_date"), Nothing, record("expiry_date")),
                .Privacy = If(record.IsNull("privacy"), Nothing, record("privacy").ToString()),
                .DownloadCount = If(record.IsNull("download_count"), Nothing, record("download_count")),
                .Availability = If(record.IsNull("availability"), Nothing, record("availability").ToString()),
                .CreatedAt = If(record.IsNull("created_at"), Nothing, record("created_at")),
                .UpdatedAt = If(record.IsNull("updated_at"), Nothing, record("updated_at"))
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
            Debug.WriteLine($"[FileSharedRepository] GetByUploader error: {_connection.ErrorMessage}")
            Return New List(Of FilesShared)()
        End If

        Dim records = _connection.FetchAll()

        For Each record As DataRow In records
            Dim file As New FilesShared() With {
                .Id = If(record.IsNull("id"), Nothing, record("id")),
                .Name = If(record.IsNull("name"), Nothing, record("name").ToString()),
                .FileName = If(record.IsNull("file_name"), Nothing, record("file_name").ToString()),
                .FileDescription = If(record.IsNull("file_description"), Nothing, record("file_description").ToString()),
                .FilePath = If(record.IsNull("file_path"), Nothing, record("file_path").ToString()),
                .FileSize = If(record.IsNull("file_size"), Nothing, record("file_size").ToString()),
                .FileType = If(record.IsNull("file_type"), Nothing, record("file_type").ToString().Trim()),
                .UploadedBy = If(record.IsNull("uploaded_by"), Nothing, record("uploaded_by")),
                .ShareType = If(record.IsNull("share_type"), Nothing, record("share_type").ToString()),
                .ShareValue = If(record.IsNull("share_value"), Nothing, record("share_value").ToString()),
                .ExpiryDate = If(record.IsNull("expiry_date"), Nothing, record("expiry_date")),
                .Privacy = If(record.IsNull("privacy"), Nothing, record("privacy").ToString()),
                .DownloadCount = If(record.IsNull("download_count"), Nothing, record("download_count")),
                .Availability = If(record.IsNull("availability"), Nothing, record("availability").ToString()),
                .CreatedAt = If(record.IsNull("created_at"), Nothing, record("created_at")),
                .UpdatedAt = If(record.IsNull("updated_at"), Nothing, record("updated_at"))
            }

            filesList.Add(file)
        Next

        Return filesList
    End Function

    Public Function GetByShareType(filesShared As FilesShared) As FilesShared
        _connection.Prepare("SELECT * FROM files_shared WHERE share_type = @share_type AND share_value = @share_value AND uploaded_by <> @uploaded_by  ORDER BY id DESC")
        _connection.AddParam("@share_type", filesShared.ShareType)
        _connection.AddParam("@share_value", filesShared.ShareValue)
        _connection.AddParam("@uploaded_by", filesShared.UploadedBy)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileSharedRepository] GetByShareType error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New FilesShared() With {
                .Id = If(_connection.DataRow.IsNull("id"), Nothing, _connection.DataRow("id")),
                .Name = If(_connection.DataRow.IsNull("name"), Nothing, _connection.DataRow("name").ToString()),
                .FileName = If(_connection.DataRow.IsNull("file_name"), Nothing, _connection.DataRow("file_name").ToString()),
                .FileDescription = If(_connection.DataRow.IsNull("file_description"), Nothing, _connection.DataRow("file_description").ToString()),
                .FilePath = If(_connection.DataRow.IsNull("file_path"), Nothing, _connection.DataRow("file_path").ToString()),
                .FileSize = If(_connection.DataRow.IsNull("file_size"), Nothing, _connection.DataRow("file_size").ToString()),
                .FileType = If(_connection.DataRow.IsNull("file_type"), Nothing, _connection.DataRow("file_type").ToString().Trim()),
                .UploadedBy = If(_connection.DataRow.IsNull("uploaded_by"), Nothing, _connection.DataRow("uploaded_by")),
                .ShareType = If(_connection.DataRow.IsNull("share_type"), Nothing, _connection.DataRow("share_type").ToString()),
                .ShareValue = If(_connection.DataRow.IsNull("share_value"), Nothing, _connection.DataRow("share_value").ToString()),
                .ExpiryDate = If(_connection.DataRow.IsNull("expiry_date"), Nothing, _connection.DataRow("expiry_date")),
                .Privacy = If(_connection.DataRow.IsNull("privacy"), Nothing, _connection.DataRow("privacy").ToString()),
                .DownloadCount = If(_connection.DataRow.IsNull("download_count"), Nothing, _connection.DataRow("download_count")),
                .Availability = If(_connection.DataRow.IsNull("availability"), Nothing, _connection.DataRow("availability").ToString()),
                .CreatedAt = If(_connection.DataRow.IsNull("created_at"), Nothing, _connection.DataRow("created_at")),
                .UpdatedAt = If(_connection.DataRow.IsNull("updated_at"), Nothing, _connection.DataRow("updated_at"))
            }
        End If

        Return Nothing
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
            Debug.WriteLine($"[FileSharedRepository] Read error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        ' Use FetchAll() to get all records
        Dim records = _connection.FetchAll()

        ' Iterate through each record using For Each
        For Each record As DataRow In records
            Dim file As New FilesShared() With {
                .Id = If(record.IsNull("id"), Nothing, record("id")),
                .Name = If(record.IsNull("name"), Nothing, record("name").ToString()),
                .FileName = If(record.IsNull("file_name"), Nothing, record("file_name").ToString()),
                .FileDescription = If(record.IsNull("file_description"), Nothing, record("file_description").ToString()),
                .FilePath = If(record.IsNull("file_path"), Nothing, record("file_path").ToString()),
                .FileSize = If(record.IsNull("file_size"), Nothing, record("file_size").ToString()),
                .FileType = If(record.IsNull("file_type"), Nothing, record("file_type").ToString().Trim()),
                .UploadedBy = If(record.IsNull("uploaded_by"), Nothing, record("uploaded_by")),
                .ShareType = If(record.IsNull("share_type"), Nothing, record("share_type").ToString()),
                .ShareValue = If(record.IsNull("share_value"), Nothing, record("share_value").ToString()),
                .ExpiryDate = If(record.IsNull("expiry_date"), Nothing, record("expiry_date")),
                .Privacy = If(record.IsNull("privacy"), Nothing, record("privacy").ToString()),
                .DownloadCount = If(record.IsNull("download_count"), Nothing, record("download_count")),
                .Availability = If(record.IsNull("availability"), Nothing, record("availability").ToString()),
                .CreatedAt = If(record.IsNull("created_at"), Nothing, record("created_at")),
                .UpdatedAt = If(record.IsNull("updated_at"), Nothing, record("updated_at"))
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
            Debug.WriteLine($"[FileSharedRepository] Insert error: {_connection.ErrorMessage}")
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
            Debug.WriteLine($"[FileSharedRepository] Update error: {_connection.ErrorMessage}")
            Return False
        End If


        If Not _connection.HasRecord Then
            Debug.WriteLine($"[FileSharedRepository] Update Error: File doesn't exist")
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
            Debug.WriteLine($"[FileSharedRepository] Theres an error: {_connection.ErrorMessage}")
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
            Debug.WriteLine($"[FileSharedRepository] Theres an error: {_connection.ErrorMessage}")
            Return False
        End If


        If Not _connection.HasRecord Then
            _connection.ErrorMessage = "File doesn't exists."
            Return True
        End If

        _connection.Prepare("DELETE FROM files_shared WHERE id = @file_id")
        _connection.AddParam("@file_id", filesShared.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileSharedRepository] Theres an error: {_connection.ErrorMessage}")
            Return False
        End If

        If _connection.HasChanges Then
            Return True
        End If

        Return False
    End Function

    Public Function DeleteAll(filesShared As FilesShared) As Boolean
        _connection.Prepare("DELETE FROM files_shared WHERE uploaded_by = @uploaded_by")
        _connection.AddParam("@uploaded_by", filesShared.UploadedBy)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[FileSharedRepository] DeleteAll error: {_connection.ErrorMessage}")
            Return False
        End If

        If _connection.HasChanges Then
            Return True
        End If

        Return False
    End Function
End Class