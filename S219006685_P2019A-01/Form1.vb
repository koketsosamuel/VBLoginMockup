' *********************************************************************************
' Surname, Initials: Maotomabe, K S
' Student Number: 219006685
' Practical: P2019A-01
' Class name: frmLogin
' *********************************************************************************


'enforcing variable declaration and explicit conversions
Option Explicit On
Option Strict On

Public Class frmLogin

    'My variable Declarations
    Private username As Double
    Private password As Double
    Private response As String
    Private correctPassword As Double


    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click

        'Assigning text values to their variables and converting to correct data types
        username = CDbl(txtUsername.Text)
        password = CDbl(txtPassword.Text)
        txtResponse.Text = response

        'setting up the correct passsword to be half the username
        correctPassword = username / 2

        'Checking if the username has a length = 6 and the correct password
        If Len(username.ToString) = 6 And password = correctPassword Then

            response = "Access Granted"

        Else

            response = "program will be locked down"

        End If

    End Sub

End Class
