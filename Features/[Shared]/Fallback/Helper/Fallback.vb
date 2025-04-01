Imports Prism.Navigation.Regions
Imports Prism.Ioc

#Disable Warning
Public Class Fallback
    Private Shared ReadOnly _fallbackService As IFallbackService
    Private Shared _connection As Connection
    Private Shared _tcs As TaskCompletionSource(Of Boolean)
    Private Shared _isChecking As Boolean

    Shared Sub New()
        Dim regionManager As IRegionManager = ContainerLocator.Container.Resolve(Of IRegionManager)()
        _fallbackService = New FallbackService(regionManager)
        _connection = New Connection()
    End Sub

    Public Shared Async Function CheckConnection() As Task(Of Boolean)
        Try
            ' First immediate check
            If Await TestConnection() Then Return True

            ' Show fallback and wait for retry
            Show()
            _tcs = New TaskCompletionSource(Of Boolean)()
            Return Await _tcs.Task
        Finally
            Hide()
        End Try
    End Function

    Private Shared Async Function TestConnection() As Task(Of Boolean)
        Try
            Return Await Task.Run(Function() _connection.TestConnection())
        Catch
            Return False
        End Try
    End Function

    Public Shared Async Sub Retry()
        If _isChecking Then Return
        _isChecking = True

        Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())

        Try
            If Await TestConnection() Then
                _tcs?.TrySetResult(True)
            End If
        Finally
            _isChecking = False
            Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Hide())
        End Try
    End Sub

    Private Shared Sub Show()
        Application.Current.Dispatcher.Invoke(Sub()
                                                  _fallbackService.Show(New FallbackView)
                                              End Sub)
    End Sub

    Private Shared Sub Hide()
        Application.Current.Dispatcher.Invoke(Sub()
                                                  _fallbackService.Hide()
                                              End Sub)
    End Sub
End Class