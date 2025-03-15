Public Interface IPopupService
    Sub Show(content As Object)
    Sub Hide()
    Sub ShowPopUp(view As Object, viewModel As Object)
    Sub ShowPopUpWithResult(view As Object, viewModel As Object, callback As Action(Of Object))

End Interface