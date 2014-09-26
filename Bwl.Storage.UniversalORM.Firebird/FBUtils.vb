Imports FirebirdSql.Data.FirebirdClient

Public Class FbUtils

	Public Shared Sub ExecSQL(connString As String, sql As String, Optional parameters As FirebirdSql.Data.FirebirdClient.FbParameter() = Nothing)
		Dim con = New FbConnection(connString)
		Try
			con.Open()

			Dim cmd = New FbCommand(sql, con)
			If parameters IsNot Nothing Then
				Dim params As System.Collections.Generic.IEnumerable(Of FbParameter)
				' ???
				params = parameters
				cmd.Parameters.AddRange(params)
			End If
			cmd.ExecuteNonQuery()
			cmd.Dispose()
		Catch ex As Exception
			Throw New Exception(String.Format("FbUtils.ExecSQL({0}, {1}) - {2})", connString, ex.Message, sql))
		Finally
			con.Close()
			con.Dispose()
		End Try
	End Sub

	Public Shared Function ExecSQLScalar(connString As String, sql As String, Optional parameters As FbParameter() = Nothing) As Object
		Dim res As Object = Nothing
		Dim con = New FbConnection(connString)
		Try
			con.Open()
			Dim cmd = New FbCommand(sql, con)
			If parameters IsNot Nothing Then
				Dim params As System.Collections.Generic.IEnumerable(Of FbParameter)
				' ???
				params = parameters
				cmd.Parameters.AddRange(params)
			End If
			res = cmd.ExecuteScalar()
			cmd.Dispose()
		Catch ex As Exception
			Throw New Exception(String.Format("FbUtils.ExecSQLScalar({0}, {1}) - {2})", connString, sql, ex.Message))
		Finally
			con.Close()
			con.Dispose()
		End Try
		Return res
	End Function

	Public Shared Sub CreateDB(connStringBld As FbConnectionStringBuilder, dbName As String)
		If (Not FbUtils.CheckConnection(connStringBld.ConnectionString)) Then
			Threading.Thread.Sleep(2000)
			If (Not FbUtils.CheckConnection(connStringBld.ConnectionString)) Then
				FbConnection.CreateDatabase(connStringBld.ToString())
			End If
		End If
	End Sub


	Public Shared Function TableExists(connString As String, tableName As String) As Boolean
		Dim res = False
		Dim sql = String.Format("SELECT rdb$relation_name FROM rdb$relations WHERE (RDB$SYSTEM_FLAG = 0) AND (RDB$RELATION_TYPE = 0) AND rdb$relation_name = '{0}'", tableName.ToUpper())
		If FbUtils.ExecSQLScalar(connString, sql) IsNot Nothing Then
			res = True
		End If
		Return res
	End Function

	Public Shared Function CheckConnection(connString As String) As Boolean
		Dim res = False
		Try
			Dim con = New FbConnection(connString)
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
	Public Shared Function GetObjectList(connString As String, sql As String, Optional parameters As FbParameter() = Nothing) As List(Of List(Of Object))
		Dim list As List(Of List(Of Object)) = Nothing
		Dim reader As SqlReaderHelper = Nothing
		Try
			Dim con = New FbConnection(connString)
			con.Open()
			Dim cmd = New FbCommand(sql, con)
			If parameters IsNot Nothing Then
				Dim params As System.Collections.Generic.IEnumerable(Of FbParameter)
				params = parameters
				cmd.Parameters.AddRange(params)
			End If
			Dim sr = cmd.ExecuteReader()
			reader = New SqlReaderHelper(sr, cmd, con)
			list = reader.GetObjectList
		Catch ex As Exception
			Throw New Exception(String.Format("FbUtils.ExecSQL({0}, {1}) - {2})", connString, sql, ex.ToString))
		Finally
			If reader IsNot Nothing Then
				reader.Close()
			End If
		End Try
		Return list
	End Function

End Class


