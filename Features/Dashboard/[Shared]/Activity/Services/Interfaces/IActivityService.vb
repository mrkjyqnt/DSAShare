Public Interface IActivityService
    Property ActivityCount As Integer
    Function GetUserActivity(Optional activities As Activities = Nothing) As List(Of ActivityServiceModel)
    Function AddActivity(activity As Activities) As Boolean
    Sub DeleteAllActivity(user As Users)
End Interface
