Imports Prism.DryIoc
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
        containerRegistry.RegisterSingleton(Of SessionManager)()

        ' Register the Authentication
        containerRegistry.Register(Of IAuthenticationService, AuthenticationService)()
        containerRegistry.RegisterForNavigation(Of AuthenticationView)("AuthenticationView")

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

        ' Check if the server is open
        If Not _connection.TestConnection() Then
            MessageBox.Show("Cannot connect to the server", "Database Connection Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Application.Current.Shutdown()
            Return
        End If

        MyBase.OnInitialized()
        ' Start with the AuthenticationView
        Dim regionManager = ContainerLocator.Container.Resolve(Of IRegionManager)()
        regionManager.RequestNavigate("MainRegion", "AuthenticationView")
    End Sub

    
End Class