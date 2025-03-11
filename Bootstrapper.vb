﻿Imports Prism.DryIoc
Imports Prism.Ioc
Imports Prism.Navigation.Regions
Imports System.Runtime.Versioning


Public Class Bootstrapper
    Inherits PrismBootstrapper

    Private ReadOnly _connection As New Connection() 

    Protected Overrides Function CreateShell() As DependencyObject
        Return Container.Resolve(Of MainWindow)()
    End Function

    Protected Overrides Sub RegisterTypes(containerRegistry As IContainerRegistry)

         ' Register the SessionManager as a singleton
        containerRegistry.Register(Of ISessionManager, SessionManager)()
        containerRegistry.RegisterSingleton(Of SessionManager)()

        ' Register the Authentication
        containerRegistry.Register(Of IAuthenticationService, AuthenticationService)()
        containerRegistry.RegisterForNavigation(Of AuthenticationView)("AuthenticationView")

        ' Register the Fallback
        containerRegistry.RegisterForNavigation(Of FallbackView)("FallBackView")

        ' Register the Authentication Pages
        containerRegistry.RegisterForNavigation(Of SignInView)("SignInView")
        containerRegistry.RegisterForNavigation(Of SignUpView)("SignUpView")

        ' Register the Dashboard
        containerRegistry.RegisterForNavigation(Of DashboardView)("DashboardView")
        containerRegistry.RegisterForNavigation(Of NavigationView)("NavigationView")

         ' Register the Dashboard Pages
        containerRegistry.RegisterForNavigation(Of HomeView)("HomeView")
        containerRegistry.RegisterForNavigation(Of AccountView)("AccountView")
        containerRegistry.RegisterForNavigation(Of PublicFilesView)("PublicFilesView")
        containerRegistry.RegisterForNavigation(Of SharedFilesView)("SharedFilesView")
        containerRegistry.RegisterForNavigation(Of AccessedFilesView)("AccessedFilesView")
        containerRegistry.RegisterForNavigation(Of ManageUsersView)("ManageUsersView")
        containerRegistry.RegisterForNavigation(Of ManageFilesView)("ManageFilesView")
    End Sub

    <SupportedOSPlatform("windows10.0")>
    <SupportedOSPlatform("windows11.0")>
    Protected Overrides Sub OnInitialized()
        Dim regionManager = Container.Resolve(Of IRegionManager)()
        Dim sessionManager = Container.Resolve(Of ISessionManager)()

        If Not _connection.TestConnection() Then
            MessageBox.Show("Cannot connect to the server", "Database Connection Error", MessageBoxButton.OK, MessageBoxImage.Error)
            regionManager.RequestNavigate("MainRegion", "FallBackView")
            Return
        End If

        sessionManager.LoadSession()

        MyBase.OnInitialized()

        If sessionManager.IsLoggedIn() Then
            regionManager.RequestNavigate("MainRegion", "DashboardView")
        Else
            regionManager.RequestNavigate("MainRegion", "AuthenticationView")
        End If
    End Sub

    
End Class