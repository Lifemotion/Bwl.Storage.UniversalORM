Imports Npgsql

Public Class PgUtils

    Public Shared Sub ExecSql(connString As String, sql As String, Optional parameters As NpgsqlParameter() = Nothing)
        Dim con = New NpgsqlConnection(connString)
        Try
            con.Open()

            Dim cmd = New NpgsqlCommand(sql, con)
            If parameters IsNot Nothing Then
                Dim params As IEnumerable(Of NpgsqlParameter)
                ' ???
                params = parameters
                cmd.Parameters.AddRange(params)
            End If
            cmd.ExecuteNonQuery()
            cmd.Dispose()
        Catch ex As Exception
            Throw New Exception(String.Format("PgUtils.ExecSQL({0}, {1}) - {2})", connString, ex.Message, sql))
        Finally
            con.Close()
            con.Dispose()
        End Try
    End Sub

    Public Shared Function ExecSqlScalar(connString As String, sql As String, Optional parameters As NpgsqlParameter() = Nothing) As Object
        Dim res As Object
        Dim con = New NpgsqlConnection(connString)
        Try
            con.Open()
            Dim cmd = New NpgsqlCommand(sql, con)
            If parameters IsNot Nothing Then
                Dim params As IEnumerable(Of NpgsqlParameter)
                ' ???
                params = parameters
                cmd.Parameters.AddRange(params)
            End If
            res = cmd.ExecuteScalar()
            cmd.Dispose()
        Catch ex As Exception
            Throw New Exception(String.Format("PgUtils.ExecSQLScalar({0}, {1}) - {2})", connString, sql, ex.Message))
        Finally
            con.Close()
            con.Dispose()
        End Try
        Return res
    End Function

    Public Shared Sub CreateDb(connStringBld As NpgsqlConnectionStringBuilder, dbName As String)
        If (Not CheckConnection(connStringBld.ConnectionString)) Then
            Threading.Thread.Sleep(2000)
            If (Not CheckConnection(connStringBld.ConnectionString)) Then
                CreateDatabase(connStringBld)
            End If
        End If
    End Sub

    Private Shared Sub CreateDatabase(connStringBld As NpgsqlConnectionStringBuilder)

        Dim connStr = String.Format("Server={0};Port={1};User Id={2};Password={3};",
                                    connStringBld.Host, connStringBld.Port, connStringBld.Username, connStringBld.Password)
        Dim mConn = New NpgsqlConnection(connStr)
        Dim mCreateDbCmd = New NpgsqlCommand(String.Format("CREATE DATABASE ""{0}"" WITH OWNER = '{1}' ENCODING = 'UTF8' CONNECTION LIMIT = {2};",
                                                           connStringBld.Database, connStringBld.Username, -1), mConn)
        mConn.Open()
        mCreateDbCmd.ExecuteNonQuery()
        mConn.Close()
        connStr = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4}",
                                connStringBld.Host, connStringBld.Port, connStringBld.Username, connStringBld.Password, connStringBld.Database)
        mConn = New NpgsqlConnection(connStr)
        Dim mCreateTblCmd = New NpgsqlCommand("CREATE TABLE table1(ID CHAR(256) CONSTRAINT id PRIMARY KEY, Title CHAR)", mConn)
        mConn.Open()
        mCreateTblCmd.ExecuteNonQuery()
        mConn.Close()

    End Sub

    Public Shared Function TableExists(connString As String, tableName As String) As Boolean
        Dim res As Boolean
        Dim sql = String.Format("SELECT EXISTS (SELECT 1 FROM pg_tables Where tablename = '{0}');", tableName)
        'Dim sql = String.Format("SELECT rdb$relation_name FROM rdb$relations WHERE (RDB$SYSTEM_FLAG = 0) AND (RDB$RELATION_TYPE = 0) AND rdb$relation_name = '{0}'", tableName.ToUpper())
        res = CType(ExecSqlScalar(connString, sql), Boolean)
        Return res
    End Function

    Public Shared Function CheckConnection(connString As String) As Boolean
        Dim res As Boolean
        Try
            Dim con = New NpgsqlConnection(connString)
            con.Open()
            res = con.State = ConnectionState.Open
            con.Close()
            con.Dispose()
        Catch ex As Exception
            res = False
        End Try
        Return res
    End Function

    ''' <summary>
    ''' Выполняет SQL запрос и возвращает его результаты в виде списка, содержащего список полей объектов
    ''' </summary>
    Public Shared Function GetObjectList(connString As String, sql As String, Optional parameters As NpgsqlParameter() = Nothing) As List(Of List(Of Object))
        Dim list As List(Of List(Of Object))
        Dim reader As SqlReaderHelper = Nothing
        Try
            Dim con = New NpgsqlConnection(connString)
            con.Open()
            Dim cmd = New NpgsqlCommand(sql, con)
            If parameters IsNot Nothing Then
                Dim params As IEnumerable(Of NpgsqlParameter)
                params = parameters
                cmd.Parameters.AddRange(params)
            End If
            Dim sr = cmd.ExecuteReader()
            reader = New SqlReaderHelper(sr, cmd, con)
            list = reader.GetObjectList
        Catch ex As Exception
            Throw New Exception(String.Format("PgUtils.ExecSQL({0}, {1}) - {2})", connString, sql, ex.ToString))
        Finally
            If reader IsNot Nothing Then
                reader.Close()
            End If
        End Try
        Return list
    End Function

End Class


