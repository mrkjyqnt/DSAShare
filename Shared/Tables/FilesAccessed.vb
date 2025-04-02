Imports System
Imports System.Collections.Generic

''' <summary>
''' Represents the FilesAccessed table.
''' </summary>
Public Class FilesAccessed
    Public Property Id As Integer
    Public Property UserId As Integer
    Public Property FileId As Integer
    Public Property AccessedAt As DateTime

    ''' <summary>
    ''' Converts the table data to a dictionary for database operations.
    ''' </summary>
    ''' <returns>A dictionary of column names and values.</returns>
    Public Function ToDictionary() As Dictionary(Of String, Object)
        Return New Dictionary(Of String, Object) From {
            {"user_id", UserId},
            {"file_id", FileId},
            {"accessed_at", AccessedAt}
        }
    End Function
End Class