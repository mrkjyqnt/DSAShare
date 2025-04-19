Imports System
Imports System.Windows
Imports System.Windows.Media
Imports Microsoft.Win32

Public Enum AppTheme
    Light
    Dark
    System
End Enum

Module ThemeHelper

    Private ReadOnly ThemePaths As New Dictionary(Of AppTheme, String) From {
        {AppTheme.Light, "/Components/Theme/LightTheme.xaml"},
        {AppTheme.Dark, "/Components/Theme/DarkTheme.xaml"}
    }

    Private ReadOnly IconPaths As New Dictionary(Of AppTheme, String) From {
        {AppTheme.Light, "/Components/Theme/LightIcons.xaml"},
        {AppTheme.Dark, "/Components/Theme/DarkIcons.xaml"}
    }

    ' Track the current theme
    Private _currentTheme As AppTheme = AppTheme.System ' Default is system-based

    Public Property CurrentTheme As AppTheme
        Get
            Return _currentTheme
        End Get
        Private Set(value As AppTheme)
            _currentTheme = value
        End Set
    End Property

    ''' <summary>
    ''' Applies the specified theme and corresponding icon set to the application.
    ''' </summary>
    ''' <param name="theme"></param>
    Public Sub ApplyTheme(theme As AppTheme)
        ' Always store the user preference
        CurrentTheme = theme

        ' Determine actual theme to apply visually
        Dim visualTheme As AppTheme = If(theme = AppTheme.System, GetSystemTheme(), theme)

        Application.Current.Dispatcher.Invoke(Sub()
                                                  ' Preserve non-theme dictionaries
                                                  Dim existingDictionaries = Application.Current.Resources.MergedDictionaries.ToList()
                                                  Application.Current.Resources.MergedDictionaries.Clear()

                                                  For Each dictionary In existingDictionaries
                                                      If dictionary.Source IsNot Nothing AndAlso
                                                         (dictionary.Source.OriginalString.EndsWith("LightTheme.xaml") OrElse
                                                          dictionary.Source.OriginalString.EndsWith("DarkTheme.xaml") OrElse
                                                          dictionary.Source.OriginalString.EndsWith("LightIcons.xaml") OrElse
                                                          dictionary.Source.OriginalString.EndsWith("DarkIcons.xaml")) Then
                                                          Continue For
                                                      End If
                                                      Application.Current.Resources.MergedDictionaries.Add(dictionary)
                                                  Next

                                                  ' Apply the actual theme resources (based on system or selected)
                                                  Dim themeUri = New Uri(ThemePaths(visualTheme), UriKind.Relative)
                                                  Dim iconUri = New Uri(IconPaths(visualTheme), UriKind.Relative)

                                                  Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary With {.Source = themeUri})
                                                  Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary With {.Source = iconUri})

                                                  ' Adjust brush based on applied theme
                                                  If visualTheme = AppTheme.Light Then
                                                      Application.Current.Resources("BlackBrush") = New SolidColorBrush(Color.FromRgb(61, 61, 58))
                                                      Debug.WriteLine("Applied Light theme BlackBrush color.")
                                                  Else
                                                      Application.Current.Resources("BlackBrush") = New SolidColorBrush(Color.FromRgb(243, 242, 238))
                                                      Debug.WriteLine("Applied Dark theme BlackBrush color.")
                                                  End If
                                              End Sub)
    End Sub




    Public Function GetThemeFromString(themeName As String) As AppTheme
        Select Case themeName.ToLower()
            Case "light"
                Return AppTheme.Light
            Case "dark"
                Return AppTheme.Dark
            Case "system"
                Return AppTheme.System
            Case Else
                Return AppTheme.Light
        End Select
    End Function

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

    Public Sub WatchForSystemThemeChanges()
        If CurrentTheme = AppTheme.System Then
            Dim newTheme = GetSystemTheme()
            Static lastVisualTheme As AppTheme = newTheme

            If newTheme <> lastVisualTheme Then
                ApplyTheme(AppTheme.System)
                lastVisualTheme = newTheme
                Debug.WriteLine("Detected system theme change.")
            End If
        End If
    End Sub

End Module
