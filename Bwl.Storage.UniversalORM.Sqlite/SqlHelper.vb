Imports System.Data.SQLite

Public Class SqlHelper
    Public Property Sql As String
    Public Property Parameters As List(Of SQLiteParameter)

    Public Sub New(sql As String, Optional parameters As List(Of SQLiteParameter) = Nothing)
        Me.Sql = sql
        Me.Parameters = parameters
    End Sub
End Class
