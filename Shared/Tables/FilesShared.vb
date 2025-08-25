Imports System
Imports System.Collections.Generic
Imports System.ComponentModel

''' <summary>
''' Represents the FilesShared table.
''' </summary>
Public Class FilesShared
    Implements INotifyPropertyChanged

    Public Property Id As Integer? = Nothing
    Public Property Name As String
    Public Property FileName As String
    Public Property FileDescription As String
    Public Property FilePath As String
    Public Property FileSize As String
    Public Property FileType As String
    Public Property UploadedBy As Integer? = Nothing
    Public Property ShareType As String
    Public Property ShareValue As String
    Public Property ExpiryDate As DateTime? = Nothing
    Public Property Privacy As String
    Public Property DownloadCount As Integer? = Nothing
    Public Property Availability As String
    Public Property CreatedAt As DateTime? = Nothing
    Public Property UpdatedAt As DateTime? = Nothing

    Private _progress As Integer
    Public Property Progress As Integer
        Get
            Return _progress
        End Get
        Set(value As Integer)
            If _progress <> value Then
                _progress = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Progress)))
            End If
        End Set
    End Property

    Private _status As String

    Public Property Status As String
        Get
            Return _status
        End Get
        Set(value As String)
            If _status <> value Then
                _status = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Status)))
            End If
        End Set
    End Property

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class