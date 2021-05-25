'Imports System.Data.SQLite

'Public Class SqliteBatchExecution

'    Public SqlStatements As New List(Of String)

'    Public Sub New()
'    End Sub

'    ''' <summary>
'    ''' Executing commands. Results may vary
'    ''' </summary>
'    Public Sub Execute(connString As String)
'        Try

'            If SqlStatements.Any() Then
'                Using con As New SQLiteConnection(connString)
'                    Using cmd = New SQLiteCommand(SqlStatements.Aggregate(Function(f, t) f + "; " + t), con)
'                        cmd.ExecuteNonQuery()
'                    End Using
'                End Using
'            End If
'        Catch ex As Exception
'            Throw
'        End Try
'    End Sub

'End Class