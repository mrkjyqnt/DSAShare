Imports System.Text
Imports System.Security.Cryptography
Imports System.IO
Imports System.Net.NetworkInformation

Module Tools
    Public Function ImageSource(image As String) As String
        Return $"pack://application:,,,/Components/Images/{image}"
    End Function

    Public Function IconSource(image As String) As String
        Return $"pack://application:,,,/Components/Images/{image}"
    End Function

    Public Function GetIcon(iconName As String) As DrawingBrush
        ' If we're already on the UI thread, get the resource directly
        If Application.Current.Dispatcher.CheckAccess() Then
            Return TryCast(Application.Current.Resources(iconName), DrawingBrush)
        Else
            ' Otherwise, invoke on UI thread and wait for result
            Dim result As DrawingBrush = Nothing
            Application.Current.Dispatcher.Invoke(Sub()
                                                      result = TryCast(Application.Current.Resources(iconName), DrawingBrush)
                                                  End Sub)
            Return result
        End If
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

    ''' <summary>
    ''' Get the MAC address of the first active network interface
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Returns "UNKNOWN" if no active network interface is found</remarks>
    Public Function GetMacAddress() As String
        Dim macAddress As String = String.Empty
        For Each nic As NetworkInterface In NetworkInterface.GetAllNetworkInterfaces()
            If nic.OperationalStatus = OperationalStatus.Up AndAlso
            nic.NetworkInterfaceType <> NetworkInterfaceType.Loopback AndAlso
            nic.NetworkInterfaceType <> NetworkInterfaceType.Tunnel Then
                macAddress = nic.GetPhysicalAddress().ToString()
                If Not String.IsNullOrWhiteSpace(macAddress) Then
                    Return macAddress
                End If
            End If
        Next
        Return "UNKNOWN"
    End Function

    ''' <summary>
    ''' Restart the application
    ''' </summary>
    ''' <remarks>Use this method to restart the application from anywhere in the code</remarks>
    Public Sub RestartApplication()
        Process.Start("DSAShare")
        Application.Current.Shutdown()
    End Sub

    ''' <summary>
    ''' Compute the SHA256 hash of a file
    ''' </summary>
    ''' <param name="filePath"></param>
    ''' <returns></returns>
    Public Function ComputeSHA256(filePath As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Using stream As FileStream = File.OpenRead(filePath)
                Dim hash As Byte() = sha256.ComputeHash(stream)
                Return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()
            End Using
        End Using
    End Function

    Public Async Function RunOnUiAsync(action As Action) As Task
        Await Application.Current.Dispatcher.InvokeAsync(action).Task
    End Function

    Public Async Function RunOnUiAsync(Of T)(func As Func(Of T)) As Task(Of T)
        Return Await Application.Current.Dispatcher.InvokeAsync(func).Task
    End Function
End Module
