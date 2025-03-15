Public Class PopupClosedEventArgs
    Inherits EventArgs

    Public Property Result As Object

    Public Sub New(result As Object)
        Me.Result = result
    End Sub
End Class