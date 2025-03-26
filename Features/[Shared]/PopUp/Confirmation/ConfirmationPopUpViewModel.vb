Imports Prism.Commands
Imports Prism.Mvvm

Public Class ConfirmationPopUpViewModel
       Inherits BindableBase
    Implements IPopupViewModel

    Public Event PopupClosed As EventHandler(Of PopupClosedEventArgs) Implements IPopupViewModel.PopupClosed

    Private _inputText As String
    Private _result As PopupResult

    Public ReadOnly Property EnterCommand As DelegateCommand
    Public ReadOnly Property CancelCommand As DelegateCommand

    Public Property InputText As String
        Get
            Return _inputText
        End Get
        Set(value As String)
            SetProperty(_inputText, value)
        End Set
    End Property

    Public Sub New()
        _result = New PopupResult

        EnterCommand = New DelegateCommand(AddressOf OnEnter)
        CancelCommand = New DelegateCommand(AddressOf OnCancel)
    End Sub

    Private Sub OnEnter()
        _result.Add("Input", InputText)

        RaiseEvent PopupClosed(Me, New PopupClosedEventArgs(_result))
    End Sub

    Private Sub OnCancel()
        RaiseEvent PopupClosed(Me, New PopupClosedEventArgs(Nothing))
    End Sub
End Class
