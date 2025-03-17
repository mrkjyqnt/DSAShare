Imports Prism.Navigation.Regions
Imports Prism.Ioc

Public Class PopUp
    Private Shared _popupService As IPopupService

    ' Shared constructor to initialize _popupService
    Shared Sub New()
        Dim regionManager As IRegionManager = ContainerLocator.Container.Resolve(Of IRegionManager)()
        _popupService = New PopupService(regionManager)
    End Sub

    ' Information Popup (no result)
    Public Shared Sub Information(Title As String, Content As String)
        Dim infoPopupViewModel As New InformationPopUpViewModel(Title, Content)
        _popupService.ShowPopUp(New InformationPopUpView(), infoPopupViewModel)
    End Sub

    ' Selection Popup (with result)
    Public Shared Async Function Selection() As Task(Of PopupResult)
        Dim tcs As New TaskCompletionSource(Of PopupResult)()
        Dim selectionPopupViewModel As New SelectionPopUpViewModel()

        _popupService.ShowPopUp(New SelectionPopUpView(), selectionPopupViewModel,
                               Sub(result)
                                   tcs.TrySetResult(result)
                               End Sub)

        Return Await tcs.Task ' Wait for the popup to close and return the result
    End Function
End Class