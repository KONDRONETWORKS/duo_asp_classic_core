<%
' Duo Web SDK v4 Integration for Classic ASP
' Following Duo's recommended flow: state generation, request URL, callback handling, token exchange

' Configuration - Replace with your Duo credentials
Dim DUO_CLIENT_ID, DUO_CLIENT_SECRET, DUO_API_HOST, DUO_REDIRECT_URI

' Initialize Duo configuration
DUO_CLIENT_ID = "DIQ8BOPVUCELV4V4S4H0"
DUO_CLIENT_SECRET = "G702NyF96D1eKge9AbdrVJOb7o7IVJ19B6xh0E5I"
DUO_API_HOST = "api-a322db2c.duosecurity.com"
DUO_REDIRECT_URI = "http://localhost:5000/Callback"

' Generate a random state value for CSRF protection
Function GenerateDuoState()
    Dim state, i
    Randomize
    state = ""
    For i = 1 To 32
        state = state & Chr(Int(Rnd * 26) + 97)
    Next
    GenerateDuoState = state
End Function

' Generate Duo authorization URL
Function GenerateDuoAuthURL(username, state)
    Dim authURL
    
    authURL = "https://" & DUO_API_HOST & "/oauth/v1/authorize?"
    authURL = authURL & "client_id=" & Server.URLEncode(DUO_CLIENT_ID)
    authURL = authURL & "&redirect_uri=" & Server.URLEncode(DUO_REDIRECT_URI)
    authURL = authURL & "&response_type=code"
    authURL = authURL & "&scope=openid"
    authURL = authURL & "&request=" & Server.URLEncode(state)
    If Len(username) > 0 Then
        authURL = authURL & "&login_hint=" & Server.URLEncode(username)
    End If
    GenerateDuoAuthURL = authURL
End Function



' Exchange authorization code for token
Function ExchangeCodeForToken(authCode)
    Dim httpRequest, postData, responseText
    Dim tokenInfo
    
    Set httpRequest = CreateObject("MSXML2.XMLHTTP")
    
    postData = "grant_type=authorization_code"
    postData = postData & "&client_id=" & Server.URLEncode(DUO_CLIENT_ID)
    postData = postData & "&client_secret=" & Server.URLEncode(DUO_CLIENT_SECRET)
    postData = postData & "&code=" & Server.URLEncode(authCode)
    postData = postData & "&redirect_uri=" & Server.URLEncode(DUO_REDIRECT_URI)
    
    httpRequest.Open "POST", "https://" & DUO_API_HOST & "/oauth/v1/token", False
    httpRequest.setRequestHeader "Content-Type", "application/x-www-form-urlencoded"
    httpRequest.setRequestHeader "User-Agent", "Duo-ASP-Classic/1.0"
    
    On Error Resume Next
    httpRequest.Send postData
    
    If Err.Number = 0 Then
        responseText = httpRequest.responseText
        ' Parse JSON response (simplified)
        If InStr(responseText, "access_token") > 0 Then
            tokenInfo = "SUCCESS:" & responseText
        Else
            tokenInfo = "ERROR:" & responseText
        End If
    Else
        tokenInfo = "ERROR:HTTP Request failed - " & Err.Description
    End If
    On Error GoTo 0
    
    httpRequest = Nothing
    ExchangeCodeForToken = tokenInfo
End Function

' Validate Duo authentication
Function ValidateDuoAuth(username, request)
    Dim storedRequest, isValid
    isValid = False
    
    ' Get stored request from session
    storedRequest = Session("duo_request")
    
    ' Validate request parameter
    If Len(storedRequest) > 0 And storedRequest = request Then
        ' Request is valid, clear it from session
        Session("duo_request") = ""
        isValid = True
    End If
    
    ValidateDuoAuth = isValid
End Function

' Store Duo request in session
Sub StoreDuoRequest(request)
    Session("duo_request") = request
End Sub

' Check if user has completed Duo 2FA
Function IsDuoAuthenticated()
    IsDuoAuthenticated = (Len(Session("duo_authenticated")) > 0)
End Function

' Mark user as Duo authenticated
Sub MarkDuoAuthenticated()
    Session("duo_authenticated") = "true"
End Sub

' Clear Duo authentication
Sub ClearDuoAuth()
    Session("duo_authenticated") = ""
    Session("duo_request") = ""
End Sub
%> 