Imports System.ComponentModel
Imports Prism.Commands
Imports Prism.Mvvm
Imports Prism.Navigation.Regions

''' <summary>
''' ViewModel for the Dashboard.
''' </summary>
Public Class DashboardViewModel
    Inherits BindableBase
    Implements IRegionMemberLifetime

    Private ReadOnly _regionManager As IRegionManager
    Private ReadOnly _sessionManager As ISessionManager

    Public ReadOnly Property KeepAlive As Boolean Implements IRegionMemberLifetime.KeepAlive
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Sub New(regionManager As IRegionManager, sessionManager As ISessionManager)

        _regionManager = regionManager
        _sessionManager = sessionManager

        _regionManager.RegisterViewWithRegion("PageRegion", "HomeView")
        _regionManager.RegisterViewWithRegion("NavigationRegion", "NavigationView")
    End Sub
End Class