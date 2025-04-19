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
            Await Application.Current.Dispatcher.InvokeAsync(Sub()
                                                                 Loading.Show()
                                                             End Sub)
            Await Task.Run(Sub()
                               ConfigurationModule.GetSettings()
                               Fallback.Retry()
                           End Sub).ConfigureAwait(True)

            Loading.Hide()
        Catch ex As Exception
        Finally
        End Try
    End Function
End Class