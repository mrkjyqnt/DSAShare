Imports System.Text
Imports System.Security.Cryptography

Module Tools
    Public Function ImageSource(image As String) As String
        Return $"pack://application:,,,/Components/Images/{image}"
    End Function

    Public Function IconSource(image As String) As String
        Return $"pack://application:,,,/Components/Images/{image}"
    End Function

    Public  Function HashPassword(password As String) As String
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
End Module
