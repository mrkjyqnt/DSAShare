Imports Prism.Navigation.Regions

Public Class PopupService
    Implements IPopupService

    Private ReadOnly _regionManager As IRegionManager

    Public Sub New(regionManager As IRegionManager)
        _regionManager = regionManager
    End Sub

    Private Sub Show(view As Object)
        _regionManager.Regions("PopUpRegion").Add(view)

        Dim mainWindow = CType(Application.Current.MainWindow, MainWindow)
        mainWindow.PopUp.Visibility = Visibility.Visible
    End Sub

    Private Sub Hide()
        _regionManager.Regions("PopUpRegion").RemoveAll()

        Dim mainWindow = CType(Application.Current.MainWindow, MainWindow)
        mainWindow.PopUp.Visibility = Visibility.Collapsed
    End Sub

    Public Sub ShowPopUp(view As Object, viewModel As Object, Optional callback As Action(Of PopupResult) = Nothing) Implements IPopupService.ShowPopUp
        view.DataContext = viewModel

        If TypeOf viewModel Is IPopupViewModel Then
            Dim popupViewModel = CType(viewModel, IPopupViewModel)

            AddHandler popupViewModel.PopupClosed, Sub(sender, e)
                                                      callback?.Invoke(e.Result)
                                                      Hide()
                                                  End Sub
        Else
            Console.WriteLine("Referenced ViewModel should implement IPopupViewModel")
            Return
        End If

        Show(view)
    End Sub
End Class