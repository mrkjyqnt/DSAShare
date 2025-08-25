Imports System.Data

Public Class UsersRepository
    Private ReadOnly _connection As Connection

    Public Sub New(connection As Connection)
        _connection = connection
    End Sub

    ''' <summary>
    ''' Authenticates a user and logs them in.
    ''' </summary>
    Public Function Auth(user As Users) As Boolean
        _connection.Prepare("SELECT * FROM users WHERE username = @username AND password_hash = @password ORDER BY id DESC")
        _connection.AddParam("@username", user.Username)
        _connection.AddParam("@password", user.PasswordHash)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[UserRepository] Auth Error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasRecord
    End Function

    ''' <summary>
    ''' Reads a user by username.
    ''' </summary>
    Public Function GetByUsername(user As Users) As Users
        _connection.Prepare("SELECT * FROM users WHERE username = @username ORDER BY id DESC")
        _connection.AddParam("@username", user.Username)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[UserRepository] GetByUsername Error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return MapDataRowToUser(_connection.DataRow)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Reads a user by ID.
    ''' </summary>
    Public Function GetById(user As Users) As Users
        _connection.Prepare("SELECT * FROM users WHERE id = @id ORDER BY id DESC")
        _connection.AddParam("@id", user.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[UserRepository] GetById Error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        If _connection.HasRecord Then
            Return MapDataRowToUser(_connection.DataRow)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets all users.
    ''' </summary>
    Public Function Read() As List(Of Users)
        Dim usersList As New List(Of Users)()
        _connection.Prepare("SELECT * FROM users ORDER BY id DESC")
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[UserRepository] Read Error: {_connection.ErrorMessage}")
            Return Nothing
        End If

        For Each record As DataRow In _connection.FetchAll()
            usersList.Add(MapDataRowToUser(record))
        Next

        Return usersList
    End Function

    ''' <summary>
    ''' Inserts a new user into the database.
    ''' </summary>
    Public Function Insert(user As Users) As Boolean
        Try
            ' Check if username exists
            _connection.Prepare("SELECT * FROM users WHERE username = @username")
            _connection.AddParam("@username", user.Username)
            _connection.Execute()

            If _connection.HasRecord Then
                Debug.WriteLine($"[UserRepository] Username already exists: {user.Username}")
                Return False
            End If

            ' Insert the new user
            _connection.Prepare("INSERT INTO users " &
                               "(name, username, password_hash, security_question_1, security_question_2, security_question_3, " &
                               "security_answer_1, security_answer_2, security_answer_3, role, status, app_appearance, created_at) " &
                               "VALUES (@name, @username, @password, @sq1, @sq2, @sq3, @sa1, @sa2, @sa3, @role, @status, @appearance, @createdAt)")

            AddCommonUserParams(_connection, user)
            _connection.AddParam("@createdAt", DateTime.Now)

            _connection.Execute()

            If _connection.HasError Then
                Debug.WriteLine($"[UserRepository] Insert Error: {_connection.ErrorMessage}")
                Return False
            End If

            Return _connection.HasChanges

        Catch ex As Exception
            Debug.WriteLine($"[UserRepository] Insert Exception: {ex.Message}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Updates an existing user in the database.
    ''' </summary>
    Public Function Update(user As Users) As Boolean
        _connection.Prepare("SELECT * FROM users WHERE id = @id")
        _connection.AddParam("@id", user.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[UserRepository] Update Error: {_connection.ErrorMessage}")
            Return False
        End If

        If Not _connection.HasRecord Then
            Debug.WriteLine($"[UserRepository] User not found with ID: {user.Id}")
            Return False
        End If

        ' Update the user
        _connection.Prepare("UPDATE users SET " &
                           "name = @name, username = @username, password_hash = @password, " &
                           "security_question_1 = @sq1, security_question_2 = @sq2, security_question_3 = @sq3, " &
                           "security_answer_1 = @sa1, security_answer_2 = @sa2, security_answer_3 = @sa3, " &
                           "role = @role, status = @status, app_appearance = @appearance, created_at = @createdAt " &
                           "WHERE id = @id")

        AddCommonUserParams(_connection, user)
        _connection.AddParam("@createdAt", user.CreatedAt)
        _connection.AddParam("@id", user.Id)

        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[UserRepository] Update Error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasChanges
    End Function

    ''' <summary>
    ''' Deletes a user from the database.
    ''' </summary>
    Public Function Delete(user As Users) As Boolean
        _connection.Prepare("SELECT * FROM users WHERE id = @id")
        _connection.AddParam("@id", user.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[UserRepository] Delete Error: {_connection.ErrorMessage}")
            Return False
        End If

        If Not _connection.HasRecord Then
            Return False
        End If

        _connection.Prepare("DELETE FROM users WHERE id = @id")
        _connection.AddParam("@id", user.Id)
        _connection.Execute()

        If _connection.HasError Then
            Debug.WriteLine($"[UserRepository] Delete Error: {_connection.ErrorMessage}")
            Return False
        End If

        Return _connection.HasChanges
    End Function

    ''' <summary>
    ''' Verifies security questions for password recovery.
    ''' </summary>
    Public Function VerifySecurityQuestions(username As String, answers As Dictionary(Of Integer, String)) As Boolean
        _connection.Prepare("SELECT security_answer_1, security_answer_2, security_answer_3 " &
                           "FROM users WHERE username = @username")
        _connection.AddParam("@username", username)
        _connection.Execute()

        If _connection.HasError OrElse Not _connection.HasRecord Then
            Return False
        End If

        Dim answer1 = _connection.DataRow("security_answer_1").ToString()
        Dim answer2 = _connection.DataRow("security_answer_2").ToString()
        Dim answer3 = _connection.DataRow("security_answer_3").ToString()

        Return answers(1) = answer1 AndAlso answers(2) = answer2 AndAlso answers(3) = answer3
    End Function

    ''' <summary>
    ''' Gets security questions for a user.
    ''' </summary>
    Public Function GetSecurityQuestions(username As String) As Dictionary(Of Integer, String)
        _connection.Prepare("SELECT security_question_1, security_question_2, security_question_3 " &
                           "FROM users WHERE username = @username")
        _connection.AddParam("@username", username)
        _connection.Execute()

        If _connection.HasError OrElse Not _connection.HasRecord Then
            Return Nothing
        End If

        Return New Dictionary(Of Integer, String) From {
            {1, _connection.DataRow("security_question_1").ToString()},
            {2, _connection.DataRow("security_question_2").ToString()},
            {3, _connection.DataRow("security_question_3").ToString()}
        }
    End Function

    ''' <summary>
    ''' Updates user password.
    ''' </summary>
    Public Function UpdatePassword(username As String, newPasswordHash As String) As Boolean
        _connection.Prepare("UPDATE users SET password_hash = @password WHERE username = @username")
        _connection.AddParam("@password", newPasswordHash)
        _connection.AddParam("@username", username)
        _connection.Execute()

        Return Not _connection.HasError AndAlso _connection.HasChanges
    End Function

    Private Function MapDataRowToUser(row As DataRow) As Users
        Return New Users() With {
            .Id = row("id"),
            .Name = row("name").ToString(),
            .Username = row("username").ToString(),
            .PasswordHash = row("password_hash").ToString(),
            .SecurityQuestion1 = If(IsDBNull(row("security_question_1")), "", row("security_question_1").ToString()),
            .SecurityQuestion2 = If(IsDBNull(row("security_question_2")), "", row("security_question_2").ToString()),
            .SecurityQuestion3 = If(IsDBNull(row("security_question_3")), "", row("security_question_3").ToString()),
            .SecurityAnswer1 = If(IsDBNull(row("security_answer_1")), "", row("security_answer_1").ToString()),
            .SecurityAnswer2 = If(IsDBNull(row("security_answer_2")), "", row("security_answer_2").ToString()),
            .SecurityAnswer3 = If(IsDBNull(row("security_answer_3")), "", row("security_answer_3").ToString()),
            .Role = row("role").ToString(),
            .Status = row("status").ToString(),
            .AppAppearance = If(IsDBNull(row("app_appearance")), "", row("app_appearance").ToString()),
            .CreatedAt = row("created_at")
        }
    End Function

    Private Sub AddCommonUserParams(conn As Connection, user As Users)
        conn.AddParam("@name", user.Name)
        conn.AddParam("@username", user.Username)
        conn.AddParam("@password", user.PasswordHash)
        conn.AddParam("@sq1", user.SecurityQuestion1)
        conn.AddParam("@sq2", user.SecurityQuestion2)
        conn.AddParam("@sq3", user.SecurityQuestion3)
        conn.AddParam("@sa1", user.SecurityAnswer1)
        conn.AddParam("@sa2", user.SecurityAnswer2)
        conn.AddParam("@sa3", user.SecurityAnswer3)
        conn.AddParam("@role", user.Role)
        conn.AddParam("@status", user.Status)
        conn.AddParam("@appearance", user.AppAppearance)
    End Sub
End Class