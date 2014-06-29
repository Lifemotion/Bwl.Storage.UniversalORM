Imports System.Data.SqlClient

Public Class MSSQLSRVStorage
	Inherits CommonObjStorage

	Private _name As String
	Private _dbName As String

	Public Sub New(connStringBld As SqlConnectionStringBuilder, type As Type, dbName As String)
		MyBase.New(type)
		_name = type.Name
		_dbName = dbName
		ConnectionStringBld = connStringBld

		CheckDB()
	End Sub

	Public Overrides Sub AddObj(obj As ObjBase)
		CheckDB()
		Dim json = CfJsonConverter.Serialize(obj)
		Save(ConnectionString, obj.ID, json, obj.GetType)

		For Each indexing In _indexingMembers
			Dim indexTableName = GetIndexTableName(indexing)
			Try
				Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
				If (TypeOf (indexValue) Is DateTime) Then
					indexValue = CType(indexValue, DateTime).Ticks
				End If
				SaveIndex(indexTableName, obj.ID, indexValue)
			Catch ex As Exception
				'...
			End Try
		Next
	End Sub

	Public Overrides Sub AddObjects(objects As ObjBase())
		Throw New NotSupportedException

		'CheckDB()
		'Dim jsonList = New List(Of String)
		'Dim ids = New List(Of String)
		'Dim indexTableNames = New List(Of String)
		'Dim indexValues = New List(Of Object)
		'For Each obj In objects
		'	Dim json = JsonConverter.Serialize(obj)
		'	jsonList.Add(json)
		'	ids.Add(obj.ID)

		'	For Each indexing In _indexingMembers
		'		Dim indexTableName = GetIndexTableName(indexing)
		'		indexTableNames.Add(indexTableName)
		'		Try
		'			Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
		'			If (TypeOf (indexValue) Is DateTime) Then
		'				indexValue = CType(indexValue, DateTime).Ticks
		'			End If
		'			indexValues.Add(indexValue)
		'		Catch ex As Exception
		'			'...
		'		End Try
		'	Next
		'Next
	End Sub


	Public Overrides Function FindObjCount(searchParams As SearchParams) As Long
		Dim res As Long = 0
		CheckDB()

		'''' TOP
		Dim topSql = String.Empty
		If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) Then
			If searchParams.SelectOptions.TopValue > 0 Then
				topSql = " TOP " + searchParams.SelectOptions.TopValue.ToString + " "
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
				sortTableName = GetIndexTableName(indexInfo)
			Else
				Throw New Exception("MSSQLSRVStorage.FindObj _ BadSortParam _ index " + searchParams.SortParam.Field + " not found.")
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
		Dim fromSql = GenerateFromSql(crit, sort)

		'''' where
		Dim whereSql = String.Empty
		Dim parameters As SqlParameter() = Nothing
		Dim helper = GenerateWhereSql(crit, sort)
		If helper IsNot Nothing Then
			whereSql = helper.SQL
			parameters = helper.Parameters.ToArray
		End If

		'''' main sql
		Dim mainSelect = String.Format("Select count(*) from (select {4} [{1}].[guid] FROM {2} {3}) a ", topSql, Name, fromSql, whereSql, topSql)
		Dim count = MSSQLSRVUtils.ExecSQLScalar(ConnectionString, mainSelect, parameters)
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
				topSql = " TOP " + searchParams.SelectOptions.TopValue.ToString + " "
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
				sortTableName = GetIndexTableName(indexInfo)
			Else
				Throw New Exception("MSSQLSRVStorage.FindObj _ BadSortParam _ index " + indexInfo.Name + " not found.")
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
		Dim fromSql = GenerateFromSql(crit, sort)

		'''' where
		Dim whereSql = String.Empty
		Dim parameters As SqlParameter() = Nothing
		Dim helper = GenerateWhereSql(crit, sort)
		If helper IsNot Nothing Then
			whereSql = helper.SQL
			parameters = helper.Parameters.ToArray
		End If

		Dim betweenSql = String.Empty
		If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) AndAlso (searchParams.SelectOptions.SelectMode = SelectMode.Between) Then
			betweenSql = String.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY ", searchParams.SelectOptions.StartValue, searchParams.SelectOptions.EndValue - searchParams.SelectOptions.StartValue + 1)
		End If

		'''' main sql
		Dim mainSelect = String.Format("Select {0} [{1}].[guid] FROM {2} {3} ORDER BY [{4}].[{5}] {6} {7}", topSql, Name, fromSql, whereSql, sortTableName, sortField, sortModeStr, betweenSql)

		Dim list = MSSQLSRVUtils.GetObjectList(ConnectionString, mainSelect, parameters)
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
		Dim sql = String.Format("SELECT [json], [type] FROM [dbo].[{0}] WHERE [guid] = '{1}'", Name, id)
		Dim vals = MSSQLSRVUtils.GetObjectList(ConnectionString, sql)
		Dim jsonObj = vals(0)(0)
		Dim typeName = vals(0)(1)
		If typeName Is Nothing Then
			typeName = SupportedType.AssemblyQualifiedName
		End If
		If (jsonObj IsNot Nothing) Then
			Dim json = jsonObj.ToString
			res = CfJsonConverter.Deserialize(json, Type.GetType(typeName.ToString))
		End If
		Return res
	End Function

	Public Overloads Overrides Function GetObj(Of T As ObjBase)(id As String) As T
		Return GetObj(id)
	End Function

	Public Overrides Sub RemoveObj(id As String)
		CheckDB()
		Dim sql = String.Format("DELETE FROM [dbo].[{0}] WHERE [guid] like '{1}'", Name, id)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
	End Sub

	Public Overrides Sub UpdateObj(obj As ObjBase)
		CheckDB()
		Dim json = CfJsonConverter.Serialize(obj)
		Update(ConnectionString, obj.ID, json)

		For Each indexing In _indexingMembers
			Dim indexTableName = GetIndexTableName(indexing)
			Try
				Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
				If (TypeOf (indexValue) Is DateTime) Then
					indexValue = CType(indexValue, DateTime).Ticks
				End If
				UpdateIndex(indexTableName, obj.ID, indexValue)
			Catch ex As Exception
				'...
			End Try
		Next
	End Sub

	Public ReadOnly Property Name As String
		Get
			Return _name
		End Get
	End Property

	Public Property ConnectionStringBld As SqlConnectionStringBuilder

	Public Overrides Function GetObjects(Of T As ObjBase)(objIds As String()) As IEnumerable(Of T)
		Return GetObjects(objIds).Select(Function(o) CType(o, T))
	End Function

	Public Overrides Function GetObjects(objIds As String()) As IEnumerable(Of ObjBase)
		CheckDB()

		Dim resList = New List(Of ObjBase)
		Dim sql = String.Empty
		Dim i = 0
		If (objIds IsNot Nothing AndAlso objIds.Any) Then
			For Each id In objIds
				If String.IsNullOrWhiteSpace(sql) Then
					sql = String.Format("SELECT [json] , {2} as tmp, [type] FROM [dbo].[{0}] WHERE ([guid] = '{1}')", Name, id, i)
				Else
					sql += String.Format(" Union SELECT [json] , {2} as tmp, [type] FROM [dbo].[{0}] WHERE ([guid] = '{1}')", Name, id, i)
				End If
				i += 1
			Next
			sql += " order by tmp"

			Dim ValuesObjList = MSSQLSRVUtils.GetObjectList(ConnectionString, sql)
			If (ValuesObjList IsNot Nothing) Then
				Dim tmpList = ValuesObjList.Select(Function(j)
													   Dim typeName = j(2)
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
		Dim sql = String.Format("SELECT [guid] FROM [dbo].[{0}] WHERE [guid] = '{1}'", Name, id)
		Dim idFromDb = MSSQLSRVUtils.ExecSQLScalar(ConnectionString, sql)
		If (Not String.IsNullOrWhiteSpace(idFromDb) AndAlso idFromDb.ToString.Replace(" ", "") = id) Then
			res = True
		End If
		Return res
	End Function

	Private ReadOnly Property ConnectionString As String
		Get
			Return ConnectionStringBld.ConnectionString
		End Get
	End Property

	Private Sub CheckDB()
		MSSQLSRVUtils.CreateDB(ConnectionStringBld, _dbName)
		CreateMainTable(ConnectionString, Name)
	End Sub


	Private Shared Sub CreateMainTable(connString As String, tableName As String)
		If (Not MSSQLSRVUtils.TableExists(connString, tableName)) Then
			Threading.Thread.Sleep(1000)
			If (Not MSSQLSRVUtils.TableExists(connString, tableName)) Then
				Dim sql = String.Format(My.Resources.CreateMainTableSQL, tableName)
				MSSQLSRVUtils.ExecSQL(connString, sql)
			End If
		End If

		'If (MSSQLSRVUtils.TableExists(connString, tableName)) Then
		'	Try

		'	Catch ex As Exception
		'		'...
		'	End Try
		'End If
	End Sub

	Private Sub SaveIndex(tableName As String, id As String, value As Object)
		Dim sql = String.Format(My.Resources.InsertIndexSQL, tableName, id, "@p1")
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql, {New SqlParameter("@p1", value)})
	End Sub

	Private Sub UpdateIndex(tableName As String, id As String, value As Object)
		Dim sql = String.Format("UPDATE [{0}] SET [value] = @p1 WHERE [guid] = @p2", tableName)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql, {New SqlParameter("@p1", value), New SqlParameter("@p2", id)})
	End Sub

	Private Function GetIndexTableName(indexing As IndexInfo) As String
		Dim indexTableName = String.Empty
		If indexing.Name.ToLower = "id" Then
			indexTableName = Name
		Else
			indexTableName = String.Format("{0}_{1}", Name, indexing.Name.Replace(".", "_"))

			If (Not MSSQLSRVUtils.TableExists(ConnectionString, indexTableName)) Then
				Threading.Thread.Sleep(1000)
				If (Not MSSQLSRVUtils.TableExists(ConnectionString, indexTableName)) Then
					Dim sql = String.Empty
					Dim t = indexing.Type
					Select Case (t)
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
						Case GetType(Boolean)
							sql = String.Format(My.Resources.CreateStringIndexTableSQL, indexTableName, Name, Byte.MaxValue.ToString)
						Case Else
							Dim enumType = Type.GetType("System.Enum, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
							If t.BaseType = enumType Then
								sql = String.Format(My.Resources.CreateStringIndexTableSQL, indexTableName, Name, Byte.MaxValue.ToString)
							Else
								Throw New Exception("Обнаружен не поддерживаемый тип индексируемого поля " + Name + " _ " + t.FullName)
							End If
					End Select
					MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
				End If
			End If
		End If
		Return indexTableName
	End Function

	Private Function GenerateFromSql(criterias As IEnumerable(Of FindCriteria), sort As SortParam) As String
		Dim fromSQl As String = " " + Name + " "

		Dim where = String.Empty
		For Each index In _indexingMembers
			Dim crt = Nothing
			If (criterias IsNot Nothing) Then
				crt = criterias.FirstOrDefault(Function(c) c.Field = index.Name)
			End If
			Dim sortField = False
			If sort IsNot Nothing AndAlso sort.Field = index.Name Then
				sortField = True
			End If
			If (crt IsNot Nothing Or sortField) Then
				Dim indexTableName = GetIndexTableName(index)
				fromSQl += String.Format(", [{0}]", indexTableName)
			End If
		Next
		Return fromSQl
	End Function

	Private Function GenerateWhereSql(criterias As IEnumerable(Of FindCriteria), sort As SortParam) As SqlHelper
		Dim where = String.Empty
		For Each index In _indexingMembers
			Dim crt = Nothing
			If (criterias IsNot Nothing) Then
				crt = criterias.FirstOrDefault(Function(c) c.Field = index.Name)
			End If
			Dim sortField = False
			If sort IsNot Nothing AndAlso sort.Field = index.Name Then
				sortField = True
			End If
			If (crt IsNot Nothing Or sortField) Then
				Dim indexTableName = GetIndexTableName(index)
				If (String.IsNullOrEmpty(where)) Then
					where = String.Format(" ([{0}].[guid] = [{1}].[guid]) ", Name, indexTableName)
				Else
					where += " AND " + String.Format(" ([{0}].[guid] = [{1}].[guid]) ", Name, indexTableName)
				End If
			End If
		Next

		Dim parameters As New List(Of SqlParameter)()
		Dim i = 0

		If criterias IsNot Nothing Then
			For Each crit In criterias
				Dim value = crit.Value
				If crit.Field.ToLower = "id" Then
					Dim pName = "@p" + i.ToString
					Dim str = String.Format(" ([{0}].[guid] = {1}) ", Name, pName)
					If (String.IsNullOrEmpty(where)) Then
						where += str
					Else
						where += " AND " + str
					End If
					parameters.Add(New SqlParameter(pName, value))
				Else
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
							Case FindCondition.greaterOrEqual
								str = String.Format(" ([{0}].[value] >= {1}) ", indexTableName, pName)
							Case FindCondition.lessOrEqual
								str = String.Format(" ([{0}].[value] <= {1}) ", indexTableName, pName)
						End Select

						If (String.IsNullOrEmpty(where)) Then
							where += str
						Else
							where += " AND " + str
						End If

						If (TypeOf (value) Is DateTime) Then
							value = CType(value, DateTime).Ticks
						End If
						parameters.Add(New SqlParameter(pName, value))
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
		Dim sql = String.Format("INSERT INTO [dbo].[{0}] ([guid] ,[json], [type]) VALUES('{1}', '{2}', '{3}')", Name, id, json, type.AssemblyQualifiedName)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
	End Sub

	Private Sub Update(connStr As String, id As String, json As String)
		Dim sql = String.Format("UPDATE [{0}] SET [json] = @p1 WHERE [guid] = @p2", Name)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql, {New SqlParameter("@p1", json), New SqlParameter("@p2", id)})
	End Sub

	Public Overrides Sub RemoveAllObjects()
		CheckDB()
		Dim sql = String.Format("DELETE FROM [dbo].[{0}]", Name)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
	End Sub
End Class
