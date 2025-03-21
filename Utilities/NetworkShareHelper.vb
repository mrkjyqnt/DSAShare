Imports System
Imports System.IO
Imports System.Net
Imports System.Runtime.InteropServices

Module NetworkShareHelper
    ''' <summary>
    ''' Connects to a network share using the provided credentials.
    ''' </summary>
    ''' <param name="networkPath">The UNC path of the network share (e.g., \\192.168.8.10\Server Storage\).</param>
    ''' <param name="credentials">The credentials to use for authentication.</param>
    Public Sub ConnectToNetworkShare(networkPath As String, credentials As NetworkCredential)
        Dim netResource As New NetResource() With {
            .Scope = ResourceScope.GlobalNetwork,
            .ResourceType = ResourceType.Disk,
            .DisplayType = ResourceDisplayType.Share,
            .RemoteName = networkPath
        }

        Dim result As Integer = WNetAddConnection2(netResource, credentials.Password, credentials.UserName, 0)

        If result <> 0 Then
            Throw New IOException($"Failed to connect to the network share. Error code: {result}", result)
        End If
    End Sub

    ''' <summary>
    ''' Disconnects from a network share.
    ''' </summary>
    ''' <param name="networkPath">The UNC path of the network share.</param>
    Public Sub DisconnectFromNetworkShare(networkPath As String)
        WNetCancelConnection2(networkPath, 0, True)
    End Sub

    ' P/Invoke declarations for WNetAddConnection2 and WNetCancelConnection2
    <DllImport("mpr.dll", CharSet:=CharSet.Auto)>
    Private Function WNetAddConnection2(netResource As NetResource, password As String, username As String, flags As Integer) As Integer
    End Function

    <DllImport("mpr.dll", CharSet:=CharSet.Auto)>
    Private Function WNetCancelConnection2(name As String, flags As Integer, force As Boolean) As Integer
    End Function

    ' NetResource structure
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)>
    Private Class NetResource
        Public Scope As ResourceScope
        Public ResourceType As ResourceType
        Public DisplayType As ResourceDisplayType
        Public Usage As Integer
        Public LocalName As String
        Public RemoteName As String
        Public Comment As String
        Public Provider As String
    End Class

    ' ResourceScope enumeration
    Private Enum ResourceScope
        Connected = 1
        GlobalNetwork
        Remembered
        Recent
        Context
    End Enum

    ' ResourceType enumeration
    Private Enum ResourceType
        Any = 0
        Disk = 1
        Print = 2
        Reserved = 8
    End Enum

    ' ResourceDisplayType enumeration
    Private Enum ResourceDisplayType
        Generic = &H0
        Domain = &H1
        Server = &H2
        Share = &H3
        File = &H4
        Group = &H5
        Network = &H6
        Root = &H7
        Shareadmin = &H8
        Directory = &H9
        Tree = &HA
        Ndscontainer = &HB
    End Enum
End Module