Imports Prism.Mvvm
Imports Prism.Navigation.Regions

''' <summary>
''' ViewModel for the Authentication.
''' </summary>
Public Class AuthenticationViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Return False
        End Get
    End Property

    Private ReadOnly _regionManager As IRegionManager

    Public Sub New(regionManager As IRegionManager)

        _regionManager = regionManager

        ' Navigate to SignInView when the ViewModel is created
        _regionManager.RegisterViewWithRegion(Of SignInView)("AuthenticationRegion")
    End Sub


End Class