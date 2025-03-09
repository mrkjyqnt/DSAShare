Imports System.Windows
Imports Prism.DryIoc

Public Class Application
    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        MyBase.OnStartup(e)

        ' Initialize the Prism Bootstrapper
        Dim bootstrapper As New Bootstrapper()
        bootstrapper.Run()
    End Sub
End Class