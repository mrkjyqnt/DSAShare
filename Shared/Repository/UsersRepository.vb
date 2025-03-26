Imports System.Data

Public Class UsersRepository
    Private ReadOnly _connection As Connection

    Public Sub New(connection As Connection)
        _connection = connection
    End Sub

    ''' <summary>
    ''' Authenticates a user and logs them in.
    ''' </summary>
    ''' <param name="username"></param>
    ''' <param name="password"></param>
    ''' <returns></returns>
    Public Function Auth(user As Users) As Boolean
        _connection.Prepare("SELECT * 
                            FROM users 
                            WHERE username = @username AND password_hash = @password ORDER BY id DESC")
        _connection.AddParam("@username", user.Username)
        _connection.AddParam("@password", user.PasswordHash)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If

        If _connection.HasRecord Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Reads a user by username.
    ''' </summary>
    ''' <param name="username">The username to search for.</param>
    ''' <returns>A Users object if found; otherwise, Nothing.</returns>
    Public Function GetByUsername(user As Users) As Users
        _connection.Prepare("SELECT * 
                            FROM users 
                            WHERE username = @username ORDER BY id DESC")
        _connection.AddParam("@username", user.Username)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return New Users() With {
                .Id = _connection.DataRow("id"),
                .Username = _connection.DataRow("username").ToString(),
                .PasswordHash = _connection.DataRow("password_hash").ToString(),
                .Role = _connection.DataRow("role").ToString(),
                .Status = _connection.DataRow("status").ToString(),
                .CreatedAt = _connection.DataRow("created_at")
            }
        End If

        ' User not found
        Return Nothing
    End Function

    ''' <summary>
    ''' Reads a user by ID.
    ''' </summary>
    ''' <param name="id">The ID to search for.</param>
    ''' <returns>A Users object if found; otherwise, Nothing.</returns>
    Public Function Read() As List(Of Users)
        Dim usersList As New List(Of Users)()
        _connection.Prepare("SELECT * FROM users ORDER BY id DESC")
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return Nothing
        End If

        For Each record As DataRow In _connection.FetchAll()
            Dim user As New Users() With {
                .Id = record("id"),
                .Username = record("username").ToString(),
                .PasswordHash = record("password_hash").ToString(),
                .Role = record("role").ToString(),
                .Status = record("status").ToString(),
                .CreatedAt = record("created_at")
                }
                usersList.Add(user)
        Next

        Return usersList
    End Function

    ''' <summary>
    ''' Inserts a new user into the database.
    ''' </summary>
    ''' <param name="user">The Users object containing user data.</param>
    Public Function Insert(user As Users) As Boolean
        ' Check if the username already exists
        _connection.Prepare("SELECT * FROM users WHERE username = @username ORDER BY id DESC")
        _connection.AddParam("@username", user.Username)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If

        If _connection.HasRecord Then
            Return False
        End If

        ' Insert the new user
        _connection.Prepare("INSERT INTO users (name, username, password_hash, role, status, created_at) VALUES (@name, @username, @password, @role, @status, @createdAt)")
        
        _connection.AddParam("@name", user.Username)
        _connection.AddParam("@username", user.Username)
        _connection.AddParam("@password", user.PasswordHash)
        _connection.AddParam("@role", user.Role)
        _connection.AddParam("@status", user.Status)
        _connection.AddParam("@createdAt", DateTime.Now)

        _connection.Execute()

        Return _connection.HasChanges
    End Function

    ''' <summary>
    ''' Updates an existing user in the database.
    ''' </summary>
    ''' <param name="user">The Users object containing updated user data.</param>
    Public Function Update(user As Users) As Boolean
        _connection.Prepare("SELECT * FROM users WHERE username = @username ORDER BY id DESC")
        _connection.AddParam("@username", user.Username)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If

        If Not _connection.HasRecord Then
            Return False
        End If

        ' Update the user
        _connection.Prepare("UPDATE users SET username = @username, password_hash = @password, role = @role, status = @status WHERE id = @id")
        _connection.AddParam("@username", user.Username)
        _connection.AddParam("@password", HashPassword(user.PasswordHash))
        _connection.AddParam("@role", user.Role)
        _connection.AddParam("@status", user.Status)
        _connection.AddParam("@id", user.Id)
        _connection.Execute()

        If _connection.HasChanges Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Deletes a user from the database.
    ''' </summary>
    ''' <param name="userId">The ID of the user to delete.</param>
    Public Function Delete(user As Users)  As Boolean
        _connection.Prepare("SELECT * FROM users WHERE username = @username")
        _connection.AddParam("@username", user.Username)
        _connection.Execute()

        If _connection.HasError Then
            ErrorHandler.SetError(_connection.ErrorMessage)
            Return False
        End If

        If Not _connection.HasRecord Then
            Return False
        End If

        ' Delete the user
        _connection.Prepare("DELETE FROM users WHERE id = @id")
        _connection.AddParam("@id", user.Id)
        _connection.Execute()

        If _connection.HasChanges Then
            Return True
        Else
            Return False
        End If
    End Function
End Class