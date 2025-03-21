Imports System.Text
Imports System.Security.Cryptography

Module Tools
    Public Function ImageSource(image As String) As String
        Return $"pack://application:,,,/Components/Images/{image}"
    End Function

    Public Function IconSource(image As String) As String
        Return $"pack://application:,,,/Components/Images/{image}"
    End Function

    Public Function HashPassword(password As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim passwordBytes As Byte() = Encoding.UTF8.GetBytes(password)
            Dim hashBytes As Byte() = sha256.ComputeHash(passwordBytes)
            Dim stringBuilder As New StringBuilder()
            For Each b As Byte In hashBytes
                stringBuilder.Append(b.ToString("x2"))
            Next

                    Return stringBuilder.ToString()
            End Using
      End Function

    Public Sub Dispatch(action As Action)
        Application.Current.Dispatcher.Invoke(action)
    End Sub

    Public Function FormatFileSize(bytes As Long) As String
        Dim sizes As String() = {"B", "KB", "MB", "GB", "TB"}
        Dim len As Double = bytes
        Dim order As Integer = 0

        While len >= 1024 AndAlso order < sizes.Length - 1
            order += 1
            len /= 1024
        End While

        Return String.Format("{0:0.##} {1}", len, sizes(order))
    End Function
End Module
