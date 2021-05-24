Imports System.Data.SQLite
Imports System.Threading

Public Class SqliteUtils

    ''' <summary>
    ''' Пустой объект для синхронизации потоков
    ''' </summary>
    Private Shared ReadOnly SyncObj As New Object

    ''' <summary>
    ''' Выполнение SQL-запроса без результата
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="sql">SQL-запрос</param>
    ''' <param name="parameters">Параметры запроса</param>
    Public Shared Sub ExecSql(connString As String, sql As String, Optional parameters As SQLiteParameter() = Nothing)
        SyncLock SyncObj
            Dim con = New SQLiteConnection(connString)
            Try
                con.Open()

                Dim cmd = New SQLiteCommand(sql, con)
                If parameters IsNot Nothing Then
                    Dim params As IEnumerable(Of SQLiteParameter)
                    ' ???
                    params = parameters
                    cmd.Parameters.AddRange(params.ToArray())
                End If
                cmd.ExecuteNonQuery()
                cmd.Dispose()
            Catch ex As Exception
                Throw New Exception(String.Format("SqliteUtils.ExecSQL({0}, {1}) - {2})", connString, ex.Message, sql))
            Finally
                con.Close()
                con.Dispose()
            End Try
        End SyncLock
    End Sub

    ''' <summary>
    ''' Выполнение SQL и получение объекта
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="sql">SQL-запрос</param>
    ''' <param name="parameters">Параметры запроса</param>
    ''' <returns>Объект</returns>
    Public Shared Function ExecSqlScalar(connString As String, sql As String, Optional parameters As SQLiteParameter() = Nothing) As Object
        Dim res As Object
        Dim con = New SQLiteConnection(connString)
        Try
            con.Open()
            Dim cmd = New SQLiteCommand(sql, con)
            If parameters IsNot Nothing Then
                Dim params As IEnumerable(Of SQLiteParameter)
                ' ???
                params = parameters
                cmd.Parameters.AddRange(params.ToArray())
            End If
            res = cmd.ExecuteScalar()
            cmd.Dispose()
        Catch ex As Exception
            Throw New Exception(String.Format("SqliteUtils.ExecSQLScalar({0}, {1}) - {2})", connString, sql, ex.Message))
        Finally
            con.Close()
            con.Dispose()
        End Try
        Return res
    End Function

    ''' <summary>
    ''' Создание БД (прим. - база создаётся автоматически при попытке подключения, если её не существует)
    ''' </summary>
    ''' <param name="connStringBld">Строка подключения</param>
    Public Shared Sub CreateDb(connStringBld As SQLiteConnectionStringBuilder)
        SyncLock SyncObj
            If (Not CheckConnection(connStringBld.ConnectionString)) Then
                Thread.Sleep(2000)
                If (Not CheckConnection(connStringBld.ConnectionString)) Then
                    Throw New Exception("Could not create SQLite database") ' База создаётся автоматически при обращении к ней
                End If
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Проверка существования таблицы
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="tableName">Имя таблицы</param>
    ''' <returns>Таблица существует</returns>
    Public Shared Function TableExists(connString As String, tableName As String) As Boolean
        Dim res As Boolean
        SyncLock SyncObj
            Try
                Dim sql = String.Format("SELECT COUNT(name) FROM sqlite_master WHERE type='table' AND name='{0}';", tableName)
                res = CType(ExecSqlScalar(connString, sql), Integer) > 0
            Catch ex As Exception
                res = False
            End Try
        End SyncLock
        Return res
    End Function

    ''' <summary>
    ''' Проверка соединения с БД
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <returns>Соединение ОК</returns>
    Public Shared Function CheckConnection(connString As String) As Boolean
        Dim res As Boolean
        SyncLock SyncObj
            Try
                Dim con = New SQLiteConnection(connString)
                con.Open()
                res = con.State = ConnectionState.Open
                con.Close()
                con.Dispose()
            Catch ex As Exception
                res = False
            End Try
        End SyncLock
        Return res
    End Function

    ''' <summary>
    ''' Выполняет SQL запрос и возвращает его результаты в виде списка, содержащего список полей объектов
    ''' </summary>
    ''' <param name="connString">Строка подключения</param>
    ''' <param name="sql">SQL-запрос</param>
    ''' <param name="parameters">Параметры запроса</param>
    ''' <returns>Список объектов</returns>
    Public Shared Function GetObjectList(connString As String, sql As String, Optional parameters As SQLiteParameter() = Nothing) As List(Of List(Of Object))
        Dim list As List(Of List(Of Object))
        Dim reader As SqlReaderHelper = Nothing
        SyncLock SyncObj
            Try
                Dim con = New SQLiteConnection(connString)
                con.Open()
                Dim cmd = New SQLiteCommand(sql, con)
                If parameters IsNot Nothing Then
                    Dim params As IEnumerable(Of SQLiteParameter)
                    params = parameters
                    cmd.Parameters.AddRange(params.ToArray())
                End If
                Dim sr = cmd.ExecuteReader()
                reader = New SqlReaderHelper(sr, cmd, con)
                list = reader.GetObjectList
            Catch ex As Exception
                Throw New Exception(String.Format("SqliteUtils.ExecSQL({0}, {1}) - {2})", connString, sql, ex.ToString))
            Finally
                If reader IsNot Nothing Then
                    reader.Close()
                End If
            End Try
        End SyncLock
        Return list
    End Function

End Class


