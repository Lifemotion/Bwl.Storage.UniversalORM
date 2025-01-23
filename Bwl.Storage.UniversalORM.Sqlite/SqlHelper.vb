Imports Microsoft.Data.Sqlite

Public Class SqlHelper
    Public Property Sql As String
    Public Property Parameters As List(Of SqliteParameter)

    Public Sub New(sql As String, Optional parameters As List(Of SqliteParameter) = Nothing)
        Me.Sql = sql
        Me.Parameters = parameters
    End Sub
End Class
