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
    Private ReadOnly _sessionManager As ISessionManager

    Public Sub New(regionManager As IRegionManager, sessionManager As ISessionManager)

        _regionManager = regionManager
        _sessionManager = sessionManager

        _regionManager.RegisterViewWithRegion("PageRegion", "HomeView")
        _regionManager.RegisterViewWithRegion("NavigationRegion", "NavigationView")

        If _sessionManager.CurrentUser IsNot Nothing Then
            MsgBox("Hello " & _sessionManager.CurrentUser.Role)
        Else
            MsgBox("No user session found!")
        End If

    End Sub
End Class