Imports System.Windows
Class MainWindow
    Private WithEvents _themeWatcher As ThemeWatcher
    
    Public Sub New()
        InitializeComponent()
        ' Initialize ThemeWatcher
        _themeWatcher = New ThemeWatcher()
        _themeWatcher.StartMonitoring()
        
        AddHandler _themeWatcher.ThemeChanged, AddressOf OnThemeChanged
    End Sub
    
    Private Sub OnThemeChanged(sender As Object, newTheme As AppTheme)
        ThemeHelper.ApplyTheme(AppTheme.System)
        Debug.WriteLine($"Applied new system theme: {newTheme}")
    End Sub
    
    ' Clean up when the window is closed
    Protected Overrides Sub OnClosed(e As EventArgs)
        _themeWatcher.StopMonitoring()
        MyBase.OnClosed(e)
    End Sub
End Class