Imports Prism.Commands
Imports Prism.Mvvm

Public Class InformationPopUpViewModel
    Inherits BindableBase
    Implements IPopupViewModel

    Public Event PopupClosed As EventHandler(Of PopupClosedEventArgs) Implements IPopupViewModel.PopupClosed

    Private _titleText As String
    Private _informationText As String

    Public ReadOnly Property CloseCommand As DelegateCommand

    Public Property TitleText As String
        Get
            Return _titleText
        End Get
        Set(value As String)
            SetProperty(_titleText, value)
        End Set
    End Property

    Public Property InformationText As String
        Get
            Return _informationText
        End Get
        Set(value As String)
            SetProperty(_informationText, value)
        End Set
    End Property

    Public Sub New()
        CloseCommand = New DelegateCommand(AddressOf OnClose)
    End Sub

    Public Sub New(Title As String, Information As String)
        Me.New()
        Me.TitleText = Title
        Me.InformationText = Information
    End Sub

    Private Sub OnClose()
       RaiseEvent PopupClosed(Me, New PopupClosedEventArgs(Nothing))
    End Sub
End Class