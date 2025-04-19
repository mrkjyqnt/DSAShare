Imports System.IO
Imports System.Threading.Tasks.Dataflow

Public Class ActivityService
    Implements IActivityService

    Private Property ActivityCount As Integer
    Private ReadOnly _activitiesRepository As ActivitiesRepository
    Private ReadOnly _sessionManager As ISessionManager

    Public Sub New(activitiesRepository As ActivitiesRepository,
                   sessionManager As ISessionManager)
        _activitiesRepository = activitiesRepository
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

    Public Function GetUserActivity(Optional user As Users = Nothing) As List(Of Activities) Implements IActivityService.GetUserActivity
        Dim result = New List(Of Activities)()
        Dim _activityList As List(Of Activities)

        Try
            _activityList = _activitiesRepository.GetByUserId(New Activities With {
                .UserId = If(user?.Id, _sessionManager.CurrentUser.Id)
            })

            If IsNullOrEmpty(_activityList) OrElse _activityList.Count < 0 Then
                Return Nothing
            End If

            Debug.WriteLine($"[DEBUG] Found {_activityList.Count} activities")

            For Each activity In _activityList
                Debug.WriteLine($"[DEBUG] Adding activity: {activity.Id}")
                result.Add(New Activities With {
                    .Id = activity.Id,
                    .FileId = activity.FileId,
                    .AccountId = activity.AccountId,
                    .Action = activity.Action,
                    .ActionIn = activity.ActionIn,
                    .Name = activity.Name,
                    .ActionAt = activity.ActionAt
                })
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

    Public Sub DeleteAllActivity(user As Users) Implements IActivityService.DeleteAllActivity
        Try
            _activitiesRepository.DeleteAll(New Activities With {.UserId = user.Id})
        Catch ex As Exception
            Debug.WriteLine($"[DEBUG] Theres an error deleting the user activities")
            Debug.WriteLine(ex.Message)
        End Try
    End Sub
End Class
