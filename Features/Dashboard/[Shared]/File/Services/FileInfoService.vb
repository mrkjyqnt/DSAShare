Public Class FileInfoService
    Implements IFileInfoService

    Public Property FilePath As String Implements IFileInfoService.FilePath
    Public Property Name As String Implements IFileInfoService.Name
    Public Property Size As String Implements IFileInfoService.Size
    Public Property Type As String Implements IFileInfoService.Type

    Public Sub Extract(filePath As String) Implements IFileInfoService.Extract
        If String.IsNullOrEmpty(filePath) Then Exit Sub

        Dim fileInfo As New System.IO.FileInfo(filePath)
        Me.FilePath = filePath
        Me.Name = System.IO.Path.GetFileNameWithoutExtension(filePath) 
        Me.Size = FormatFileSize(fileInfo.Length)
        Me.Type = fileInfo.Extension
    End Sub
End Class