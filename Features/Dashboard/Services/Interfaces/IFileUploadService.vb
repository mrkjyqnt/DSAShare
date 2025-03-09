Public Interface IFileUploadService
    Function UploadFile(filePath As String, uploadedBy As Integer, shareType As String, Optional shareCode As String = Nothing, Optional expiryDate As DateTime? = Nothing, Optional privacy As String = "Public") As Boolean
End Interface