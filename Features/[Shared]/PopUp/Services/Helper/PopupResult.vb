Imports System.Dynamic

Public Class PopupResult
    Inherits DynamicObject
    Private ReadOnly _properties As New Dictionary(Of String, Object)

    Public Sub Add(name As String, value As Object)
        _properties(name) = value
    End Sub

    ' Allows dynamic property access (e.g., result.Input)
    Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
        Return _properties.TryGetValue(binder.Name, result)
    End Function

    ' Still provides GetValue for generic access
    Public Function GetValue(Of T)(propertyName As String) As T
        If _properties.ContainsKey(propertyName) Then
            Return CType(_properties(propertyName), T)
        End If
        Return Nothing
    End Function
End Class