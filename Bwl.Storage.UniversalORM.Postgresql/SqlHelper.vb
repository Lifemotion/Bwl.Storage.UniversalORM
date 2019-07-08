Imports Npgsql

Public Class SqlHelper
    Public Property SQL As String
    Public Property Parameters As List(Of NpgsqlParameter)

    Public Sub New(sql As String, Optional parameters As List(Of NpgsqlParameter) = Nothing)
        Me.SQL = sql
        Me.Parameters = parameters
    End Sub
End Class
