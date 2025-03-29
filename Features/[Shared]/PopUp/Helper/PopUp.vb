Imports Prism.Navigation.Regions
Imports Prism.Ioc

Public Class PopUp
    Private Shared _popupService As IPopupService

    ''' <summary>
    ''' Initializes the <see cref="PopUp"/> class.
    ''' </summary>
    Shared Sub New()
        Dim regionManager As IRegionManager = ContainerLocator.Container.Resolve(Of IRegionManager)()
        _popupService = New PopupService(regionManager)
    End Sub

    ''' <summary>
    ''' Shows the information pop up.
    ''' </summary>
    ''' <param name="Title"></param>
    ''' <param name="Content"></param>
    Public Shared Function Information(Title As String, Content As String) As Task
        Dim tcs As New TaskCompletionSource(Of Boolean)()
        Dim infoPopupViewModel As New InformationPopUpViewModel(Title, Content)

        _popupService.ShowPopUp(New InformationPopUpView(), infoPopupViewModel,
                               Sub(result)
                                   tcs.TrySetResult(True)
                               End Sub)

        Return tcs.Task
    End Function

    ''' <summary>
    ''' Shows the selection pop up.
    ''' </summary>
    ''' <returns></returns>
    Public Shared Async Function Selection() As Task(Of PopupResult)
        Dim tcs As New TaskCompletionSource(Of PopupResult)()
        Dim selectionPopupViewModel As New SelectionPopUpViewModel()

        _popupService.ShowPopUp(New SelectionPopUpView(), selectionPopupViewModel,
                               Sub(result)
                                   tcs.TrySetResult(result)
                               End Sub)

        Return Await tcs.Task
    End Function

    ''' <summary>
    ''' Shows the confirmation pop up.
    ''' </summary>
    ''' <returns></returns>
    Public Shared Async Function Confirmation() As Task(Of PopupResult)
        Dim tcs As New TaskCompletionSource(Of PopupResult)()
        Dim confirmationPopupViewModel As New ConfirmationPopUpViewModel()

        _popupService.ShowPopUp(New ConfirmationPopUpView(), confirmationPopupViewModel,
                               Sub(result)
                                   tcs.TrySetResult(result)
                               End Sub)

        Return Await tcs.Task
    End Function
End Class