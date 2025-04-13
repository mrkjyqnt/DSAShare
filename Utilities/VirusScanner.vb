Imports System.Diagnostics
Imports System.IO

Public Class VirusScanner
    Private Const DefenderPath As String = "C:\Program Files\Windows Defender\MpCmdRun.exe"

    ''' <summary>
    ''' Scans a file using Windows Defender before upload.
    ''' Returns True if clean, False if infected or scan fails.
    ''' </summary>
    Public Shared Function ScanFile(filePath As String) As Boolean
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException("File not found.", filePath)
        End If

        If Not IsWindowsDefenderAvailable() Then
            Throw New InvalidOperationException("Windows Defender not found.")
        End If

        Try
            Dim processStartInfo As New ProcessStartInfo() With {
                .FileName = DefenderPath,
                .Arguments = $"-Scan -ScanType 3 -File ""{filePath}"" -DisableRemediation",
                .RedirectStandardOutput = True,
                .UseShellExecute = False,
                .CreateNoWindow = True
            }

            Using defenderProcess As New Process()
                defenderProcess.StartInfo = processStartInfo
                defenderProcess.Start()
                defenderProcess.WaitForExit()

                ' Exit Code 0 = Clean, 2 = Threats found
                Return (defenderProcess.ExitCode = 0)
            End Using
        Catch ex As Exception
            ' Log error (implement your own logging)
            Debug.WriteLine($"Scan failed: {ex.Message}")
            Return False ' Fail-safe: Assume infected
        End Try
    End Function

    ''' <summary>
    ''' Checks if Windows Defender is available on the system.
    ''' </summary>
    Public Shared Function IsWindowsDefenderAvailable() As Boolean
        Return File.Exists(DefenderPath)
    End Function

    ''' <summary>
    ''' Checks if a file extension is blocked (e.g., .exe, .bat).
    ''' </summary>
    Public Shared Function IsExtensionBlocked(filePath As String) As Boolean
        Dim blockedExtensions As String() = {".exe", ".bat", ".vbs", ".js", ".ps1", ".cmd"}
        Dim fileExt As String = Path.GetExtension(filePath).ToLower()
        Return blockedExtensions.Contains(fileExt)
    End Function
End Class