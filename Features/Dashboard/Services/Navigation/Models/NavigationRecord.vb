Imports Prism.Navigation

Public Class NavigationRecord
    Public Property Region As String
    Public Property View As String
    Public Property Item As String
    Public Property Parameters As NavigationParameters
    
    Public Sub New(region As String, view As String, item As String, parameters As NavigationParameters)
        Me.Region = region
        Me.View = view
        Me.Item = item
        Me.Parameters = If(parameters, New NavigationParameters())
    End Sub
End Class