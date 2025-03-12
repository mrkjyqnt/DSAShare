Module SourceHelper
    Public Function ImageSource(image As String) As String
        Return $"pack://application:,,,/Components/Images/{image}"
    End Function

    Public Function IconSource(image As String) As String
        Return $"pack://application:,,,/Components/Images/{image}"
    End Function


End Module
