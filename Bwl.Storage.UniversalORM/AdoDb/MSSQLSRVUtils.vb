Imports System.Data.SqlClient

Public Class MSSQLSRVUtils
    Public Shared Sub ExecSQL(connString As String, sql As String, Optional parameters As SqlParameter() = Nothing)
        Dim con = New SqlConnection(connString)
        Try
            con.Open()
            Dim cmd = New SqlCommand(sql, con)
            If parameters IsNot Nothing Then
                cmd.Parameters.AddRange(parameters)
            End If
            cmd.ExecuteNonQuery()
            cmd.Dispose()
        Catch ex As Exception
            Dim readableParams = If(parameters IsNot Nothing AndAlso parameters.Any(),
                                    parameters.Select(Function(f) $"{f.ParameterName}, type {f.DbType}, value {f.Value}").Aggregate(Function(f, t) f + vbNewLine + t),
                                    "None")
            Throw New Exception($"MSSQLSRVUtils.ExecSQL. Connection string: {connString}{vbNewLine}SQL: {sql}{vbNewLine} Params: {readableParams}{vbNewLine} ERR: {ex.ToString()}")
        Finally
            con.Close()
            con.Dispose()
        End Try
    End Sub

    Public Shared Function ExecSQLScalar(connString As String, sql As String, Optional parameters As SqlParameter() = Nothing) As Object
        Dim res As Object = Nothing
        Dim con = New SqlConnection(connString)
        Try
            con.Open()
            Dim cmd = New SqlCommand(sql, con)
            If parameters IsNot Nothing Then
                cmd.Parameters.AddRange(parameters)
            End If
            res = cmd.ExecuteScalar()
            cmd.Dispose()
        Catch ex As Exception
            Dim readableParams = If(parameters IsNot Nothing AndAlso parameters.Any(),
                                    parameters.Select(Function(f) $"{f.ParameterName}, type {f.DbType}, value {f.Value}").Aggregate(Function(f, t) f + vbNewLine + t),
                                    "None")
            Throw New Exception($"MSSQLSRVUtils.ExecSQLScalar. Connection string: {connString}{vbNewLine}SQL: {sql}{vbNewLine} Params: {readableParams}{vbNewLine} ERR: {ex.ToString()}")
        Finally
            con.Close()
            con.Dispose()
        End Try
        Return res
    End Function

    Public Shared Sub CreateDB(connStringBld As SqlConnectionStringBuilder, dbName As String)
        If (Not MSSQLSRVUtils.CheckConnection(connStringBld.ConnectionString)) Then
            Threading.Thread.Sleep(2000)
            If (Not MSSQLSRVUtils.CheckConnection(connStringBld.ConnectionString)) Then
                Dim conStrBld2 = New SqlConnectionStringBuilder
                conStrBld2.IntegratedSecurity = connStringBld.IntegratedSecurity
                conStrBld2.ConnectTimeout = connStringBld.ConnectTimeout
                conStrBld2.UserID = connStringBld.UserID
                conStrBld2.Password = connStringBld.Password
                conStrBld2.DataSource = connStringBld.DataSource
                Dim sql = String.Format("CREATE DATABASE [{0}]", dbName)
                MSSQLSRVUtils.ExecSQL(conStrBld2.ConnectionString, sql)
                Threading.Thread.Sleep(2000)
                connStringBld.InitialCatalog = dbName
            End If
        End If
    End Sub

    Public Shared Function TableExists(connString As String, tableName As String) As Boolean
        Dim res = False
        Dim sql = String.Format("SELECT object_id('{0}')", tableName)
        If (Not DBNull.Value.Equals(MSSQLSRVUtils.ExecSQLScalar(connString, sql))) Then
            res = True
        End If
        Return res
    End Function

    Public Shared Function CheckConnection(connString As String) As Boolean
        Dim res = False
        Try
            Dim con = New SqlConnection(connString)
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
    Public Shared Function GetObjectList(connString As String, sql As String, Optional parameters As SqlParameter() = Nothing) As List(Of List(Of Object))
        Dim list As List(Of List(Of Object)) = Nothing
        Dim reader As SqlReaderHelper = Nothing
        Try
            Dim con = New SqlConnection(connString)
            con.Open()
            Dim cmd = New SqlCommand(sql, con)
            If parameters IsNot Nothing Then
                cmd.Parameters.AddRange(parameters)
            End If
            Dim sr = cmd.ExecuteReader()
            reader = New SqlReaderHelper(sr, cmd, con)
            list = reader.GetObjectList
        Catch ex As Exception
            Dim readableParams = If(parameters IsNot Nothing AndAlso parameters.Any(),
                                    parameters.Select(Function(f) $"{f.ParameterName}, type {f.DbType}, value {f.Value}").Aggregate(Function(f, t) f + vbNewLine + t),
                                    "None")
            Throw New Exception($"MSSQLSRVUtils.GetObjectList. Connection string: {connString}{vbNewLine}SQL: {sql}{vbNewLine} Params: {readableParams}{vbNewLine} ERR: {ex.ToString()}")
        Finally
            If reader IsNot Nothing Then
                reader.Close()
            End If
        End Try
        Return list
    End Function

End Class


