Imports System.Security.Cryptography
Imports System.Text

Public Class PasswordHasher
    ''' <summary>
    ''' Hashes a password using SHA-256.
    ''' </summary>
    ''' <param name="password">The password to hash.</param>
    ''' <returns>The hashed password as a hexadecimal string.</returns>
    Public Shared Function HashPassword(password As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            ' Convert the password to a byte array
            Dim passwordBytes As Byte() = Encoding.UTF8.GetBytes(password)

            ' Compute the hash
            Dim hashBytes As Byte() = sha256.ComputeHash(passwordBytes)

            ' Convert the hash to a hexadecimal string
            Dim stringBuilder As New StringBuilder()
            For Each b As Byte In hashBytes
                stringBuilder.Append(b.ToString("x2")) ' "x2" formats each byte as a 2-digit hexadecimal value
            Next

            Return stringBuilder.ToString()
        End Using
    End Function

    ''' <summary>
    ''' Verifies a password against a hashed password.
    ''' </summary>
    ''' <param name="password">The password to verify.</param>
    ''' <param name="hashedPassword">The hashed password to compare against.</param>
    ''' <returns>True if the password matches the hashed password; otherwise, False.</returns>
    Public Shared Function VerifyPassword(password As String, hashedPassword As String) As Boolean
        ' Hash the input password
        Dim inputHash As String = HashPassword(password)

        ' Compare the hashed input password with the stored hashed password
        Return String.Equals(inputHash, hashedPassword, StringComparison.OrdinalIgnoreCase)
    End Function
End Class