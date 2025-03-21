Imports Prism.Commands
Imports Prism.Mvvm

Public Class SelectionPopUpViewModel
    Inherits BindableBase
    Implements IPopupViewModel

    Public Event PopupClosed As EventHandler(Of PopupClosedEventArgs) Implements IPopupViewModel.PopupClosed

    Private _inputText As String
    Private _isCodeSelected As Boolean
    Private _isWordSelected As Boolean
    Private _result As PopupResult

    Public ReadOnly Property EnterCommand As DelegateCommand
    Public ReadOnly Property CancelCommand As DelegateCommand

    Public Property InputText As String
        Get
            Return _inputText
        End Get
        Set(value As String)
            If ValidateInput(value, SelectedOption) Then
                SetProperty(_inputText, value)
            End If
        End Set
    End Property

    Public Property IsCodeSelected As Boolean
        Get
            Return _isCodeSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isCodeSelected, value)
            If value Then SelectedOption = "Code"
        End Set
    End Property

    Public Property IsWordSelected As Boolean
        Get
            Return _isWordSelected
        End Get
        Set(value As Boolean)
            SetProperty(_isWordSelected, value)
            If value Then SelectedOption = "Word"
        End Set
    End Property

    Private _selectedOption As String
    Public Property SelectedOption As String
        Get
            Return _selectedOption
        End Get
        Private Set(value As String)
            If value IsNot _selectedOption Then
                SetProperty(_selectedOption, value)
                InputText = ""
            End If
        End Set
    End Property

    Public Sub New()
        IsCodeSelected = True
        _result = New PopupResult

        EnterCommand = New DelegateCommand(AddressOf OnEnter)
        CancelCommand = New DelegateCommand(AddressOf OnCancel)
    End Sub

    Private Sub OnEnter()
        _result.Add("Input", InputText)
        _result.Add("SelectedOption", SelectedOption)

        RaiseEvent PopupClosed(Me, New PopupClosedEventArgs(_result))
    End Sub

    Private Sub OnCancel()
        RaiseEvent PopupClosed(Me, New PopupClosedEventArgs(Nothing))
    End Sub

    Public Sub HandlePaste(sender As Object, e As DataObjectPastingEventArgs)
        InputValidationHelper.HandlePaste(sender, e, SelectedOption)
    End Sub
End Class