Imports System.Data

Public Class ActivitiesRepository
    Private ReadOnly _connection As Connection

    Public Sub New(connection As Connection)
        _connection = connection
    End Sub

    Public Function GetByUserId(activities As Activities) As List(Of Activities)
        Dim activityList As New List(Of Activities)()

        ' Modified query with ORDER BY
        _connection.Prepare("SELECT * FROM activities WHERE user_id = @user_id ORDER BY id DESC")
        _connection.AddParam("@user_id", activities.UserId)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        Dim records = _connection.FetchAll()

        For Each record As DataRow In records
            Dim activity As New Activities() With {
                .Id = record("id"),
                .Action = record("action").ToString,
                .ActionIn = record("action_in").ToString,
                .ActionAt = record("action_at").ToString,
                .FileId = record("file_id"),
                .UserId = record("user_id")
            }

            activityList.Add(activity)
        Next

        Return activityList
    End Function

    Public Function Read() As List(Of Activities)
        Dim activityList As New List(Of Activities)()

        ' Modified query with ORDER BY
        _connection.Prepare("SELECT * FROM activities ORDER BY id DESC")
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        Dim records = _connection.FetchAll()
        For Each record As DataRow In records
            Dim activity As New Activities() With {
                .Id = record("id"),
                .Action = record("action").ToString,
                .ActionIn = record("action_in").ToString,
                .ActionAt = record("action_at").ToString,
                .FileId = record("file_id"),
                .UserId = record("user_id")
            }
            activityList.Add(activity)
        Next
        Return activityList
    End Function

    ' Rest of your methods remain unchanged
    Public Function Insert(activities As Activities) As Boolean
        _connection.Prepare("INSERT INTO activities (action, action_in, action_at, file_id, user_id) VALUES (@action, @action_in, @action_at, @file_id, @user_id)")
        _connection.AddParam("@action", activities.Action)
        _connection.AddParam("@action_in", activities.ActionIn)
        _connection.AddParam("@action_at", activities.ActionAt)
        _connection.AddParam("@file_id", activities.FileId)
        _connection.AddParam("@user_id", activities.UserId)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If

        Return True
    End Function

    Public Function Delete(activities As Activities) As Boolean
        _connection.Prepare("SELECT * FROM activities WHERE user_id = @user_id and id = @id")
        _connection.AddParam("@user_id", activities.UserId)
        _connection.AddParam("@id", activities.Id)
        _connection.Execute()

        If Not _connection.HasRecord Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If

        _connection.Prepare("DELETE FROM activities WHERE id = @id")
        _connection.AddParam("@id", activities.Id)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If
        Return True
    End Function
End Class