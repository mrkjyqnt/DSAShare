Imports Microsoft.Data.SqlClient
Imports System.Data

''' <summary>
''' Connection class for database operations.
''' </summary>
Public Class Connection
    Public Connect As New SqlConnection("Data Source=192.168.8.10\SQLEXPRESS;Initial Catalog=dsa_share_database;User ID=member;Password=member;Trust Server Certificate=True") ' Connection injection
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
    ''' <param name="query">The SQL query to prepare.</param>
    Public Sub Prepare(query As String)
        CommandString = query
    End Sub

    ''' <summary>
    ''' Executes the prepared query.
    ''' </summary>
    ''' <returns>True if the query executed successfully; otherwise, False.</returns>
    Public Sub Execute()
        Try
            Query(CommandString)
        Catch ex As Exception
            HasError = True
            ErrorMessage = ex.Message
        End Try
    End Sub

    ' Open Connection with Checker
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

    ' Close Connection with Checker
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
    ''' Add parameters to the query.
    ''' </summary>
    ''' <param name="key">The parameter name.</param>
    ''' <param name="value">The parameter value.</param>
    Public Sub AddParam(ByVal key As String, ByVal value As Object)
        Try
            If value Is Nothing Then
                Parameters.Add(New SqlParameter(key, DBNull.Value))
            Else
                Parameters.Add(New SqlParameter(key, value))
            End If
        Catch ex As Exception
            HasError = True
            ErrorMessage = ex.Message
        End Try
    End Sub

    ''' <summary>
    ''' Query the database.
    ''' </summary>
    ''' <param name="command_query">The SQL query to execute.</param>
    Private Sub Query(ByVal command_query As String)
        ' Reset error state
        HasError = False
        HasChanges = Nothing
        ErrorMessage = String.Empty
        DataCount = 0
        HasRecord = False

        Try
            Open()
            Command = New SqlCommand(command_query, Connect)

            ' Add parameters to the command
            If Parameters.Count > 0 Then
                For Each param As SqlParameter In Parameters
                    Command.Parameters.Add(param)
                Next
                Parameters.Clear()
            End If

            ' Execute the query based on its type
            If command_query.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) OrElse
               command_query.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) OrElse
               command_query.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase) Then

                Dim rowsAffected As Integer = Command.ExecuteNonQuery()
                HasChanges = (rowsAffected > 0)

            ElseIf command_query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) Then
                Dim adapter As New SqlDataAdapter(Command)
                Data = New DataSet()
                DataCount = adapter.Fill(Data)

                If DataCount > 0 Then
                    DataRow = Data.Tables(0).Rows(0)
                    HasRecord = True
                End If
            End If

        Catch ex As Exception
            HasError = True
            ErrorMessage = ex.Message
        Finally
            Close()
        End Try
    End Sub

    ''' <summary>
    ''' Tests the database connection.
    ''' </summary>
    ''' <returns>True if the connection is successful; otherwise, False.</returns>
    Public Function TestConnection() As Boolean
        Try
            ' Open the connection
            If Connect.State = ConnectionState.Closed Then
                Connect.Open()
            End If

            ' Close the connection
            If Connect.State = ConnectionState.Open Then
                Connect.Close()
            End If

            ' Connection successful
            Return True
        Catch ex As Exception
            ' Connection failed
            HasError = True
            ErrorMessage = ex.Message
            Return False
        End Try
    End Function
End Class