Imports Prism.DryIoc
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

        ' Register the SessionManager as a singleton
        containerRegistry.Register(Of ISessionManager, SessionManager)()
        containerRegistry.RegisterSingleton(Of SessionManager)()

        containerRegistry.Register(Of IFallbackService, FallbackService)()
        containerRegistry.RegisterSingleton(Of FallbackService)()
        
        ' Register the PopUp Service and Views
        containerRegistry.Register(Of IPopupService, PopupService)
        containerRegistry.RegisterSingleton(Of PopupService)

        ' Register the Loading
        containerRegistry.Register(Of ILoadingService, LoadingService)
        containerRegistry.RegisterSingleton(Of LoadingService)

        ' Register the Authentication
        containerRegistry.Register(Of IAuthenticationService, AuthenticationService)()
        containerRegistry.Register(Of IRegistrationService, RegistrationService)()
        containerRegistry.RegisterForNavigation(Of AuthenticationView)("AuthenticationView")

        ' Register the Fallback
        containerRegistry.RegisterForNavigation(Of FallbackView)("FallBackView")
        containerRegistry.RegisterForNavigation(Of LoadingView)("LoadingView")

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

    ''' <summary>
    ''' Initialize the application.
    ''' </summary>
    Protected Overrides Async Sub OnInitialized()
        MyBase.OnInitialized()

        Dim regionManager = Container.Resolve(Of IRegionManager)()
        Dim sessionManager = Container.Resolve(Of ISessionManager)()
        Dim loadingService = Container.Resolve(Of ILoadingService)()
        Dim dispatcher = Application.Current.Dispatcher

        sessionManager.LoadSession()
        loadingService.Show(New StartupLoadingView)

        Try
            If Not Await Task.Run(Function() _connection.TestConnection()).ConfigureAwait(False) Then
                dispatcher.Invoke(Sub()
                    regionManager.RequestNavigate("MainRegion", "FallBackView")
                End Sub)
                Return
            End If

            If sessionManager.IsLoggedIn() Then
                dispatcher.Invoke(Sub()
                    regionManager.RequestNavigate("MainRegion", "DashboardView")
                End Sub)
            Else
                dispatcher.Invoke(Sub()
                    regionManager.RequestNavigate("MainRegion", "AuthenticationView")
                End Sub)
            End If

        Catch ex As Exception
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            dispatcher.Invoke(Sub()
                loadingService.Hide
            End Sub)
        End Try
    End Sub


    
End Class