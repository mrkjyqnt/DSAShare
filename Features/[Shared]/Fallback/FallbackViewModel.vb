Imports CommunityToolkit.Mvvm.Input
Imports Prism.Mvvm

#Disable Warning
Public Class FallbackViewModel
    Inherits BindableBase

    Public ReadOnly Property RetryCommand As IAsyncRelayCommand

    Sub New()
        Loading.Hide()
        RetryCommand = New AsyncRelayCommand(AddressOf OnRetryCommand)
    End Sub

    Private Async Function OnRetryCommand() As Task
        Try
            Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
            Await Task.Run(Sub()
                               ConfigurationModule.GetSettings()
                               Fallback.Retry()
                           End Sub).ConfigureAwait(True)

        Catch ex As Exception
        Finally
            Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Hide())
        End Try
    End Function
End Class