Public Interface IActivityService
    Property ActivityCount As Integer
    Function GetUserActivity(Optional user As Users = Nothing) As List(Of Activities)
    Function AddActivity(activity As Activities) As Boolean
    Sub DeleteAllActivity(user As Users)
End Interface
