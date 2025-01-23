Imports System.Data
Imports Microsoft.Data.Sqlite
Imports System.Threading

Public Class SqliteUtils

    ''' <summary>
    ''' Выполнение SQL-запроса без результата
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="sql">SQL-запрос</param>
    ''' <param name="parameters">Параметры запроса</param>
    ''' <param name="longTask">Задача может выполняться долго (ОПАСНО! Выполнение без таймаута!)</param>
    Public Shared Sub ExecSql(connString As String, sql As String, Optional parameters As SqliteParameter() = Nothing, Optional longTask As Boolean = False)
        Using con = New SqliteConnection(connString)
            con.Open()
            OptimizeSQLite(con)
            Using proc = con.BeginTransaction()
                Try
                    Using cmd = con.CreateCommand()
                        cmd.Transaction = proc

                        ' Для больших и долгих задач лучше всего использовать память, чтобы избежать ошибок доступа
                        If longTask Then
                            cmd.CommandText = "PRAGMA temp_store = MEMORY;"
                            cmd.ExecuteNonQuery()
                        End If

                        If parameters IsNot Nothing Then cmd.Parameters.AddRange(parameters.AsEnumerable())
                        cmd.CommandText = $"{sql};"
                        If longTask Then cmd.CommandTimeout = 0 ' Опасно! 0 означает что задача может выполняться бесконечно! 
                        cmd.ExecuteNonQuery()

                        ' После долгой задачи возвращаем дефолты
                        If longTask Then
                            cmd.CommandTimeout = 30
                            cmd.Parameters.Clear()
                            cmd.CommandText = "PRAGMA temp_store = DEFAULT;"
                            cmd.ExecuteNonQuery()
                        End If

                    End Using
                    proc.Commit()
                Catch ex As Exception
                    proc.Rollback()
                    Dim readableParams = If(parameters IsNot Nothing AndAlso parameters.Any(),
                                             parameters.Select(Function(f) $"{f.ParameterName}, type {f.DbType}, value {f.Value}").Aggregate(Function(f, t) f + vbNewLine + t),
                                            "None")
                    Throw New Exception($"SqliteUtils.ExecSQL. Connection string: {connString}{vbNewLine}SQL: {sql}{vbNewLine} Params: {readableParams}{vbNewLine} ERR: {ex.ToString()}")
                End Try
            End Using
        End Using
    End Sub

    Private Shared Sub OptimizeSQLite(connection As SqliteConnection)
        Using cmd = connection.CreateCommand()
            cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;"
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' Выполнение SQL и получение объекта
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="sql">SQL-запрос</param>
    ''' <param name="parameters">Параметры запроса</param>
    ''' <returns>Объект</returns>
    Public Shared Function ExecSqlScalar(connString As String, sql As String, Optional parameters As SqliteParameter() = Nothing) As Object
        Using con = New SqliteConnection(connString)
            con.Open()
            OptimizeSQLite(con)
            Try
                Dim result As Object
                Using cmd = con.CreateCommand()
                    If parameters IsNot Nothing Then
                        cmd.Parameters.AddRange(parameters.AsEnumerable())
                    End If
                    cmd.CommandText = sql
                    result = cmd.ExecuteScalar()
                End Using
                Return result
            Catch ex As Exception
                Dim readableParams = If(parameters IsNot Nothing AndAlso parameters.Any(),
                                        parameters.Select(Function(f) $"{f.ParameterName}, type {f.DbType}, value {f.Value}").Aggregate(Function(f, t) f + vbNewLine + t),
                                        "None")
                Throw New Exception($"SqliteUtils.ExecSQLScalar. Connection string: {connString}{vbNewLine}SQL: {sql}{vbNewLine} Params: {readableParams}{vbNewLine} ERR: {ex.ToString()}")
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Создание БД (прим. - база создаётся автоматически при попытке подключения, если её не существует)
    ''' </summary>
    ''' <param name="connStringBld">Строка подключения</param>
    Public Shared Sub CreateDb(connStringBld As SqliteConnectionStringBuilder)
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
        Using con = New SqliteConnection(connString)
            con.Open()
            OptimizeSQLite(con)
            Try
                Return con.State = ConnectionState.Open
            Catch ex As Exception
                Return False
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Выполняет SQL запрос и возвращает его результаты в виде списка, содержащего список полей объектов
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="sql">SQL-запрос</param>
    ''' <param name="parameters">Параметры запроса</param>
    ''' <returns>Список объектов</returns>
    Public Shared Function GetObjectList(connString As String, sql As String, Optional parameters As SqliteParameter() = Nothing) As List(Of List(Of Object))
        Using con = New SqliteConnection(connString)
            con.Open()
            OptimizeSQLite(con)
            Try
                Dim result As List(Of List(Of Object))
                Using cmd = con.CreateCommand()
                    If parameters IsNot Nothing AndAlso parameters.Any() Then
                        cmd.Parameters.AddRange(parameters.AsEnumerable())
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
                Dim readableParams = If(parameters IsNot Nothing AndAlso parameters.Any(),
                                        parameters.Select(Function(f) $"{f.ParameterName}, type {f.DbType}, value {f.Value}").Aggregate(Function(f, t) f + vbNewLine + t),
                                        "None")
                Throw New Exception($"SqliteUtils.GetObjectList. Connection string: {connString}{vbNewLine}SQL: {sql}{vbNewLine} Params: {readableParams}{vbNewLine} ERR: {ex.ToString()}")
            End Try
        End Using
    End Function

End Class