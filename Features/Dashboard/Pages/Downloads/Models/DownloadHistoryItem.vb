﻿Imports System.ComponentModel
Imports System.IO

Public Class DownloadHistoryItem
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private _url As String
    Private _filePath As String
    Private _fileName As String
    Private _downloadDate As DateTime
    Private _status As DownloadStatus
    Private _isFileExists As Boolean
    Private _fileSize As Long

    Public Property Url As String
        Get
            Return _url
        End Get
        Set(value As String)
            _url = value
            OnPropertyChanged(NameOf(Url))
        End Set
    End Property

    Public Property FilePath As String
        Get
            Return _filePath
        End Get
        Set(value As String)
            _filePath = value
            OnPropertyChanged(NameOf(FilePath))
            UpdateFileStatus()
        End Set
    End Property

    Public Property FileName As String
        Get
            Return _fileName
        End Get
        Set(value As String)
            _fileName = value
            OnPropertyChanged(NameOf(FileName))
        End Set
    End Property

    Public Property DownloadDate As DateTime
        Get
            Return _downloadDate
        End Get
        Set(value As DateTime)
            _downloadDate = value
            OnPropertyChanged(NameOf(DownloadDate))
        End Set
    End Property

    Public Property Status As DownloadStatus
        Get
            Return _status
        End Get
        Set(value As DownloadStatus)
            _status = value
            OnPropertyChanged(NameOf(Status))
        End Set
    End Property

    Public Property IsFileExists As Boolean
        Get
            Return _isFileExists
        End Get
        Private Set(value As Boolean)
            _isFileExists = value
            OnPropertyChanged(NameOf(IsFileExists))
        End Set
    End Property

    Public Property FileSize As Long
        Get
            Return _fileSize
        End Get
        Set(value As Long)
            _fileSize = value
            OnPropertyChanged(NameOf(FileSize))
        End Set
    End Property

    Public Sub UpdateFileStatus()
        Dim exists = File.Exists(FilePath)
        IsFileExists = exists
        
        ' Update file size if exists
        If exists Then
            Try
                Dim info = New FileInfo(FilePath)
                FileSize = info.Length
            Catch
                FileSize = 0
            End Try
        Else
            FileSize = 0
        End If

        ' Update status based on file existence
        If Status = DownloadStatus.Completed AndAlso Not exists Then
            Status = DownloadStatus.Removed
        ElseIf Status = DownloadStatus.Removed AndAlso exists Then
            Status = DownloadStatus.Completed
        End If
    End Sub

    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public Overrides Function Equals(obj As Object) As Boolean
        If obj Is Nothing OrElse Not TypeOf obj Is DownloadHistoryItem Then
            Return False
        End If
        Dim other = DirectCast(obj, DownloadHistoryItem)
        Return String.Equals(Me.FilePath, other.FilePath, StringComparison.Ordinal)
    End Function

    Public Overrides Function GetHashCode() As Integer
        If FilePath Is Nothing Then
            Return 0
        End If
        Return FilePath.GetHashCode()
    End Function
End Class