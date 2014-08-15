Imports FirebirdSql.Data.FirebirdClient
Imports FirebirdSql.Data.Isql

Public Class FBStorage
	Inherits CommonObjStorage

	Private _name As String
	Private _dbName As String

	Public Property ConnectionStringBld As FbConnectionStringBuilder

	Private ReadOnly Property ConnectionString As String
		Get
			Return ConnectionStringBld.ConnectionString
		End Get
	End Property

	Public Sub New(connStringBld As FbConnectionStringBuilder, type As Type, dbName As String)
		MyBase.New(type)
		_name = type.Name
		_dbName = dbName
		ConnectionStringBld = connStringBld

		CheckDB()
	End Sub

	Public Overrides Sub AddObj(obj As ObjBase)
		CheckDB()
		Dim json = CfJsonConverter.Serialize(obj)
		'Dim fromEncodind = System.Text.Encoding.UTF8
		'Dim encoding = System.Text.Encoding.GetEncoding(1251)
		'Dim djson = fromEncodind.GetString(encoding.GetBytes(json))
		Save(ConnectionString, obj.ID, json, obj.GetType)

		For Each indexing In _indexingMembers
			'Dim indexTableName = GetIndexTableName(indexing)
			Dim indexTableName = GetIndexName(indexing).Replace(".", "_")
			Try
				Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
				If (TypeOf (indexValue) Is DateTime) Then
					indexValue = CType(indexValue, DateTime).Ticks
				End If
				SaveIndex(Name, indexTableName, obj.ID, indexValue)
			Catch ex As Exception
				Throw New Exception(ex.Message)
			End Try
		Next
	End Sub

	Public Overrides Sub AddObjects(objects As ObjBase())
		For Each obj In objects
			AddObj(obj)
		Next
		'Throw New NotSupportedException
	End Sub

	Public Overrides Function FindObjCount(searchParams As SearchParams) As Long
		Dim res As Long = 0
		CheckDB()

		'''' TOP
		Dim topSql = String.Empty
		If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) Then
			If searchParams.SelectOptions.TopValue > 0 Then
				topSql = " FIRST " + searchParams.SelectOptions.TopValue.ToString + " "
			End If
		End If

		'''' sorting
		Dim sortModeStr = "ASC"
		Dim sortField = "guid"
		Dim sortTableName = Name
		If (searchParams IsNot Nothing) AndAlso (searchParams.SortParam IsNot Nothing) Then
			If searchParams.SortParam.SortMode = SortMode.Descending Then
				sortModeStr = "DESC"
			End If

			Dim indexInfo = _indexingMembers.FirstOrDefault(Function(indInf) indInf.Name = searchParams.SortParam.Field)
			If (indexInfo IsNot Nothing) Then
				sortField = "value"
				sortTableName = GetIndexName(indexInfo)
			Else
				Throw New Exception("FBStorage.FindObj _ BadSortParam _ index " + searchParams.SortParam.Field + " not found.")
			End If
		End If

		Dim crit As IEnumerable(Of FindCriteria) = Nothing
		If (searchParams IsNot Nothing) Then
			crit = searchParams.FindCriterias
		End If

		Dim sort As SortParam = Nothing
		If (searchParams IsNot Nothing) Then
			sort = searchParams.SortParam
		End If

		'''' from + where
		'Dim fromSql = GenerateFromSql(crit, sort)

		'''' where
		Dim whereSql = String.Empty
		Dim parameters As FbParameter() = Nothing
		Dim helper = GenerateWhereSql(crit, sort)
		If helper IsNot Nothing Then
			whereSql = helper.SQL
			parameters = helper.Parameters.ToArray
		End If

		'''' main sql
		Dim mainSelect = String.Format("SELECT COUNT(*) FROM (SELECT {2} GUID FROM {0} {1})", Name, whereSql, topSql)
		Dim count = FbUtils.ExecSQLScalar(ConnectionString, mainSelect, parameters)
		If count IsNot Nothing Then
			res = Convert.ToInt64(count)
		End If
		Return res
	End Function

	Public Overrides Function FindObj(searchParams As SearchParams) As String()
		CheckDB()

		'''' TOP
		Dim topSql = String.Empty
		If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) AndAlso (searchParams.SelectOptions.SelectMode = SelectMode.Top) Then
			If searchParams.SelectOptions.TopValue > 0 Then
				topSql = " FIRST " + searchParams.SelectOptions.TopValue.ToString + " "
			End If
		End If

		'''' sorting
		Dim sortModeStr = "ASC"
		Dim sortField = "guid"
		'Dim sortTableName = Name
		If (searchParams IsNot Nothing) AndAlso (searchParams.SortParam IsNot Nothing) Then
			If searchParams.SortParam.SortMode = SortMode.Descending Then
				sortModeStr = "DESC"
			End If

			Dim indexInfo = _indexingMembers.FirstOrDefault(Function(indInf) indInf.Name = searchParams.SortParam.Field)
			If (indexInfo IsNot Nothing) Then
				sortField = GetIndexName(indexInfo)
			Else
				Throw New Exception("FBStorage.FindObj _ BadSortParam _ index " + indexInfo.Name + " not found.")
			End If
		End If

		Dim crit As IEnumerable(Of FindCriteria) = Nothing
		If (searchParams IsNot Nothing) Then
			crit = searchParams.FindCriterias
		End If

		Dim sort As SortParam = Nothing
		If (searchParams IsNot Nothing) Then
			sort = searchParams.SortParam
		End If

		'''' from + where
		'Dim fromSql = GenerateFromSql(crit, sort)

		'''' where
		Dim whereSql = String.Empty
		Dim parameters As FbParameter() = Nothing
		Dim helper = GenerateWhereSql(crit, sort)
		If helper IsNot Nothing Then
			whereSql = helper.SQL
			parameters = helper.Parameters.ToArray
		End If

		Dim betweenSql = String.Empty
		Dim mainSelect = String.Empty
		If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) AndAlso (searchParams.SelectOptions.SelectMode = SelectMode.Between) Then
			'SELECT FIRST 10 SKIP 20 column1, column2, column3 FROM foo
			mainSelect = String.Format("SELECT FIRST {1} SKIP {2} GUID FROM {0}", Name, searchParams.SelectOptions.EndValue - searchParams.SelectOptions.StartValue, searchParams.SelectOptions.StartValue)
			'betweenSql
		Else

			'''' main sql			
			If (searchParams Is Nothing) Or ((searchParams IsNot Nothing) AndAlso (searchParams.SortParam Is Nothing)) Then
				mainSelect = String.Format("SELECT {2} GUID FROM {0} {1}", Name, whereSql, topSql)
			ElseIf (searchParams.SortParam IsNot Nothing) Then
				mainSelect = String.Format("SELECT {2} GUID FROM {0} {1} ORDER BY {3} {4}", Name, whereSql, topSql, """" + sortField.ToUpper() + """", sortModeStr)
			End If
		End If
		Dim list = FbUtils.GetObjectList(ConnectionString, mainSelect, parameters)
		If (list IsNot Nothing AndAlso list.Any) Then
			Dim resList = list.Select(Function(d) d(0).ToString)
			Return resList.ToArray
		Else
			Return {}
		End If
	End Function

	Public Overrides Function GetObj(id As String) As ObjBase
		CheckDB()

		Dim res As ObjBase = Nothing
		Dim sql = String.Format("SELECT json, type FROM {0} WHERE guid = '{1}'", Name, id)
		Dim vals = FbUtils.GetObjectList(ConnectionString, sql)
		If vals IsNot Nothing AndAlso vals.Any Then
			Dim jsonObj = vals(0)(0)
			Dim typeName = vals(0)(1)
			If typeName Is Nothing Then
				typeName = SupportedType.AssemblyQualifiedName
			End If
			If (jsonObj IsNot Nothing) Then
				Dim json = jsonObj.ToString
				res = CfJsonConverter.Deserialize(json, Type.GetType(typeName.ToString))
			End If
		End If
		Return res
	End Function

	Public Overloads Overrides Function GetObj(Of T As ObjBase)(id As String) As T
		Return GetObj(id)
	End Function

	Public Overrides Sub RemoveObj(id As String)
		CheckDB()
		Dim sql = String.Format("DELETE FROM {0} WHERE guid like '{1}'", Name, id)
		FbUtils.ExecSQL(ConnectionString, sql)
	End Sub

	Public Overrides Sub UpdateObj(obj As ObjBase)
		CheckDB()
		Dim json = CfJsonConverter.Serialize(obj)
		Update(ConnectionString, obj.ID, json)

		For Each indexing In _indexingMembers
			Dim indexTableName = GetIndexName(indexing).Replace(".", "_")
			Try
				Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
				If (TypeOf (indexValue) Is DateTime) Then
					indexValue = CType(indexValue, DateTime).Ticks
				End If
				UpdateIndex(indexTableName, obj.ID, indexValue)
			Catch ex As Exception
				Throw New Exception("Ошибка обновления объекта в базе данных: " + ex.Message)
			End Try
		Next
	End Sub

	Public ReadOnly Property Name As String
		Get
			Return _name
		End Get
	End Property


	Public Overrides Function GetObjects(Of T As ObjBase)(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of T)
		Return GetObjects(objIds, sortParam).Select(Function(o) CType(o, T))
	End Function

	Public Overrides Function GetObjects(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase)
		CheckDB()

		'''' sorting
		Dim sortModeStr = "ASC"
		Dim sortField = ""
		Dim sortFieldName = ""
		If sortParam IsNot Nothing Then
			If sortParam.SortMode = SortMode.Descending Then
				sortModeStr = "DESC"
			End If

			Dim indexInfo = _indexingMembers.FirstOrDefault(Function(indInf) indInf.Name = sortParam.Field)
			If (indexInfo IsNot Nothing) Then
				sortFieldName = GetIndexName(indexInfo).ToUpper
				sortField = ", " + sortFieldName
			Else
				Throw New Exception("FBtorage.GetObjects _ BadSortParam _ index " + indexInfo.Name + " not found.")
			End If
		End If

		Dim resList = New List(Of ObjBase)
		If (objIds IsNot Nothing AndAlso objIds.Any) Then
			Dim strIds = " ( ( t.GUID = '" + String.Join("' ) or ( t.GUID = '", objIds) + "' ) ) "
			Dim sql = String.Format("SELECT JSON , TYPE {1} FROM {0} t  WHERE {2}", Name, sortField, strIds)

			If (sortFieldName <> "") Then
				'sql += " and  (s.GUID = t.GUID) "
				sql += String.Format(" ORDER BY {0} {1}", sortFieldName, sortModeStr)
			End If

			Dim ValuesObjList = FbUtils.GetObjectList(ConnectionString, sql)
			If (ValuesObjList IsNot Nothing) Then
				Dim tmpList = ValuesObjList.Select(Function(j)
													   Dim typeName = j(1)
													   If typeName Is Nothing Then
														   typeName = SupportedType.AssemblyQualifiedName
													   End If
													   Return CType(CfJsonConverter.Deserialize(j(0).ToString, Type.GetType(typeName)), ObjBase)
												   End Function)
				resList.AddRange(tmpList)
			End If
		End If
		Return resList
	End Function

	Public Overrides Function Contains(id As String) As Boolean
		CheckDB()

		Dim res = False
		Dim sql = String.Format("SELECT GUID FROM {0} WHERE GUID = '{1}'", Name, id)
		Dim idFromDb = FbUtils.ExecSQLScalar(ConnectionString, sql)
		If (Not String.IsNullOrWhiteSpace(idFromDb) AndAlso idFromDb.ToString.Replace(" ", "") = id) Then
			res = True
		End If
		Return res
	End Function


	Private Sub CheckDB()
		FbUtils.CreateDB(ConnectionStringBld, _dbName)
		CreateMainTable(ConnectionString, Name)
	End Sub


	Private Shared Sub CreateMainTable(connString As String, tableName As String)

		If (Not FbUtils.TableExists(connString, tableName)) Then
			Threading.Thread.Sleep(1000)
			If (Not FbUtils.TableExists(connString, tableName)) Then
				Dim sql = String.Format(My.Resources.CreateMainTableSQL, tableName)
				FbUtils.ExecSQL(connString, sql)
			End If
		End If
	End Sub

	Private Sub SaveIndex(tableName As String, columnName As String, id As String, value As Object)
		Dim sql = String.Format(My.Resources.UpdateIndexSQL, tableName, columnName.ToUpper(), id, "@p1")
		FbUtils.ExecSQL(ConnectionString, sql, {New FbParameter("@p1", value)})
	End Sub

	Private Sub UpdateIndex(ColumnName As String, id As String, value As Object)
		Dim sql = String.Format("UPDATE {0} SET {1} = @p2 WHERE GUID = @p3", Name, """" + ColumnName.ToUpper() + """")
		FbUtils.ExecSQL(ConnectionString, sql, {New FbParameter("@p2", value), New FbParameter("@p3", id)})
	End Sub

	Private Function GetIndexName(indexing As IndexInfo) As String
		Dim indexName = String.Empty
		indexName = indexing.Name.Replace(".", "_").ToUpper()

		Dim t = indexing.Type

		Dim SQLfields = String.Format("SELECT R.RDB$FIELD_NAME FROM RDB$FIELDS F, RDB$RELATION_FIELDS R WHERE F.RDB$FIELD_NAME = R.RDB$FIELD_SOURCE AND R.RDB$SYSTEM_FLAG = 0 AND RDB$RELATION_NAME = '{0}' AND R.RDB$FIELD_NAME = '{1}'", Name.ToUpper(), indexName)
		Dim fields = FbUtils.ExecSQLScalar(ConnectionString, SQLfields)
		If fields Is Nothing Then
			Dim ListQuery As New FbBatchExecution(New FbConnection(ConnectionString))
			Dim sql = String.Empty
			Select Case (t)
				Case GetType(String)
					Dim len = Byte.MaxValue.ToString
					If (indexing.Length > 0 And indexing.Length < Byte.MaxValue) Then
						len = indexing.Length.ToString
					End If
					sql = String.Format(My.Resources.AddStringColumn, Name, indexName, len)

				Case GetType(Integer)
					sql = String.Format(My.Resources.AddIntColumn, Name, indexName)
				Case GetType(Double)
					sql = String.Format(My.Resources.AddFloatColumn, Name, indexName)
				Case GetType(DateTime)
					sql = String.Format(My.Resources.AddBigIntColumn, Name, indexName)
				Case GetType(Long)
					sql = String.Format(My.Resources.AddBigIntColumn, Name, indexName)
				Case GetType(Boolean)
					sql = String.Format(My.Resources.AddStringColumn, Name, indexName, Byte.MaxValue.ToString)
				Case Else
					Dim enumType = Type.GetType("System.Enum, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
					If t.BaseType = enumType Then
						sql = String.Format(My.Resources.AddStringColumn, Name, indexName, Byte.MaxValue.ToString)
					Else
						Throw New Exception("Обнаружен не поддерживаемый тип индексируемого поля " + indexName + " _ " + t.FullName)
					End If
			End Select
			ListQuery.SqlStatements.Add(sql)
			ListQuery.SqlStatements.Add(String.Format(My.Resources.CreateIndexSQL, Name, indexName))

			ListQuery.Execute()
		End If
		'FbUtils.ExecSQL(ConnectionString, sql)
		Return indexing.Name
	End Function

	Private Function GenerateWhereSql(criterias As IEnumerable(Of FindCriteria), sort As SortParam) As SqlHelper
		Dim where = String.Empty
		Dim parameters As New List(Of FbParameter)()
		Dim i = 0

		If criterias IsNot Nothing Then
			For Each crit In criterias
				Dim value = crit.Value
				If crit.Field.ToLower = "id" Then
					Dim pName = "@p" + i.ToString
					Dim str = String.Format(" (GUID = {0}) ", pName)
					If (String.IsNullOrEmpty(where)) Then
						where += str
					Else
						where += " AND " + str
					End If
					parameters.Add(New FbParameter(pName, value))
				Else
					Dim ind = _indexingMembers.FirstOrDefault(Function(f) f.Name = crit.Field)
					If (ind IsNot Nothing) Then
						Dim indexTableName = GetIndexName(ind).Replace(".", "_").ToUpper
						Dim str = String.Empty
						Dim pName = "@p" + i.ToString
						Const quote As String = """"
						Select Case crit.Condition
							Case FindCondition.eqaul
								str = String.Format(" ({0} = {1}) ", quote + indexTableName + quote, pName)
							Case FindCondition.greater
								str = String.Format(" ({0} > {1}) ", quote + indexTableName + quote, pName)
							Case FindCondition.less
								str = String.Format(" ({0} < {1}) ", quote + indexTableName + quote, pName)
							Case FindCondition.notEqual
								str = String.Format(" ({0} <> {1}) ", quote + indexTableName + quote, pName)
							Case FindCondition.likeEqaul
								str = String.Format(" ({0} LIKE {1}) ", quote + indexTableName + quote, pName)
							Case FindCondition.greaterOrEqual
								str = String.Format(" ({0} >= {1}) ", quote + indexTableName + quote, pName)
							Case FindCondition.lessOrEqual
								str = String.Format(" ({0} <= {1}) ", quote + indexTableName + quote, pName)
						End Select

						If (String.IsNullOrEmpty(where)) Then
							where += str
						Else
							where += " AND " + str
						End If

						If (TypeOf (value) Is DateTime) Then
							value = CType(value, DateTime).Ticks
						End If
						parameters.Add(New FbParameter(pName, value))
						i += 1
					Else
						Throw New Exception("Поле " + crit.Field + " не является индексируемым")
					End If
				End If
			Next
		End If

		If String.IsNullOrWhiteSpace(where) Then
			Return Nothing
		Else
			Return New SqlHelper(" WHERE " + where, parameters)
		End If
	End Function

	Private Sub Save(connStr As String, id As String, json As String, type As Type)
		Dim sql = String.Format("INSERT INTO {0}(GUID ,JSON, TYPE) VALUES('{1}', '{2}', '{3}')", Name, id, json, type.AssemblyQualifiedName)
		FbUtils.ExecSQL(ConnectionString, sql)
	End Sub

	Private Sub Update(connStr As String, id As String, json As String)
		Dim sql = String.Format("UPDATE {0} SET JSON = @p1 WHERE guid = @p2", Name)
		FbUtils.ExecSQL(ConnectionString, sql, {New FbParameter("@p1", json), New FbParameter("@p2", id)})
	End Sub

	Public Overrides Sub RemoveAllObjects()
		CheckDB()
		Dim sql = String.Format("DELETE FROM {0}", Name)
		FbUtils.ExecSQL(ConnectionString, sql)
	End Sub

End Class
