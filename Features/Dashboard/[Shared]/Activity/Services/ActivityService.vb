﻿Imports System.IO
Imports System.Threading.Tasks.Dataflow

Public Class ActivityService
    Implements IActivityService

    Private Property ActivityCount As Integer
    Private ReadOnly _activitiesRepository As ActivitiesRepository
    Private ReadOnly _fileSharedRepository As FileSharedRepository
    Private ReadOnly _sessionManager As ISessionManager

    Public Sub New(activitiesRepository As ActivitiesRepository,
                   fileSharedRepository As FileSharedRepository,
                   sessionManager As ISessionManager)
        _activitiesRepository = activitiesRepository
        _fileSharedRepository = fileSharedRepository
        _sessionManager = sessionManager
    End Sub

    Public Property IActivityService_ActivityCount As Integer Implements IActivityService.ActivityCount
        Get
            Return ActivityCount
        End Get
        Set(value As Integer)
            ActivityCount = value
        End Set
    End Property

    Public Function GetUserActivity(Optional activities As Activities = Nothing) As List(Of ActivityServiceModel) Implements IActivityService.GetUserActivity
        Dim _fileShared As FilesShared
        Dim result = New List(Of ActivityServiceModel)()
        Dim _activityList As List(Of Activities)

        Try
            _activityList = _activitiesRepository.GetByUserId(New Activities With {
                .UserId = If(activities?.UserId, _sessionManager.CurrentUser.Id)
            })

            If IsNullOrEmpty(_activityList) Then
                Debug.WriteLine("[DEBUG] No activities found")
                result.Add(New ActivityServiceModel With {
                    .Id = 0,
                    .FileId = 0,
                    .Action = "No recent",
                    .ActionIn = "No Reference",
                    .ActionAt = DateTime.Now.ToString,
                    .FileName = "Activities"
                })
                Return result
            End If

            Debug.WriteLine($"[DEBUG] Found {_activityList.Count} activities")

            For Each activity In _activityList
                _fileShared = _fileSharedRepository.GetById(New FilesShared With {
                    .Id = activity.FileId
                })

                If _fileShared IsNot Nothing Then
                    Debug.WriteLine($"[DEBUG] Adding activity: {_fileShared.Id}")
                    result.Add(New ActivityServiceModel With {
                        .Id = activity.Id,
                        .FileId = _fileShared.Id,
                        .Action = activity.Action,
                        .ActionIn = activity.ActionIn,
                        .ActionAt = activity.ActionAt,
                        .FileName = _fileShared.FileName
                    })
                Else
                    Debug.WriteLine("[DEBUG] No file shared found for activity")
                End If
            Next

            Return result
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Theres an error getting the user activities")
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function AddActivity(activity As Activities) As Boolean Implements IActivityService.AddActivity
        Try
            Dim result = _activitiesRepository.Insert(activity)

            If result Then
                Debug.WriteLine("[DEBUG] Error: Cannot add activity")
            End If

            Return result
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Theres an error adding the user activities")
            Debug.WriteLine(ex.Message)
            Return False
        End Try
    End Function
End Class
