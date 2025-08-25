Imports System
Imports System.IO
Imports System.Net
Imports System.Runtime.InteropServices
Imports System.Diagnostics

Module NetworkShareHelper

    Private ReadOnly _config = ConfigurationModule.GetSettings

    ''' <summary>
    ''' Connects to a network share using the provided credentials.
    ''' </summary>
    ''' <returns>True if the connection is successful, False otherwise.</returns>
    Public Function ConnectToNetworkShare() As Boolean
        Try
            Debug.WriteLine("[DEBUG] Connecting to network share...")

            Dim netResource As New NetResource() With {
                .Scope = ResourceScope.GlobalNetwork,
                .ResourceType = ResourceType.Disk,
                .DisplayType = ResourceDisplayType.Share,
                .RemoteName = _config.Network.FolderPath
            }

            Debug.WriteLine($"[DEBUG] Network Path: {_config.Network.FolderPath}")
            Debug.WriteLine($"[DEBUG] Username: {_config.Network.Username}")

            Dim result As Integer = WNetAddConnection2(netResource, _config.Network.Password, _config.Network.Username, 0)

            If result <> 0 Then
                Debug.WriteLine($"[ERROR] Failed to connect to network share. Error Code: {result} - {GetErrorMessage(result)}")
                Return False
            End If

            Debug.WriteLine("[DEBUG] Successfully connected to network share.")
            Return True

        Catch ex As Exception
            Debug.WriteLine($"[EXCEPTION] Error connecting to network share: {ex.Message}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Disconnects from a network share.
    ''' </summary>
    ''' <param name="networkPath">The UNC path of the network share.</param>
    Public Sub DisconnectFromNetworkShare(networkPath As String)
        Try
            Debug.WriteLine($"[DEBUG] Disconnecting from network share: {networkPath}")

            Dim result As Integer = WNetCancelConnection2(networkPath, 0, True)

            If result <> 0 Then
                Debug.WriteLine($"[ERROR] Failed to disconnect. Error Code: {result} - {GetErrorMessage(result)}")
            Else
                Debug.WriteLine("[DEBUG] Successfully disconnected from network share.")
            End If

        Catch ex As Exception
            Debug.WriteLine($"[EXCEPTION] Error disconnecting from network share: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Converts a Windows API error code to a human-readable message.
    ''' </summary>
    ''' <param name="errorCode">The Windows API error code.</param>
    ''' <returns>A human-readable error message.</returns>
    Private Function GetErrorMessage(errorCode As Integer) As String
        Dim message As String = New System.ComponentModel.Win32Exception(errorCode).Message
        Return message
    End Function

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