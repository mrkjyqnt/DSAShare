Imports System.Data
Imports Microsoft.VisualBasic.ApplicationServices

Public Class SecurityRepository
    Private ReadOnly _connection As Connection

    Public Sub New(connection As Connection)
        _connection = connection
    End Sub

    Public Function GetByUserId(userId As Integer) As Security
        _connection.Prepare("SELECT * FROM security WHERE user_id = @userId ORDER BY id DESC")
        _connection.AddParam("@userId", userId)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[SecurityRepository] GetByUserId Error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New Security() With {
                .Id = _connection.DataRow("id"),
                .UserId = _connection.DataRow("user_id"),
                .MacAddress = _connection.DataRow("mac_address").ToString(),
                .AttemptCount = _connection.DataRow("attempt_count"),
                .LockUntil = If(IsDBNull(_connection.DataRow("lock_until")), Nothing, _connection.DataRow("lock_until"))
            }
        End If

        Return Nothing
    End Function

    Public Function GetByMacAddress(security As Security) As Security
        _connection.Prepare("SELECT * FROM security WHERE mac_address = @macAddress ORDER BY id DESC")
        _connection.AddParam("@macAddress", security.MacAddress)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[SecurityRepository] GetByMacAddress Error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New Security() With {
                .Id = _connection.DataRow("id"),
                .UserId = _connection.DataRow("user_id"),
                .MacAddress = _connection.DataRow("mac_address").ToString(),
                .AttemptCount = _connection.DataRow("attempt_count"),
                .LockUntil = If(IsDBNull(_connection.DataRow("lock_until")), Nothing, _connection.DataRow("lock_until"))
            }
        End If

        Return Nothing
    End Function

    Public Function Read(security As Security) As Security
        _connection.Prepare("SELECT * FROM security WHERE user_id = @userId ORDER BY id DESC")
        _connection.AddParam("@userId", security.UserId)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[SecurityRepository] Read Error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New Security() With {
                .Id = _connection.DataRow("id"),
                .UserId = _connection.DataRow("user_id"),
                .MacAddress = _connection.DataRow("mac_address").ToString(),
                .AttemptCount = _connection.DataRow("attempt_count"),
                .LockUntil = If(IsDBNull(_connection.DataRow("lock_until")), Nothing, _connection.DataRow("lock_until"))
            }
        End If
        Return Nothing
    End Function

    Public Sub Insert(security As Security)
        _connection.Prepare("
        INSERT INTO security (user_id, mac_address, attempt_count, lock_until)
        VALUES (@userId, @macAddress, @attemptCount, @lockUntil)
         ")

        _connection.AddParam("@userId", security.UserId)
        _connection.AddParam("@macAddress", security.MacAddress)
        _connection.AddParam("@attemptCount", security.AttemptCount)
        _connection.AddParam("@lockUntil", security.LockUntil)

        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[SecurityRepository] Insert Error: {_connection.ErrorMessage}")
        End If
    End Sub

    Public Function Update(security As Security) As Boolean
        _connection.Prepare("UPDATE security SET attempt_count = @attemptCount, lock_until = @lockUntil WHERE mac_address = @macAddress")
        _connection.AddParam("@attemptCount", security.AttemptCount)
        _connection.AddParam("@lockUntil", security.LockUntil)
        _connection.AddParam("@macAddress", security.MacAddress)
        _connection.Execute()
        If _connection.HasError Then
            Debug.WriteLine($"[SecurityRepository] Update Error: {_connection.ErrorMessage}")
            Return False
        End If
        Return _connection.HasChanges
    End Function

    Public Function Delete(security As Security) As Boolean
        _connection.Prepare("DELETE security WHERE mac_address = @mac_address")
        _connection.AddParam("@mac_address", security.MacAddress)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[SecurityRepository] ClearLock Error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasChanges
    End Function
End Class