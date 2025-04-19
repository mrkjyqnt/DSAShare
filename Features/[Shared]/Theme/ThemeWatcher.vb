Imports System
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.Win32
Imports System.Windows
Imports System.Diagnostics

Public Class ThemeWatcher
    Private ReadOnly _themeCheckInterval As TimeSpan = TimeSpan.FromSeconds(5)
    Private _cancellationTokenSource As CancellationTokenSource
    Private _lastDetectedSystemTheme As AppTheme
    
    ' Event to notify theme changes
    Public Event ThemeChanged As EventHandler(Of AppTheme)
    
    ' Start monitoring system theme changes in the background
    Public Sub StartMonitoring()
        _lastDetectedSystemTheme = GetSystemTheme() ' Initialize with current system theme
        _cancellationTokenSource = New CancellationTokenSource()
        Task.Run(Async Function() MonitorSystemThemeAsync(_cancellationTokenSource.Token))
    End Sub
    
    ' Stop the monitoring task
    Public Sub StopMonitoring()
        _cancellationTokenSource?.Cancel()
    End Sub
    
    ' Monitors system theme and triggers event on change
    Private Async Function MonitorSystemThemeAsync(cancellationToken As CancellationToken) As Task
        Try
            While Not cancellationToken.IsCancellationRequested
                Await Task.Delay(_themeCheckInterval, cancellationToken)
                
                ' Always check the system theme regardless of current app theme
                Dim currentSystemTheme = GetSystemTheme()
                
                ' If system theme has changed since last check
                If currentSystemTheme <> _lastDetectedSystemTheme Then
                    _lastDetectedSystemTheme = currentSystemTheme
                    
                    ' Only notify if the application is currently using system theme
                    If ThemeHelper.CurrentTheme = AppTheme.System Then
                        OnThemeChanged(currentSystemTheme)
                        Debug.WriteLine($"System theme changed to {currentSystemTheme}, notifying application")
                    Else
                        Debug.WriteLine($"System theme changed to {currentSystemTheme}, but app is using {ThemeHelper.CurrentTheme} theme, not notifying")
                    End If
                End If
            End While
        Catch ex As Exception
            Debug.WriteLine($"Error in theme monitoring: {ex.Message}")
        End Try
    End Function
    
    ' Trigger the ThemeChanged event
    Protected Sub OnThemeChanged(newTheme As AppTheme)
        RaiseEvent ThemeChanged(Me, newTheme)
    End Sub
    
    ' Get the current system theme
    Public Function GetSystemTheme() As AppTheme
        Try
            Using key = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")
                If key IsNot Nothing Then
                    Dim value = key.GetValue("AppsUseLightTheme")
                    If value IsNot Nothing AndAlso CInt(value) = 0 Then
                        Return AppTheme.Dark
                    Else
                        Return AppTheme.Light
                    End If
                End If
            End Using
        Catch ex As Exception
            Debug.WriteLine($"Error reading system theme: {ex.Message}")
        End Try
        Return AppTheme.Light
    End Function
End Class