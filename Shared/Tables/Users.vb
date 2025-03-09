Imports System
Imports System.Collections.Generic

Public Class Users
    Public Property Id As Integer
    Public Property Username As String
    Public Property PasswordHash As String
    Public Property Role As String
    Public Property Status As String
    Public Property CreatedAt As DateTime

    ''' <summary>
    ''' Validates the table data before saving to the database.
    ''' </summary>
    ''' <returns>True if the data is valid; otherwise, False.</returns>
    Public Function Validate() As Boolean
        ' Validate required fields
        Return Not String.IsNullOrEmpty(Username) AndAlso
               Not String.IsNullOrEmpty(PasswordHash) AndAlso
               Not String.IsNullOrEmpty(Role) AndAlso
               Not String.IsNullOrEmpty(Status)
    End Function

    ''' <summary>
    ''' Converts the table data to a dictionary for database operations.
    ''' </summary>
    ''' <returns>A dictionary of column names and values.</returns>
    Public Function ToDictionary() As Dictionary(Of String, Object)
        Return New Dictionary(Of String, Object) From {
            {"username", Username},
            {"password_hash", PasswordHash},
            {"role", Role},
            {"status", Status},
            {"created_at", CreatedAt}
        }
    End Function
End Class