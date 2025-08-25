Imports System.Data

Public Class ReportsRepository
    Private ReadOnly _connection As Connection

    Public Sub New(connection As Connection)
        _connection = connection
    End Sub

    Public Function Read() As List(Of Reports)
        Dim reportsList As New List(Of Reports)()

        _connection.Prepare("SELECT * FROM reports ORDER BY id DESC")
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[ReportsRepository] Read error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        Dim records = _connection.FetchAll()

        For Each record As DataRow In records
            Dim file As New Reports() With {
                .Id = If(record.IsNull("id"), Nothing, record("id")),
                .FileId = If(record.IsNull("file_id"), Nothing, record("file_id")),
                .ReporterId = If(record.IsNull("reporter_id"), Nothing, record("reporter_id").ToString()),
                .ReporterDescription = If(record.IsNull("reporter_description"), Nothing, record("reporter_description").ToString()),
                .AdminId = If(record.IsNull("admin_id"), Nothing, record("admin_id").ToString()),
                .AdminDescription = If(record.IsNull("admin_description"), Nothing, record("admin_description").ToString()),
                .Status = If(record.IsNull("status"), Nothing, record("status").ToString()),
                .ReportedAt = If(record.IsNull("reported_at"), Nothing, record("reported_at"))
            }

            reportsList.Add(file)
        Next

        Return reportsList
    End Function

    ''' <summary>
    ''' Inserts a new report record into the database.
    ''' </summary>
    ''' <param name="report">The Reports object containing report data.</param>
    Public Function Insert(report As Reports) As Boolean
        _connection.Prepare("INSERT INTO reports (file_id, reporter_id, reporter_description, admin_id, admin_description, status, reported_at) " &
                           "VALUES (@file_id, @reporter_id, @reporter_description, @admin_id, @admin_description, @status, @reported_at)")
        
        _connection.AddParam("@file_id", report.FileId)
        _connection.AddParam("@reporter_id", report.ReporterId)
        _connection.AddParam("@reporter_description", report.ReporterDescription)
        _connection.AddParam("@admin_id", report.AdminId)
        _connection.AddParam("@admin_description", report.AdminDescription)
        _connection.AddParam("@status", report.Status)
        _connection.AddParam("@reported_at", report.ReportedAt)
        
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[ReportsRepository] Insert error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasChanges
    End Function

    ''' <summary>
    ''' Updates an existing report record in the database.
    ''' </summary>
    ''' <param name="report">The Reports object containing updated data.</param>
    Public Function Update(report As Reports) As Boolean
        _connection.Prepare("SELECT * FROM reports WHERE id = @id")
        _connection.AddParam("@id", report.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[ReportsRepository] Update error: {_connection.ErrorMessage}")
            Return False
        End If

        If Not _connection.HasRecord Then
            Debug.WriteLine($"[ReportsRepository] Update Error: Report doesn't exist")
            Return False
        End If

        _connection.Prepare("UPDATE reports SET " &
                           "file_id = @file_id, " &
                           "reporter_id = @reporter_id, " &
                           "reporter_description = @reporter_description, " &
                           "admin_id = @admin_id, " &
                           "admin_description = @admin_description, " &
                           "status = @status, " &
                           "reported_at = @reported_at " &
                           "WHERE id = @id")
        
        _connection.AddParam("@file_id", report.FileId)
        _connection.AddParam("@reporter_id", report.ReporterId)
        _connection.AddParam("@reporter_description", report.ReporterDescription)
        _connection.AddParam("@admin_id", report.AdminId)
        _connection.AddParam("@admin_description", report.AdminDescription)
        _connection.AddParam("@status", report.Status)
        _connection.AddParam("@reported_at", report.ReportedAt)
        _connection.AddParam("@id", report.Id)
        
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[ReportsRepository] Update error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasChanges
    End Function

    ''' <summary>
    ''' Deletes a report record by its ID.
    ''' </summary>
    ''' <param name="report"> The Reports object containing the ID of the report to delete.</param>
    ''' <returns>True if the record was deleted successfully, otherwise false.</returns>
    ''' <remarks>It is assumed that the ID is a valid integer.</remarks>
    Public Function Delete(report As Reports) As Boolean
        _connection.Prepare("SELECT * FROM reports WHERE id = @id")
        _connection.AddParam("@id", report.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[ReportsRepository] Delete error: {_connection.ErrorMessage}")
            Return False
        End If

        If Not _connection.HasRecord Then
            Debug.WriteLine($"[ReportsRepository] Delete Error: Report doesn't exist")
            Return False
        End If

        _connection.Prepare("DELETE FROM reports WHERE id = @id")
        _connection.AddParam("@id", report.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[ReportsRepository] Delete error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasChanges
    End Function
End Class
