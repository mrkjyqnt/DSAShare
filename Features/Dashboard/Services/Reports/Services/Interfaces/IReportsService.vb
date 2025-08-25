Public Interface IReportsService

    ' <summary>
    ''' Get all reports
    ''' </summary>
    ''' <returns></returns>
    Function GetReports() As List(Of Reports)

    ''' <summary>
    ''' Get report by id
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Function GetReportById(id As Integer) As Reports

    ''' <summary>
    ''' Get report by id for user
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Function CreateReport(report As Reports) As Boolean

    ''' <summary>
    ''' Get report by user id
    ''' </summary>
    ''' <param name="report"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function UpdateReport(report As Reports) As Boolean

    ''' <summary>
    ''' Delete report
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function DeleteReport(id As Integer) As Boolean

    ''' <summary>
    ''' Get report by id for user
    ''' </summary>
    ''' <param name="fileId"></param>
    ''' <param name="userId"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function GetReportByIdByUser(report As Reports) As Reports
    Function GetReportsByFileId(id As Integer) As List(Of Reports)
    Function GetReportsByUserId(userId As Integer) As List(Of Reports)
End Interface
