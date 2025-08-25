Public Interface IFileInfoService
    Property FilePath As String
    Property Name As String
    Property Size As String
    Property Type As String
    Sub Extract(filePath As String)
End Interface
