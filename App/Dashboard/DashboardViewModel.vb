Imports System.ComponentModel
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

''' <summary>
''' ViewModel for the Dashboard.
''' </summary>
Public Class DashboardViewModel
    Inherits BindableBase

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _sessionManager As SessionManager

    Public Sub New(regionManager As IRegionManager, sessionManager As SessionManager)

        _regionManager = regionManager
        _sessionManager = sessionManager

        _regionManager.RegisterViewWithRegion("PageRegion", "HomeView")
        _regionManager.RegisterViewWithRegion("NavigationRegion", "NavigationView")

        MsgBox("Hello " + _sessionManager.CurrentUser.Role)
    End Sub
End Class