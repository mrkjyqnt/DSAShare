'Imports Prism.Commands
'Imports Prism.Mvvm
'Imports Prism.Dialogs

'Public Class ConfirmationViewModel
'    Inherits BindableBase
'    Implements IDialogAware

'    ' Properties
'    Private _password As String
'    Public Property Password As String
'        Get
'            Return _password
'        End Get
'        Set(value As String)
'            SetProperty(_password, value)
'            EnterCommand.RaiseCanExecuteChanged() ' Refresh command state
'        End Set
'    End Property

'    Private _title As String
'    Public Property Title As String Implements IDialogAware.Title
'        Get
'            Return _title
'        End Get
'        Set(value As String)
'            SetProperty(_title, value)
'        End Set
'    End Property

'    ' Commands
'    Public ReadOnly Property EnterCommand As DelegateCommand
'    Public ReadOnly Property CancelCommand As DelegateCommand

'    ' Constructor
'    Public Sub New()
'        EnterCommand = New DelegateCommand(AddressOf OnEnter, AddressOf CanEnter)
'        CancelCommand = New DelegateCommand(AddressOf OnCancel)
'    End Sub

'    ' Command Methods
'    Private Sub OnEnter()
'        ' Handle confirmation logic
'        Dim parameters = New DialogParameters()
'        parameters.Add("password", Password)
'        RequestClose.Invoke(New DialogResult(ButtonResult.OK, parameters))
'    End Sub

'    Private Function CanEnter() As Boolean
'        Return Not String.IsNullOrEmpty(Password)
'    End Function

'    Private Sub OnCancel()
'        ' Handle cancellation logic
'        RequestClose.Invoke(New DialogResult(ButtonResult.Cancel))
'    End Sub

'    ' IDialogAware Implementation
'    Public Event RequestClose As Action(Of IDialogResult) Implements IDialogAware.RequestClose

'    Public Function CanCloseDialog() As Boolean Implements IDialogAware.CanCloseDialog
'        Return True ' Allow the dialog to close
'    End Function

'    Public Sub OnDialogClosed() Implements IDialogAware.OnDialogClosed
'        ' Cleanup if needed
'    End Sub

'    Public Sub OnDialogOpened(parameters As IDialogParameters) Implements IDialogAware.OnDialogOpened
'        ' Set parameters (e.g., title)
'        If parameters IsNot Nothing AndAlso parameters.ContainsKey("title") Then
'            Title = parameters.GetValue(Of String)("title")
'        End If
'    End Sub
'End Class