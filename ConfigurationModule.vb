Imports System.IO

Module ConfigurationModule
    Private ReadOnly ProjectDirectory As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..")
    Private ReadOnly ConfigPath As String = Path.Combine(ProjectDirectory, "config.ini") ' Change for XML or JSON

    Private Function ReadIniValue(section As String, key As String) As String
        Dim lines() As String = File.ReadAllLines(ConfigPath)
        Dim currentSection As String = ""
        For Each line In lines
            line = line.Trim()
            If line.StartsWith("[") And line.EndsWith("]") Then
                currentSection = line.Trim("["c, "]"c)
            ElseIf currentSection = section AndAlso line.Contains("=") Then
                Dim parts() As String = line.Split("="c, 2)
                If parts(0).Trim() = key Then
                    Return parts(1).Trim()
                End If
            End If
        Next
        Return String.Empty
    End Function

    Public ReadOnly Property FolderPath As String
        Get
            Return ReadIniValue("Network", "FOLDER_PATH")
        End Get
    End Property

    Public ReadOnly Property NetworkCredential As Net.NetworkCredential
        Get
            Dim username As String = ReadIniValue("Network", "NET_USER")
            Dim password As String = ReadIniValue("Network", "NET_PASSWORD")
            Return New Net.NetworkCredential(username, password)
        End Get
    End Property

    Public ReadOnly Property DBConnectionString As String
        Get
            Dim dbServer As String = ReadIniValue("Database", "DB_SERVER")
            Dim dbName As String = ReadIniValue("Database", "DB_NAME")
            Dim dbUser As String = ReadIniValue("Database", "DB_USER")
            Dim dbPassword As String = ReadIniValue("Database", "DB_PASSWORD")

            If String.IsNullOrEmpty(dbServer) OrElse String.IsNullOrEmpty(dbName) OrElse
               String.IsNullOrEmpty(dbUser) OrElse String.IsNullOrEmpty(dbPassword) Then
                Throw New InvalidOperationException("Missing database configuration in INI file.")
            End If

            Return $"Data Source={dbServer};Initial Catalog={dbName};User ID={dbUser};Password={dbPassword};Trust Server Certificate=True"
        End Get
    End Property
End Module
