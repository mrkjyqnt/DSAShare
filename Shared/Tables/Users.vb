''' <summary>
''' Represents the Users table.
''' </summary>
Public Class Users
    Public Property Id As Integer? = Nothing
    Public Property Name As String
    Public Property Username As String
    Public Property PasswordHash As String
    Public Property SecurityQuestion1 As String
    Public Property SecurityQuestion2 As String
    Public Property SecurityQuestion3 As String
    Public Property SecurityAnswer1 As String
    Public Property SecurityAnswer2 As String
    Public Property SecurityAnswer3 As String
    Public Property Role As String
    Public Property Status As String
    Public Property AppAppearance As String
    Public Property CreatedAt As DateTime? = Nothing

End Class