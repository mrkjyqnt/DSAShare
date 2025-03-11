Imports Prism.Mvvm
Imports Prism.Navigation.Regions

Public Class FallbackViewModel
    Inherits BindableBase

    Private ReadOnly _regionManager As IRegionManager

    Sub New(regionManager As IRegionManager)
        _regionManager = regionManager
    End Sub

End Class
