Imports FirebirdSql.Data.FirebirdClient

Public Class SqlHelper
	Public Property SQL As String
	Public Property Parameters As List(Of FbParameter)

	Public Sub New(sql As String, Optional parameters As List(Of FbParameter) = Nothing)
		Me.SQL = sql
		Me.Parameters = parameters
	End Sub
End Class
