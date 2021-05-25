Imports System.Data.SQLite
Imports System.Threading

Public Class SqliteUtils

    ''' <summary>
    ''' Выполнение SQL-запроса без результата
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="sql">SQL-запрос</param>
    ''' <param name="parameters">Параметры запроса</param>
    ''' <param name="veryTimeConsumingTask">Задача может выполняться долго (ОПАСНО! Выполнение без таймаута!)</param>
    Public Shared Sub ExecSql(connString As String, sql As String, Optional parameters As SQLiteParameter() = Nothing, Optional veryTimeConsumingTask As Boolean = False)
        Using con = New SQLiteConnection(connString)
            con.Open()
            Using proc = con.BeginTransaction()
                Try
                    Using cmd = New SQLiteCommand(con)
                        cmd.Transaction = proc

                        cmd.CommandText = "PRAGMA temp_store = MEMORY;"
                        cmd.ExecuteNonQuery()

                        If parameters IsNot Nothing Then
                            cmd.Parameters.AddRange(parameters.ToArray())
                        End If
                        cmd.CommandText = $"{sql};"
                        If veryTimeConsumingTask Then
                            cmd.CommandTimeout = 0 ' Опасно! 0 означает что задача может выполняться бесконечно! 
                        End If
                        cmd.ExecuteNonQuery()

                        If veryTimeConsumingTask Then
                            cmd.Parameters.Clear()
                            cmd.CommandTimeout = 30
                            cmd.CommandText = "PRAGMA temp_store = DEFAULT;"
                            cmd.ExecuteNonQuery()
                        End If
                    End Using
                    proc.Commit()
                Catch ex As Exception
                    proc.Rollback()
                    Throw New Exception(String.Format("SqliteUtils.ExecSQL({0}, {1}) - {2})", connString, ex.ToString(), sql))
                End Try
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Выполнение SQL и получение объекта
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="sql">SQL-запрос</param>
    ''' <param name="parameters">Параметры запроса</param>
    ''' <returns>Объект</returns>
    Public Shared Function ExecSqlScalar(connString As String, sql As String, Optional parameters As SQLiteParameter() = Nothing) As Object
        Using con = New SQLiteConnection(connString)
            con.Open()
            Try
                Dim result As Object
                Using cmd = New SQLiteCommand(con)
                    If parameters IsNot Nothing Then
                        cmd.Parameters.AddRange(parameters.ToArray())
                    End If
                    cmd.CommandText = sql
                    result = cmd.ExecuteScalar()
                End Using
                Return result
            Catch ex As Exception
                Throw New Exception(String.Format("SqliteUtils.ExecSQLScalar({0}, {1}) - {2})", connString, sql, ex.ToString))
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Создание БД (прим. - база создаётся автоматически при попытке подключения, если её не существует)
    ''' </summary>
    ''' <param name="connStringBld">Строка подключения</param>
    Public Shared Sub CreateDb(connStringBld As SQLiteConnectionStringBuilder)
        If (Not CheckConnection(connStringBld.ConnectionString)) Then
            Thread.Sleep(2000)
            If (Not CheckConnection(connStringBld.ConnectionString)) Then
                Throw New Exception("Could not create SQLite database") ' База создаётся автоматически при обращении к ней
            End If
        End If
    End Sub

    ''' <summary>
    ''' Проверка существования таблицы
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="tableName">Имя таблицы</param>
    ''' <returns>Таблица существует</returns>
    Public Shared Function TableExists(connString As String, tableName As String) As Boolean
        Try
            Dim sql = String.Format("SELECT COUNT(name) FROM sqlite_master WHERE type='table' AND name='{0}';", tableName)
            Return CType(ExecSqlScalar(connString, sql), Integer) > 0
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Проверка соединения с БД
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <returns>Соединение ОК</returns>
    Private Shared Function CheckConnection(connString As String) As Boolean
        Try
            Using con = New SQLiteConnection(connString)
                con.Open()
                Return con.State = ConnectionState.Open
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Выполняет SQL запрос и возвращает его результаты в виде списка, содержащего список полей объектов
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="sql">SQL-запрос</param>
    ''' <param name="parameters">Параметры запроса</param>
    ''' <returns>Список объектов</returns>
    Public Shared Function GetObjectList(connString As String, sql As String, Optional parameters As SQLiteParameter() = Nothing) As List(Of List(Of Object))
        Using con = New SQLiteConnection(connString)
            con.Open()
            Try
                Dim result As List(Of List(Of Object))
                Using cmd = New SQLiteCommand(con)
                    If parameters IsNot Nothing AndAlso parameters.Any() Then
                        cmd.Parameters.AddRange(parameters.ToArray())
                    End If
                    cmd.CommandText = sql
                    Using sr = cmd.ExecuteReader()
                        Dim reader = New SqlReaderHelper(sr, cmd, con)
                        result = reader.GetObjectList()
                        reader.Close()
                    End Using
                End Using
                Return result
            Catch ex As Exception
                Throw New Exception(String.Format("SqliteUtils.ExecSQL({0}, {1}) - {2})", connString, sql, ex.ToString))
            End Try
        End Using
    End Function

End Class