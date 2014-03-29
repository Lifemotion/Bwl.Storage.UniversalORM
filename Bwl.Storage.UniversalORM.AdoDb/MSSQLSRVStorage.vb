Imports System.Data.SqlClient

Public Class MSSQLSRVStorage(Of T As ObjBase)
	Inherits CommonObjStorage(Of T)
	Private _name As String

	Public Sub New(connString As String)
		_name = GetType(T).Name
		ConnectionString = connString
	End Sub

	Public Overrides Sub AddObj(obj As T)
		CreateMainTable(ConnectionString, Name)

		Dim json = JsonConverter.Serialize(obj)
		Save(ConnectionString, obj.ID, json)

		For Each indexing In _indexingMembers
			Dim indexTableName = GetIndexTableName(indexing)
			Try
				Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
				SaveIndex(indexTableName, obj.ID, indexValue)
			Catch ex As Exception
				'...
			End Try
		Next
	End Sub

	Public Shared Sub CreateMainTable(connString As String, tableName As String)
		If (Not MSSQLSRVUtils.TableExists(connString, tableName)) Then
			Dim sql = String.Format(My.Resources.CreateMainTableSQL, tableName)
			MSSQLSRVUtils.ExecSQL(connString, sql)
		End If
	End Sub

	Private Sub SaveIndex(tableName As String, id As String, value As Object)
		Dim sql = String.Format(My.Resources.InsertIndexSQL, tableName, id, "@p1")
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql, {New SqlParameter("@p1", value)})
	End Sub

	Public Overrides Function FindObj(criterias() As FindCriteria) As String()
		Dim sql = String.Format("SELECT [{0}].[guid] FROM [dbo].[{0}]", Name)
		Dim parameters As SqlParameter() = Nothing

		If (criterias IsNot Nothing AndAlso criterias.Any) Then
			Dim helper = GenerateWhereSql(criterias)
			sql = sql + helper.SQL
			parameters = helper.Parameters.ToArray
		End If

		Dim list = MSSQLSRVUtils.GetObjectList(ConnectionString, sql, parameters)
		If list IsNot Nothing AndAlso list.Any Then
			Dim resList = list.Select(Function(d) d(0).ToString)
			Return resList.ToArray
		Else
			Return {}
		End If
	End Function

	Public Overrides Function GetObj(id As String) As T
		Dim res As T = Nothing
		Dim sql = String.Format("SELECT [json] FROM [dbo].[{0}] WHERE [guid] = '{1}'", Name, id)
		Dim jsonObj = MSSQLSRVUtils.ExecSQLScalar(ConnectionString, sql)
		If (jsonObj IsNot Nothing) Then
			Dim json = jsonObj.ToString
			res = JsonConverter.Deserialize(Of T)(json)
		End If

		Return res
	End Function

	Public Overrides Sub RemoveObj(id As String)
		Dim sql = String.Format("DELETE FROM [dbo].[{0}] WHERE [guid] like '{1}'", Name, id)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
	End Sub

	Public Overrides Sub UpdateObj(obj As T)
		Throw New Exception("Операция не поддерживается")
	End Sub

	Public ReadOnly Property Name As String
		Get
			Return _name
		End Get
	End Property

	Public Property ConnectionString As String

	Private Function GetIndexTableName(indexing As IndexInfo) As String
		Dim indexTableName = String.Format("{0}_{1}", Name, indexing.Name.Replace(".", "_"))

		If (Not MSSQLSRVUtils.TableExists(ConnectionString, indexTableName)) Then
			Dim sql = String.Empty
			Select Case (indexing.Type)
				Case GetType(String)
					Dim len = Byte.MaxValue.ToString
					If (indexing.Length > 0 And indexing.Length < Byte.MaxValue) Then
						len = indexing.Length.ToString
					End If
					sql = String.Format(My.Resources.CreateStringIndexTableSQL, indexTableName, Name, len)
				Case GetType(Integer)
					sql = String.Format(My.Resources.CreateIntIndexTableSQL, indexTableName, Name)
				Case GetType(Double)
					sql = String.Format(My.Resources.CreateFloatIndexTableSQL, indexTableName, Name)
				Case GetType(DateTime)
					sql = String.Format(My.Resources.CreateBigIntIndexTableSQL, indexTableName, Name)
				Case GetType(Long)
					sql = String.Format(My.Resources.CreateBigIntIndexTableSQL, indexTableName, Name)
				Case Else
					Throw New Exception("Обнаружен не поддерживаемый тип индексируемого поля")
			End Select

			MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
		End If

		Return indexTableName
	End Function

	Public Function GenerateWhereSql(criterias() As FindCriteria) As SqlHelper
		Dim fromSQl As String = ""

		Dim where = String.Empty
		For Each index In _indexingMembers
			Dim indexTableName = GetIndexTableName(index)
			fromSQl += String.Format(", [{0}]", indexTableName)

			If (String.IsNullOrEmpty(where)) Then
				where = String.Format(" ([{0}].[guid] = [{1}].[guid]) ", Name, indexTableName)
			Else
				where += " AND " + String.Format(" ([{0}].[guid] = [{1}].[guid]) ", Name, indexTableName)
			End If
		Next


		Dim parameters As New List(Of SqlParameter)()
		Dim i = 0
		For Each crit In criterias
			Dim ind = _indexingMembers.FirstOrDefault(Function(f) f.Name = crit.Field)
			If (ind IsNot Nothing) Then
				Dim indexTableName = GetIndexTableName(ind)
				Dim str = String.Empty
				Dim pName = "@p" + i.ToString
				Select Case crit.Condition
					Case FindCondition.eqaul
						str = String.Format(" ([{0}].[value] = {1}) ", indexTableName, pName)
					Case FindCondition.greater
						str = String.Format(" ([{0}].[value] > {1}) ", indexTableName, pName)
					Case FindCondition.less
						str = String.Format(" ([{0}].[value] < {1}) ", indexTableName, pName)
					Case FindCondition.notEqual
						str = String.Format(" ([{0}].[value] <> {1}) ", indexTableName, pName)
					Case FindCondition.likeEqaul
						str = String.Format(" ([{0}].[value] LIKE {1}) ", indexTableName, pName)
				End Select

				If (String.IsNullOrEmpty(where)) Then
					where += str
				Else
					where += " AND " + str
				End If
				parameters.Add(New SqlParameter(pName, crit.Value))
				i += 1
			Else
				Throw New Exception("Поле " + crit.Field + " не является индексируемым")
			End If
		Next
		Return New SqlHelper(fromSQl + " WHERE " + where, parameters)
	End Function

	Private Sub Save(connStr As String, id As String, json As String)
		Dim sql = String.Format(My.Resources.InsertMainSQL, Name, id, json)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
	End Sub

End Class
