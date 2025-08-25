Public Class ReportsService
    Implements IReportsService

    Private ReadOnly _repository As ReportsRepository

    Public Sub New(repository As ReportsRepository)
        _repository = repository
    End Sub

    ''' <summary>
    ''' Get all reports
    ''' </summary>
    ''' <returns></returns>
    Public Function GetReports() As List(Of Reports) Implements IReportsService.GetReports
        Return _repository.Read()
    End Function

    ''' <summary>
    ''' Get report by id
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Function GetReportById(id As Integer) As Reports Implements IReportsService.GetReportById
        Dim reports = _repository.Read()
        Return reports?.FirstOrDefault(Function(r) r.Id = id)
    End Function

    ''' <summary>
    ''' Get all reports by file id
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Function GetReportsByFileId(id As Integer) As List(Of Reports) Implements IReportsService.GetReportsByFileId
        Dim reports = _repository.Read()
        Return reports?.Where(Function(r) r.FileId = id).ToList()
    End Function

    ''' <summary>
    ''' Get report by id for user
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Function GetReportByUserWithFileId(report As Reports) As Reports Implements IReportsService.GetReportByIdByUser
        Dim reports = _repository.Read()
        Return reports?.FirstOrDefault(Function(r) r.FileId = report.FileId And r.ReporterId = report.ReporterId)
    End Function

    Public Function GetReportsByUserId(userId As Integer) As List(Of Reports) Implements IReportsService.GetReportsByUserId
        Dim reports = _repository.Read()
        Return reports?.Where(Function(r) r.ReporterId = userId).ToList()
    End Function

    ''' <summary>
    ''' Get report by user id
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    Public Function CreateReport(report As Reports) As Boolean Implements IReportsService.CreateReport
        Return _repository.Insert(report)
    End Function

    ''' <summary>
    ''' Update report
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function UpdateReport(report As Reports) As Boolean Implements IReportsService.UpdateReport
        Return _repository.Update(report)
    End Function

    ''' <summary>
    ''' Delete report
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DeleteReport(id As Integer) As Boolean Implements IReportsService.DeleteReport
        Return _repository.Delete(New Reports With {.Id = id})
    End Function
End Class
