Imports System.Data.SqlClient

Public Class MSSQLSRVUtils

	Public Shared Sub ExecSQL(connString As String, sqlHelper As SqlHelper)
		ExecSQL(connString, sqlHelper.SQL, sqlHelper.Parameters.ToArray)
	End Sub

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
			Throw New Exception(String.Format("MSSQLSRVUtils.ExecSQL({0}, {1}) - {2})", connString, sql, ex.ToString))
		Finally
			con.Close()
			con.Dispose()
		End Try
	End Sub

	Public Shared Function ExecSQLScalar(connString As String, sql As String) As Object
		Dim res As Object = Nothing
		Dim con = New SqlConnection(connString)
		Try
			con.Open()
			Dim cmd = New SqlCommand(sql, con)
			res = cmd.ExecuteScalar()
			cmd.Dispose()
		Catch ex As Exception
			Throw New Exception(String.Format("MSSQLSRVUtils.ExecSQLScalar({0}, {1}) - {2})", connString, sql, ex.ToString))
		Finally
			con.Close()
			con.Dispose()
		End Try
		Return res
	End Function

	Public Shared Sub CreateDB(connStringBld As SqlConnectionStringBuilder, dbName As String)
		If (Not MSSQLSRVUtils.CheckConnection(connStringBld.ConnectionString)) Then
			connStringBld.InitialCatalog = String.Empty
			Dim sql = String.Format("CREATE DATABASE [{0}]", dbName)
			MSSQLSRVUtils.ExecSQL(connStringBld.ConnectionString, sql)
			connStringBld.InitialCatalog = dbName
		End If
	End Sub

	Public Shared Function TableExists(connString As String, tableName As String)
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
			Throw New Exception(String.Format("MSSQLSRVUtils.ExecSQL({0}, {1}) - {2})", connString, sql, ex.ToString))
		Finally
			reader.Close()
		End Try
		Return list
	End Function

End Class


