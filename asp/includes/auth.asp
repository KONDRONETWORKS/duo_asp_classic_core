<%
Option Explicit
<!--#include file="duo_auth.asp" -->

Const AUTH_SOURCE = "file" ' "file" or "memory"
Const USERS_FILE_RELATIVE = "data/users.txt"

' AJOUTEZ cette fonction manquante
Function IsDuoAuthenticated()
    ' Vérifie si l'utilisateur a complété l'authentification Duo
    IsDuoAuthenticated = (Session("DuoAuthenticated") = "true")
End Function

Function GetUsersDictionary()
	Dim source
	source = LCase(AUTH_SOURCE)
	If source = "file" Then
		Set GetUsersDictionary = ReadUsersFromFile(Server.MapPath(USERS_FILE_RELATIVE))
	Else
		Set GetUsersDictionary = GetMemoryUsers()
	End If
End Function

Function GetMemoryUsers()
	Dim dict
	Set dict = CreateObject("Scripting.Dictionary")
	dict.CompareMode = 1 ' TextCompare (case-insensitive keys)
	dict("admin") = "password123"
	dict("user") = "user123"
	Set GetMemoryUsers = dict
End Function

Function ReadUsersFromFile(filePath)
	Dim dict, fso, fh, line, pos, user, pass
	Set dict = CreateObject("Scripting.Dictionary")
	dict.CompareMode = 1 ' TextCompare (case-insensitive keys)

	Set fso = CreateObject("Scripting.FileSystemObject")
	If Not fso.FileExists(filePath) Then
		Set ReadUsersFromFile = dict
		Exit Function
	End If

	Set fh = fso.OpenTextFile(filePath, 1, False)
	On Error Resume Next
	Do While Not fh.AtEndOfStream
		line = Trim(fh.ReadLine)
		If Len(line) > 0 Then
			If Left(line, 1) <> "#" Then
				pos = InStr(line, ":")
				If pos = 0 Then pos = InStr(line, ",")
				If pos > 0 Then
					user = Trim(Left(line, pos - 1))
					pass = Trim(Mid(line, pos + 1))
					If Len(user) > 0 Then dict(user) = pass
				End If
			End If
		End If
	Loop
	fh.Close
	On Error GoTo 0

	Set ReadUsersFromFile = dict
End Function

Function ValidateCredentials(username, password)
	Dim dict, expected
	ValidateCredentials = False
	If Len(username) = 0 Then Exit Function
	Set dict = GetUsersDictionary()
	If dict.Exists(username) Then
		expected = CStr(dict(username))
		If CStr(password) = expected Then
			ValidateCredentials = True
		End If
	End If
End Function

Sub SignIn(username)
	Session("auth_user") = username
End Sub

Sub SignOut()
	Session.Abandon
End Sub

Function IsAuthenticated()
	IsAuthenticated = (Len(CStr(Session("auth_user"))) > 0)
End Function

Function CurrentUsername()
	CurrentUsername = CStr(Session("auth_user"))
End Function

Sub RequireAuth()
	If Not IsAuthenticated() Then
		Dim returnUrl
		returnUrl = Request.ServerVariables("URL")
		Response.Redirect "login.asp?returnUrl=" & Server.URLEncode(returnUrl)
		Exit Sub
	End If
	
	' Check if Duo 2FA is required and completed
	If Not IsDuoAuthenticated() Then
		Dim duoReturnUrl
		duoReturnUrl = Request.ServerVariables("URL")
		Response.Redirect "duo_auth.asp?returnUrl=" & Server.URLEncode(duoReturnUrl)
	End If
End Sub
%>