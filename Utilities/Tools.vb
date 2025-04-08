Imports System.Text
Imports System.Security.Cryptography
Imports System.IO

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

    ''' <summary>
    ''' Check if a list is null or empty
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <returns></returns>
    Public Function IsNullOrEmpty(Of T)(list As IEnumerable(Of T)) As Boolean
        Return list Is Nothing OrElse Not list.Any()
    End Function

    ''' <summary>
    ''' Check if a string is null or empty
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Public Function IsNullOrEmpty(value As String) As Boolean
        Return String.IsNullOrEmpty(value)
    End Function

    Public Function GetUniqueFilePath(folderPath As String, fileName As String) As String
        Dim baseName As String = Path.GetFileNameWithoutExtension(fileName)
        Dim extension As String = Path.GetExtension(fileName)
        Dim newPath As String = Path.Combine(folderPath, fileName)

        ' If file doesn't exist, return original path
        If Not File.Exists(newPath) Then
            Return newPath
        End If

        ' Find available numerical suffix
        Dim counter As Integer = 1
        Do While File.Exists(newPath)
            newPath = Path.Combine(folderPath, $"{baseName} ({counter}){extension}")
            counter += 1
        Loop

        Return newPath
    End Function

    Public Async Function ShowLoading() As Task
        Await Application.Current.Dispatcher.InvokeAsync(Sub() Loading.Show())
        Await Task.Delay(100).ConfigureAwait(True)
    End Function

    Public Async Function ValidateConnection() As Task(Of Boolean)
        If Not Await Fallback.CheckConnection() Then Return False
        Return True
    End Function


End Module
