<!--#include file="includes/auth.asp" -->
<!--#include file="includes/duo_auth.asp" -->
<%
Response.Buffer = True
ClearDuoAuth
SignOut
Response.Redirect "login.asp"
%> 