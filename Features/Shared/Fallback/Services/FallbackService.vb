Public Class FallbackService
    Implements IFallbackService

    Private ReadOnly _connection As Connection

    Sub New(connection As Connection)
        _connection = connection
    End Sub

    Public Function Retry() As Boolean Implements IFallbackService.Retry
        Return _connection.TestConnection
    End Function
End Class
