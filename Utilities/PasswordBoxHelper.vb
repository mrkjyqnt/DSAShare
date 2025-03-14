
Public Class PasswordBoxHelper
    ' DependencyProperty for binding Password
    Public Shared ReadOnly PasswordProperty As DependencyProperty =
        DependencyProperty.RegisterAttached(
            "Password", GetType(String), GetType(PasswordBoxHelper),
            New FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnBoundPasswordChanged))

    ' Getter
    Public Shared Function GetPassword(obj As DependencyObject) As String
        Return CStr(obj.GetValue(PasswordProperty))
    End Function

    ' Setter
    Public Shared Sub SetPassword(obj As DependencyObject, value As String)
        obj.SetValue(PasswordProperty, value)
    End Sub

    ' When Password changes in ViewModel, update PasswordBox
    Private Shared Sub OnBoundPasswordChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim passwordBox As PasswordBox = TryCast(d, PasswordBox)
        If passwordBox IsNot Nothing Then
            RemoveHandler passwordBox.PasswordChanged, AddressOf PasswordChanged
            Dim newPassword As String = If(e.NewValue?.ToString(), String.Empty)
            If passwordBox.Password <> newPassword Then
                passwordBox.Password = newPassword
            End If
            AddHandler passwordBox.PasswordChanged, AddressOf PasswordChanged

            ' Update Placeholder visibility
            Dim placeholder As TextBlock = TryCast(passwordBox.Template.FindName("PlaceholderText", passwordBox), TextBlock)
            If placeholder IsNot Nothing Then
                placeholder.Visibility = If(String.IsNullOrEmpty(passwordBox.Password), Visibility.Visible, Visibility.Hidden)
            End If
        End If
    End Sub


    ' When user types, update the PasswordProperty AND Placeholder visibility
    Private Shared Sub PasswordChanged(sender As Object, e As RoutedEventArgs)
        Dim passwordBox As PasswordBox = TryCast(sender, PasswordBox)
        If passwordBox IsNot Nothing Then
            ' Update the Password property
            SetPassword(passwordBox, passwordBox.Password)

            ' Update Placeholder visibility
            Dim placeholder As TextBlock = TryCast(passwordBox.Template.FindName("PlaceholderText", passwordBox), TextBlock)
            If placeholder IsNot Nothing Then
                placeholder.Visibility = If(String.IsNullOrEmpty(passwordBox.Password), Visibility.Visible, Visibility.Hidden)
            End If
        End If
    End Sub
End Class
