Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Class SignUpViewModel
    Inherits BindableBase

    Private ReadOnly _regionManager As IRegionManager

    Public Sub New(regionManager As IRegionManager)
        _regionManager = regionManager
        SignUpCommand = New DelegateCommand(AddressOf OnSignUp)
        SignInCommand = New DelegateCommand(AddressOf OnSignIn)
    End Sub

    Public ReadOnly Property SignUpCommand As DelegateCommand
    Public ReadOnly Property SignInCommand As DelegateCommand

    Private Sub OnSignUp()
        MsgBox("Registration is Locked")
        '_regionManager.RequestNavigate("AuthenticationRegion", "SignInView")
    End Sub

    Private Sub OnSignIn()
        _regionManager.RequestNavigate("AuthenticationRegion", "SignInView")
    End Sub
End Class