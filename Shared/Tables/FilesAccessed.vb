Imports System
Imports System.Collections.Generic

''' <summary>
''' Represents the FilesAccessed table.
''' </summary>
Public Class FilesAccessed
    Public Property Id As Integer? = Nothing
    Public Property UserId As Integer? = Nothing
    Public Property FileId As Integer? = Nothing
    Public Property AccessedAt As DateTime

End Class