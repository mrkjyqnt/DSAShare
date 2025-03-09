Imports System
Imports System.Collections.Generic

''' <summary>
''' Represents the FilesAccessed table.
''' </summary>
Public Class FilesAccessed
    Public Property Id As Integer
    Public Property UserId As Integer
    Public Property FileId As Integer
    Public Property AccessType As String
    Public Property AccessedAt As DateTime

    ''' <summary>
    ''' Validates the table data before saving to the database.
    ''' </summary>
    ''' <returns>True if the data is valid; otherwise, False.</returns>
    Public Function Validate() As Boolean
        ' Validate required fields
        Return UserId > 0 AndAlso FileId > 0 AndAlso Not String.IsNullOrEmpty(AccessType)
    End Function

    ''' <summary>
    ''' Converts the table data to a dictionary for database operations.
    ''' </summary>
    ''' <returns>A dictionary of column names and values.</returns>
    Public Function ToDictionary() As Dictionary(Of String, Object)
        Return New Dictionary(Of String, Object) From {
            {"user_id", UserId},
            {"file_id", FileId},
            {"access_type", AccessType},
            {"accessed_at", AccessedAt}
        }
    End Function
End Class