Imports Prism.Mvvm

Public Class LoadingViewModel
    Inherits BindableBase

    ''' <summary>
    ''' Gets or sets the IsLoading property. This observable property 
    ''' indicates if the view is currently loading.
    ''' </summary>
    Private _isLoading As Boolean
    Public Property IsLoading As Boolean
        Get
            Return _isLoading
        End Get
        Set(value As Boolean)
            SetProperty(_isLoading, value)
        End Set
    End Property

    ''' <summary>
    ''' Initializes a new instance of the <see cref="LoadingViewModel"/> class.
    ''' </summary>
    Sub New()
    End Sub
End Class
