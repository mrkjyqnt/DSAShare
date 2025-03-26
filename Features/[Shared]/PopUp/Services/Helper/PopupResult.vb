Imports System.Dynamic

Public Class PopupResult
    Inherits DynamicObject

    Private ReadOnly _properties As New Dictionary(Of String, Object)

    Public Sub Add(name As String, value As Object)
        _properties(name) = value
    End Sub

    Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
        Dim name = binder.Name
        If _properties.ContainsKey(name) Then
            result = _properties(name)
            Return True
        Else
            result = Nothing
            Return False
        End If
    End Function

    Public Overrides Function TrySetMember(binder As SetMemberBinder, value As Object) As Boolean
        _properties(binder.Name) = value
        Return True
    End Function
End Class