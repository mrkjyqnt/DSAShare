Imports Prism.DryIoc
Imports Prism.Events
Imports Prism.Ioc
Imports Prism.Navigation.Regions
Imports System.Runtime.Versioning

<SupportedOSPlatform("windows7.0")>
<SupportedOSPlatform("windows10.0")>
<SupportedOSPlatform("windows11.0")>
Public Class Bootstrapper
    Inherits PrismBootstrapper

    Private ReadOnly _connection As New Connection()

    ''' <summary>
    ''' Create the shell for the application.
    ''' </summary>
    ''' <returns></returns>
    Protected Overrides Function CreateShell() As DependencyObject
        Return Container.Resolve(Of MainWindow)()
    End Function

    ''' <summary>
    ''' Register the types for the application.
    ''' </summary>
    ''' <param name="containerRegistry"></param>
    Protected Overrides Sub RegisterTypes(containerRegistry As IContainerRegistry)
        ' PACKED '
        containerRegistry.RegisterSingleton(Of IRegionManager, RegionManager)()
        containerRegistry.RegisterSingleton(Of IEventAggregator, EventAggregator)()


        ' SHARED '
        ' Register the SessionManager
        containerRegistry.Register(Of ISessionManager, SessionManager)()
        containerRegistry.RegisterSingleton(Of SessionManager)()

        ' Register the Navigation
        containerRegistry.Register(Of INavigationService, NavigationService)()
        containerRegistry.RegisterSingleton(Of NavigationService)

        ' Register the Fallback 
        containerRegistry.Register(Of IFallbackService, FallbackService)()
        containerRegistry.RegisterSingleton(Of FallbackService)()

        ' Register the PopUp
        containerRegistry.Register(Of IPopupService, PopupService)
        containerRegistry.RegisterSingleton(Of PopupService)

        ' Register the Loading
        containerRegistry.Register(Of ILoadingService, LoadingService)
        containerRegistry.RegisterSingleton(Of LoadingService)

        ' Register the Fallback View
        containerRegistry.RegisterForNavigation(Of FallbackView)("FallBackView")

        ' Register the Loading View
        containerRegistry.RegisterForNavigation(Of LoadingView)("LoadingView")
        containerRegistry.RegisterForNavigation(Of StartupLoadingView)("StartupLoadingView")


        ' AUTHENTICATION '
        ' Register the Authentication Services
        containerRegistry.Register(Of IAuthenticationService, AuthenticationService)()
        containerRegistry.Register(Of IRegistrationService, RegistrationService)()
        containerRegistry.RegisterForNavigation(Of AuthenticationView)("AuthenticationView")

        ' Register the Authentication Pages
        containerRegistry.RegisterForNavigation(Of SignInView)("SignInView")
        containerRegistry.RegisterForNavigation(Of SignUpView)("SignUpView")


        ' DASHBOARD '
        ' Register the Dashboard
        containerRegistry.RegisterForNavigation(Of DashboardView)("DashboardView")
        containerRegistry.Register(Of DashboardViewModel)()
        containerRegistry.RegisterForNavigation(Of NavigationView)("NavigationView")
        containerRegistry.Register(Of NavigationViewModel)()

        ' Register the Dashboard Services
        containerRegistry.Register(Of IFileDataService, FileDataService)
        containerRegistry.Register(Of IFileDownloadService, FileDownloadService)
        containerRegistry.Register(Of IFileUploadService, FileUploadService)
        containerRegistry.RegisterSingleton(Of FileDataService)
        containerRegistry.RegisterSingleton(Of FileDownloadService)
        containerRegistry.RegisterSingleton(Of FileUploadService)

        ' Register the Dashboard Pages
        containerRegistry.RegisterForNavigation(Of HomeView)("HomeView")
        containerRegistry.Register(Of HomeViewModel)()
        containerRegistry.RegisterForNavigation(Of AccountView)("AccountView")
        'containerRegistry.Register(Of AccountViewModel)()
        containerRegistry.RegisterForNavigation(Of PublicFilesView)("PublicFilesView")
        'containerRegistry.Register(Of PublicFilesViewModel)()

        containerRegistry.RegisterForNavigation(Of SharedFilesView)("SharedFilesView")
        'containerRegistry.Register(Of SharedFilesViewModel)()
            containerRegistry.RegisterForNavigation(Of ShareFilesView)("ShareFilesView")
            containerRegistry.Register(Of ShareFilesViewModel)()


        containerRegistry.RegisterForNavigation(Of AccessedFilesView)("AccessedFilesView")
        'containerRegistry.Register(Of AccessedFilesViewModel)()
        containerRegistry.RegisterForNavigation(Of ManageUsersView)("ManageUsersView")
        'containerRegistry.Register(Of ManageUsersViewModel)()
        containerRegistry.RegisterForNavigation(Of ManageFilesView)("ManageFilesView")
        'containerRegistry.Register(Of ManageFilesViewModel)()
    End Sub

    ''' <summary>
    ''' Initialize the application.
    ''' </summary>
    Protected Overrides Async Sub OnInitialized()
        MyBase.OnInitialized()

        Dim regionManager = Container.Resolve(Of IRegionManager)()
        Dim sessionManager = Container.Resolve(Of ISessionManager)()

        sessionManager.LoadSession()

        Try
            Loading.StartUp()
            If Not Await Task.Run(Function() _connection.TestConnection()).ConfigureAwait(True) Then
                regionManager.RequestNavigate("MainRegion", "FallBackView")
                Return
            End If

            If sessionManager.IsLoggedIn() Then
                regionManager.RequestNavigate("MainRegion", "DashboardView")
            Else
                regionManager.RequestNavigate("MainRegion", "AuthenticationView")
            End If

        Catch ex As Exception
            PopUp.Information("Error", ex.Message)
        Finally
            Loading.Hide
        End Try
    End Sub



End Class