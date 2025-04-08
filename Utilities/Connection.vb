Imports Microsoft.Data.SqlClient
Imports System.Data

''' <summary>
''' Connection class for database operations.
''' </summary>
Public Class Connection
    Private Config = ConfigurationModule.GetSettings()
    Public ConnectString As String = $"Data Source={Config.Database.Server};Initial Catalog={Config.Database.Name};User ID={Config.Database.Username};Password={Config.Database.Password};Trust Server Certificate=True;MultipleActiveResultSets=True"

    Public Connect As New SqlConnection(ConnectString)
    Public Command As New SqlCommand
    Public CommandString As String
    Public Parameters As New List(Of SqlParameter)
    Public Data As DataSet
    Public DataRow As DataRow
    Public DataCount As Integer
    Public HasError As Boolean
    Public HasRecord As Boolean
    Public HasChanges As Boolean
    Public ErrorMessage As String

    ''' <summary>
    ''' Prepares the query for execution.
    ''' </summary>
    Public Sub Prepare(query As String)
        CommandString = query
    End Sub

    ''' <summary>
    ''' Executes the prepared query.
    ''' </summary>
    Public Sub Execute()
        Try
            Query(CommandString)
        Catch ex As Exception
            HasError = True
            ErrorMessage = ex.Message
        End Try
    End Sub

    ''' <summary>
    ''' Opens the connection safely.
    ''' </summary>
    Public Sub Open()
        Try
            If Connect.State = ConnectionState.Closed Then
                Connect.Open()
            End If
        Catch ex As Exception
            HasError = True
            ErrorMessage = ex.Message
        End Try
    End Sub

    ''' <summary>
    ''' Closes the connection safely.
    ''' </summary>
    Public Sub Close()
        Try
            If Connect.State = ConnectionState.Open Then
                Connect.Close()
            End If
        Catch ex As Exception
            HasError = True
            ErrorMessage = ex.Message
        End Try
    End Sub

    ''' <summary>
    ''' Adds a parameter to the query.
    ''' </summary>
    Public Sub AddParam(ByVal key As String, ByVal value As Object)
        Try
            Dim param As New SqlParameter(key, If(value IsNot Nothing, value, DBNull.Value))

            ' Prevent duplicate parameters
            If Not Parameters.Any(Function(p) p.ParameterName = key) Then
                Parameters.Add(param)
            End If

        Catch ex As Exception
            HasError = True
            ErrorMessage = ex.Message
        End Try
    End Sub

    ''' <summary>
    ''' Core method to run queries (SELECT/INSERT/UPDATE/DELETE).
    ''' </summary>
    Private Sub Query(ByVal command_query As String)
        HasError = False
        HasChanges = Nothing
        ErrorMessage = String.Empty
        DataCount = 0
        HasRecord = False

        Try
            Using conn As New SqlConnection(ConnectString)
                conn.Open()

                Using cmd As New SqlCommand(command_query, conn)
                    ' Add fresh parameter copies to avoid reuse issues
                    If Parameters.Count > 0 Then
                        cmd.Parameters.Clear()
                        For Each param As SqlParameter In Parameters
                            cmd.Parameters.Add(New SqlParameter(param.ParameterName, param.Value))
                        Next
                        Parameters.Clear()
                    End If

                    If command_query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) Then
                        Dim adapter As New SqlDataAdapter(cmd)
                        Data = New DataSet()
                        DataCount = adapter.Fill(Data)

                        If DataCount > 0 Then
                            ' Ensure that DataRow has data before accessing it
                            If Data.Tables(0).Rows.Count > 0 Then
                                DataRow = Data.Tables(0).Rows(0)
                                HasRecord = True
                            Else
                                HasError = True
                                ErrorMessage = "No data returned from SELECT query."
                            End If
                        Else
                            HasError = True
                            ErrorMessage = "No data found in the result set."
                        End If

                    ElseIf command_query.TrimStart().StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) OrElse
                           command_query.TrimStart().StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) OrElse
                           command_query.TrimStart().StartsWith("DELETE", StringComparison.OrdinalIgnoreCase) Then

                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                        HasChanges = (rowsAffected > 0)
                    End If
                End Using
            End Using

        Catch ex As Exception
            HasError = True
            ErrorMessage = ex.Message
        End Try
    End Sub


    ''' <summary>
    ''' Fetch all rows from last SELECT query.
    ''' </summary>
    Public Function FetchAll() As List(Of DataRow)
        Dim rows As New List(Of DataRow)()

        If Data IsNot Nothing AndAlso Data.Tables.Count > 0 Then
            For Each row As DataRow In Data.Tables(0).Rows
                rows.Add(row)
            Next
        End If

        Return rows
    End Function

    ''' <summary>
    ''' Tests if the connection is successful.
    ''' </summary>
    Public Function TestConnection() As Boolean
        Try
            Using conn As New SqlConnection(ConnectString)
                conn.Open()
                Return (conn.State = ConnectionState.Open)
            End Using
        Catch ex As Exception
            HasError = True
            ErrorMessage = ex.Message
            Return False
        End Try
    End Function
End Class
