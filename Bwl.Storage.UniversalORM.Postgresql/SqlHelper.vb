Imports Npgsql

Public Class SqlHelper
    Public Property Sql As String
    Public Property Parameters As List(Of NpgsqlParameter)

    Public Sub New(sql As String, Optional parameters As List(Of NpgsqlParameter) = Nothing)
        Me.Sql = sql
        Me.Parameters = parameters
    End Sub
End Class
