Imports Prism.Navigation.Regions

Public Class AuthenticationView
    Private ReadOnly _regionManager As IRegionManager

    Public Sub New(regionManager As IRegionManager)
        _regionManager = regionManager
        InitializeComponent()

        _regionManager.RequestNavigate("AuthenticationRegion", "SignInView")
    End Sub
End Class
