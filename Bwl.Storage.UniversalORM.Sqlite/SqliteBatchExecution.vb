Imports System.Data.SQLite

Public Class SqliteBatchExecution

    Private ReadOnly _connection As SQLiteConnection
    Public SqlStatements As New List(Of String)

    Public Sub New(conn As SQLiteConnection)
        _connection = conn
    End Sub

    ''' <summary>
    ''' Executing commands. Results may vary
    ''' </summary>
    Public Sub Execute()
        Try
            If _connection IsNot Nothing AndAlso SqlStatements.Any() Then
                _connection.Open()
                Using cmd = New SQLiteCommand(SqlStatements.Aggregate(Function(f, t) f + "; " + t), _connection)
                    cmd.ExecuteNonQuery()
                End Using
                _connection.Close()
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

End Class