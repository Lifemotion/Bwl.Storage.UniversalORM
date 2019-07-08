Imports Npgsql

Public Class PgBatchExecution

    Private ReadOnly _connection As NpgsqlConnection
    Public SqlStatements As New List(Of String)

    Public Sub New(conn As NpgsqlConnection)
        _connection = conn
    End Sub

    ''' <summary>
    ''' Executing commands. Results may vary
    ''' </summary>
    Public Sub Execute()
        Try
            If _connection IsNot Nothing AndAlso SqlStatements.Any() Then
                _connection.Open()
                Using cmd = New NpgsqlCommand(SqlStatements.Aggregate(Function(f, t) f + "; " + t), _connection)
                    cmd.ExecuteNonQuery()
                End Using
                _connection.Close()
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

End Class