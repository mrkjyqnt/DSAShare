Public Interface IPopupService
    Sub ShowPopUp(view As Object, viewModel As Object, Optional callback As Action(Of PopupResult) = Nothing) 

End Interface