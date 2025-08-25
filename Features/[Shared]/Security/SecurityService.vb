Imports System.Net.NetworkInformation

#Disable Warning
Public Class SecurityService
    Implements ISecurityService

    Private ReadOnly _securityRepository As SecurityRepository
    Private Const MaxAttempts As Integer = 5
    Private Const LockMinutes As Integer = 1

    Public Sub New(repository As SecurityRepository)
        _securityRepository = repository
    End Sub

    Public Async Function SecurityCheck() As Task(Of Boolean) Implements ISecurityService.SecurityCheck
        Try
            Dim security = _securityRepository.GetByMacAddress(New Security With {.MacAddress = GetMacAddress()})

            If security Is Nothing Then
                Return True
            End If

            If security.LockUntil IsNot Nothing AndAlso DateTime.Now > security.LockUntil Then
                ResetAttempts()
                Return True
            End If

                If security.LockUntil IsNot Nothing AndAlso DateTime.Now < security.LockUntil Then
                Return False
            End If

            If security.AttemptCount >= MaxAttempts Then
                If security.LockUntil Is Nothing Then
                    security.LockUntil = DateTime.Now.AddMinutes(LockMinutes)
                    _securityRepository.Update(security)
                End If
                Return False
            End If

            Return True
        Catch ex As Exception
            Debug.WriteLine($"[SecurityService] SecurityCheck Error: {ex.Message}")
            Return True
        End Try
    End Function

    Public Sub RecordAttempts() Implements ISecurityService.RecordAttempts
        Dim _security = _securityRepository.GetByMacAddress(New Security With {.MacAddress = GetMacAddress()})

        If _security Is Nothing Then
            _security = New Security With {
                .AttemptCount = 1,
                .MacAddress = GetMacAddress(),
                .LockUntil = Nothing
            }
            _securityRepository.Insert(_security)
        Else
            _security.AttemptCount += 1

            If _security.AttemptCount >= MaxAttempts Then
                ' Ensure lock is at least 10 minutes from now
                Dim minimumUnlockTime = DateTime.Now.AddMinutes(LockMinutes)

                ' If LockUntil is already set but not far enough in the future, extend it
                If _security.LockUntil Is Nothing OrElse _security.LockUntil < minimumUnlockTime Then
                    _security.LockUntil = minimumUnlockTime
                End If
            End If

            _securityRepository.Update(_security)
        End If
    End Sub

    Public Sub ResetAttempts() Implements ISecurityService.ResetAttempts
        Dim _security = _securityRepository.GetByMacAddress(New Security With {.MacAddress = GetMacAddress()
        })

        If _security IsNot Nothing Then
            _securityRepository.Delete(_security)
        End If
    End Sub

    Public Function GetLockUntil() As DateTime? Implements ISecurityService.GetLockUntil
        Dim _security = _securityRepository.GetByMacAddress(New Security With {.MacAddress = GetMacAddress()})
        Return If(_security IsNot Nothing, _security.LockUntil, Nothing)
    End Function
End Class
