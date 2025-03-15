Imports Prism.Navigation.Regions

Public Class PopupService
    Implements IPopupService

    Private ReadOnly _regionManager As IRegionManager

    Public Sub New(regionManager As IRegionManager)
        _regionManager = regionManager
    End Sub

    Private Sub Show(View As Object) Implements IPopupService.Show
        _regionManager.Regions("PopUpRegion").Add(View)

        Dim mainWindow = CType(Application.Current.MainWindow, MainWindow)
        mainWindow.PopUp.Visibility = Visibility.Visible
    End Sub

    Private Sub Hide() Implements IPopupService.Hide
        _regionManager.Regions("PopUpRegion").RemoveAll()

        Dim mainWindow = CType(Application.Current.MainWindow, MainWindow)
        mainWindow.PopUp.Visibility = Visibility.Collapsed
    End Sub

    Public Sub ShowPopUp(view As Object, viewModel As Object) Implements IPopupService.ShowPopUp
        view.DataContext = viewModel

        If TypeOf viewModel Is IPopupViewModel Then
            Dim popupViewModel = CType(viewModel, IPopupViewModel)

            AddHandler popupViewModel.PopupClosed, Sub(sender, e)
                Hide()
            End Sub

        End If

        Show(view)
    End Sub

    Public Sub ShowPopUpWithResult(view As Object, viewModel As Object, callback As Action(Of Object)) Implements IPopupService.ShowPopUpWithResult
        view.DataContext = viewModel

        If TypeOf viewModel Is IPopupViewModel Then
            Dim popupViewModel = CType(viewModel, IPopupViewModel)

            AddHandler popupViewModel.PopupClosed, Sub(sender, e)
                callback(e.Result)
                Hide()
            End Sub

        End If

        Show(view)
    End Sub
End Class