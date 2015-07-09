Imports System.Data.SqlClient

Public Class SqlHelper
	Public Property SQL As String
	Public Property Parameters As List(Of SqlParameter)

	Public Sub New(sql As String, Optional parameters As List(Of SqlParameter) = Nothing)
		Me.SQL = sql
		Me.Parameters = parameters
	End Sub
End Class
