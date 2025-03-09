Public Class ErrorHandler
    Private Shared _hasError As Boolean = False
    Private Shared _errorMessage As String = String.Empty

    ''' <summary>
    ''' Indicates whether an error has occurred.
    ''' </summary>
    Public Shared ReadOnly Property HasError As Boolean
        Get
            Return _hasError
        End Get
    End Property

    ''' <summary>
    ''' Gets the error message.
    ''' </summary>
    Public Shared ReadOnly Property ErrorMessage As String
        Get
            Return _errorMessage
        End Get
    End Property

    ''' <summary>
    ''' Sets the error state and message.
    ''' </summary>
    ''' <param name="message">The error message to set.</param>
    Public Shared Sub SetError(message As String)
        _hasError = True
        _errorMessage = message
    End Sub

    ''' <summary>
    ''' Clears the error state and message.
    ''' </summary>
    Public Shared Sub ClearError()
        _hasError = False
        _errorMessage = String.Empty
    End Sub
End Class