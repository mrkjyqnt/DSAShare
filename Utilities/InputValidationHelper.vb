Imports System.Text.RegularExpressions
Imports System.Windows
Module InputValidationHelper
    ' Regex patterns for validation
    Private ReadOnly WordPattern As String = "^[A-Za-z]*$"
    Private ReadOnly CodePattern As String = "^\d*(\.\d{0,8})?$"
    Private ReadOnly EmailPattern As String = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
    Private ReadOnly PhonePattern As String = "^\d{10}$" ' Example: 10-digit phone number
    Private ReadOnly UsernamePattern As String = "^[a-z0-9_]*$"

    ' Validate input based on the selected option
    Public Function ValidateInput(input As String, selectedOption As String) As Boolean
        Select Case selectedOption
            Case "Word"
                Return Regex.IsMatch(input, WordPattern)
            Case "Code"
                Return Regex.IsMatch(input, CodePattern)
            Case "Email"
                Return Regex.IsMatch(input, EmailPattern)
            Case "Phone"
                Return Regex.IsMatch(input, PhonePattern)
            Case "Username"
                Return Regex.IsMatch(input, UsernamePattern)
            Case Else
                Return False
        End Select
    End Function

    ' Prevent invalid input from paste operations
    Public Sub HandlePaste(sender As Object, e As DataObjectPastingEventArgs, selectedOption As String)
        If e.DataObject.GetDataPresent(GetType(String)) Then
            Dim pasteText As String = CType(e.DataObject.GetData(GetType(String)), String)
            If Not ValidateInput(pasteText, selectedOption) Then
                e.CancelCommand() 
            End If
        Else
            e.CancelCommand()
        End If
    End Sub
End Module