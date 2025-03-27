Imports System.Windows
Imports ICSharpCode.AvalonEdit

Public Class TextEditorHelper
    Inherits DependencyObject

    Public Shared ReadOnly BindableTextProperty As DependencyProperty =
        DependencyProperty.RegisterAttached("BindableText", GetType(String), GetType(TextEditorHelper),
            New FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnBindableTextChanged))

    Public Shared Function GetBindableText(ByVal editor As TextEditor) As String
        Return CType(editor.GetValue(BindableTextProperty), String)
    End Function

    Public Shared Sub SetBindableText(ByVal editor As TextEditor, ByVal value As String)
        editor.SetValue(BindableTextProperty, value)
    End Sub

    Private Shared Sub OnBindableTextChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim editor = TryCast(d, TextEditor)
        If editor IsNot Nothing Then
            Dim newText = If(e.NewValue IsNot Nothing, e.NewValue.ToString(), String.Empty)
            If editor.Text <> newText Then
                editor.Text = newText
            End If
        End If
    End Sub
End Class
