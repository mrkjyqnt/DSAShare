Imports System
Imports System.Collections.Generic

''' <summary>
''' Represents the FilesShared table.
''' </summary>
Public Class FilesShared
    Public Property Id As Integer? = Nothing
    Public Property Name As String
    Public Property FileName As String
    Public Property FileDescription As String
    Public Property FilePath As String
    Public Property FileSize As String
    Public Property FileType As String
    Public Property UploadedBy As Integer? = Nothing
    Public Property ShareType As String
    Public Property ShareValue As String
    Public Property ExpiryDate As DateTime? = Nothing
    Public Property Privacy As String
    Public Property DownloadCount As Integer? = Nothing
    Public Property Availability As String
    Public Property CreatedAt As DateTime? = Nothing
    Public Property UpdatedAt As DateTime? = Nothing

    ''' <summary>
    ''' Validates the table data before saving to the database.
    ''' </summary>
    ''' <returns>True if the data is valid; otherwise, False.</returns>
    Public Function Validate() As Boolean
        ' Validate required fields
        Return Not String.IsNullOrEmpty(FileName) AndAlso
               Not String.IsNullOrEmpty(FilePath) AndAlso
               UploadedBy > 0 AndAlso
               Not String.IsNullOrEmpty(ShareType) AndAlso
               Not String.IsNullOrEmpty(Privacy)
    End Function

    ''' <summary>
    ''' Converts the table data to a dictionary for database operations.
    ''' </summary>
    ''' <returns>A dictionary of column names and values.</returns>
    Public Function ToDictionary() As Dictionary(Of String, Object)
        Return New Dictionary(Of String, Object) From {
            {"file_name", FileName},
            {"file_path", FilePath},
            {"uploaded_by", UploadedBy},
            {"share_type", ShareType},
            {"share_code", ShareValue},
            {"expiry_date", ExpiryDate},
            {"privacy", Privacy},
            {"download_count", DownloadCount},
            {"created_at", CreatedAt}
        }
    End Function
End Class