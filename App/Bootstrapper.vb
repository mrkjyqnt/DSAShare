Imports Prism.DryIoc
Imports Prism.Events
Imports Prism.Ioc
Imports Prism.Navigation.Regions
Imports System.Runtime.Versioning

#Disable Warning
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
        containerRegistry.RegisterSingleton(Of ISessionManager, SessionManager)()

        ' Register the Fallback 
        containerRegistry.RegisterSingleton(Of IFallbackService, FallbackService)()

        ' Register the PopUp
        containerRegistry.RegisterSingleton(Of IPopupService, PopupService)

        ' Register the Loading
        containerRegistry.RegisterSingleton(Of ILoadingService, LoadingService)

        ' Register the User Service
        containerRegistry.RegisterSingleton(Of IUserService, UserService)()

        ' Register the Fallback View
        containerRegistry.RegisterForNavigation(Of FallbackView)("FallbackView")
        containerRegistry.Register(Of FallbackViewModel)()

        ' Register the Loading View
        containerRegistry.RegisterForNavigation(Of LoadingView)("LoadingView")
        containerRegistry.RegisterForNavigation(Of StartupLoadingView)("StartupLoadingView")

        ' Register Repositories
        containerRegistry.Register(Of FileSharedRepository)()
        containerRegistry.Register(Of FileAccessedRepository)()
        containerRegistry.Register(Of UsersRepository)()
        containerRegistry.Register(Of ActivitiesRepository)()


        ' AUTHENTICATION '
        ' Register the Authentication Services
        containerRegistry.RegisterSingleton(Of IAuthenticationService, AuthenticationService)()
        containerRegistry.RegisterSingleton(Of IRegistrationService, RegistrationService)()
        containerRegistry.RegisterForNavigation(Of AuthenticationView)("AuthenticationView")

        ' Register the Authentication Pages
        containerRegistry.RegisterForNavigation(Of SignInView)("SignInView")
        containerRegistry.RegisterForNavigation(Of SignUpView)("SignUpView")


        ' DASHBOARD '
        ' Register the Navigation Services
        containerRegistry.RegisterSingleton(Of INavigationService, NavigationService)()
        containerRegistry.RegisterSingleton(Of INavigationHistoryService, NavigationHistoryService)()

        ' Register the File Services
        containerRegistry.RegisterSingleton(Of IFileDataService, FileDataService)()
        containerRegistry.RegisterSingleton(Of IFileService, FileService)
        containerRegistry.RegisterSingleton(Of IFileInfoService, FileInfoService)
        containerRegistry.RegisterSingleton(Of IFilePreviewService, FilePreviewService)()
        containerRegistry.RegisterSingleton(Of IActivityService, ActivityService)

        'Register the Download Services
        containerRegistry.RegisterSingleton(Of IDownloadService, DownloadService)()

        ' Shared
        containerRegistry.RegisterForNavigation(Of FileDetailsView)("FileDetailsView")
        containerRegistry.Register(Of FileDetailsViewModel)()
        containerRegistry.RegisterForNavigation(Of FileDetailsContentView)("FileDetailsContentView")
        containerRegistry.Register(Of FileDetailsContentViewModel)()
        containerRegistry.RegisterForNavigation(Of FileSettingsView)("FileSettingsView")
        containerRegistry.Register(Of FileSettingsViewModel)()
        containerRegistry.RegisterForNavigation(Of FileDangerZoneView)("FileDangerZoneView")
        containerRegistry.Register(Of FileDangerZoneViewModel)()
        containerRegistry.RegisterForNavigation(Of ShareFilesView)("ShareFilesView")
        containerRegistry.Register(Of ShareFilesViewModel)()
        containerRegistry.RegisterForNavigation(Of UserInformationsView)("UserInformationsView")
        containerRegistry.Register(Of UserInformationsViewModel)()
        containerRegistry.RegisterForNavigation(Of UserInformationView)("UserInformationView")
        containerRegistry.Register(Of UserInformationViewModel)()
        'containerRegistry.RegisterForNavigation(Of AccountDangerZoneView)("AccountDangerZoneView")
        'containerRegistry.Register(Of AccountDangerZoneViewModel)()

        ' Register the Dashboard
        containerRegistry.RegisterForNavigation(Of DashboardView)("DashboardView")
        containerRegistry.Register(Of DashboardViewModel)()


        ' Register the Dashboard Navigation
        containerRegistry.RegisterForNavigation(Of NavigationView)("NavigationView")
        containerRegistry.Register(Of NavigationViewModel)()
        containerRegistry.RegisterSingleton(Of INavigationListService, NavigationListService)()


        ' Register the Dashboard Pages
        containerRegistry.RegisterForNavigation(Of HomeView)("HomeView")
        containerRegistry.Register(Of HomeViewModel)()
        containerRegistry.RegisterForNavigation(Of PublicFilesView)("PublicFilesView")
        containerRegistry.Register(Of PublicFilesViewModel)()
        containerRegistry.RegisterForNavigation(Of SharedFilesView)("SharedFilesView")
        containerRegistry.Register(Of SharedFilesViewModel)()
        containerRegistry.RegisterForNavigation(Of AccessedFilesView)("AccessedFilesView")
        containerRegistry.Register(Of AccessedFilesViewModel)()
        containerRegistry.RegisterForNavigation(Of ManageUsersView)("ManageUsersView")
        'containerRegistry.Register(Of ManageUsersViewModel)()
        containerRegistry.RegisterForNavigation(Of ManageFilesView)("ManageFilesView")
        'containerRegistry.Register(Of ManageFilesViewModel)()
        containerRegistry.RegisterForNavigation(Of DownloadsView)("DownloadsView")
        containerRegistry.Register(Of DownloadsViewModel)()
        containerRegistry.RegisterForNavigation(Of AccountView)("AccountView")
        containerRegistry.Register(Of AccountViewModel)()


    End Sub

    ''' <summary>
    ''' Initialize the application.
    ''' </summary>
    Protected Overrides Async Sub OnInitialized()
        MyBase.OnInitialized()

        Dim regionManager = Container.Resolve(Of IRegionManager)()
        Dim sessionManager = Container.Resolve(Of ISessionManager)()
        Dim navigation = Container.Resolve(Of INavigationService)

        sessionManager.LoadSession()
        ConfigurationModule.GetSettings()
        VerifyConfigFileExistence

        Try
            Loading.StartUp()
            Await Task.Delay(3000).ConfigureAwait(True)

            navigation.Go("MainRegion", "AuthenticationView")

        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Error initializing the application: {ex.Message}")
        Finally
            Loading.Hide()
        End Try
    End Sub



End Class