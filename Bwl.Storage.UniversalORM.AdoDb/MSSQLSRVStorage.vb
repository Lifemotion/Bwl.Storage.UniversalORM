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
		Dim json = JsonConverter.Serialize(obj)
		Save(ConnectionString, obj.ID, json)

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
		CheckDB()
		Dim jsonList = New List(Of String)
		Dim ids = New List(Of String)
		Dim indexTableNames = New List(Of String)
		Dim indexValues = New List(Of Object)
		For Each obj In objects
			Dim json = JsonConverter.Serialize(obj)
			jsonList.Add(json)
			ids.Add(obj.ID)

			For Each indexing In _indexingMembers
				Dim indexTableName = GetIndexTableName(indexing)
				indexTableNames.Add(indexTableName)
				Try
					Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
					If (TypeOf (indexValue) Is DateTime) Then
						indexValue = CType(indexValue, DateTime).Ticks
					End If
					indexValues.Add(indexValue)
				Catch ex As Exception
					'...
				End Try
			Next
		Next

		'Save(ConnectionString, ids, jsonList)
		'SaveIndex(indexTableNames, ids, indexValues)
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

		'''' main sql
		Dim mainSelect = String.Format("Select count({0} [{1}].[guid]) FROM {2} {3} ", topSql, Name, fromSql, whereSql, sortTableName, sortField, sortModeStr)
		Dim count = MSSQLSRVUtils.ExecSQLScalar(ConnectionString, mainSelect)
		If count IsNot Nothing Then
			res = Convert.ToInt64(count)
		End If
		Return res
	End Function

	Public Overrides Function FindObj(searchParams As SearchParams) As String()
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

		'''' main sql
		Dim mainSelect = String.Format("Select {0} [{1}].[guid] FROM {2} {3} ORDER BY [{4}].[{5}] {6} ", topSql, Name, fromSql, whereSql, sortTableName, sortField, sortModeStr)

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
		Dim sql = String.Format("SELECT [json] FROM [dbo].[{0}] WHERE [guid] = '{1}'", Name, id)
		Dim jsonObj = MSSQLSRVUtils.ExecSQLScalar(ConnectionString, sql)
		If (jsonObj IsNot Nothing) Then
			Dim json = jsonObj.ToString
			res = JsonConverter.Deserialize(json, SupportedType)
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
		Dim json = JsonConverter.Serialize(obj)
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

	Public Overrides Function GetObjects(Of T As ObjBase)(objIds As String(), Optional bp As BetweenParam = Nothing) As IEnumerable(Of T)
		Return GetObjects(objIds, bp).Select(Function(o) CType(o, T))
	End Function

	Public Overrides Function GetObjects(objIds As String(), Optional bp As BetweenParam = Nothing) As IEnumerable(Of ObjBase)
		CheckDB()

		objIds = GetBetweenIds(objIds, bp)

		Dim resList = New List(Of ObjBase)
		Dim sql = String.Empty
		Dim i = 0
		If (objIds IsNot Nothing AndAlso objIds.Any) Then
			For Each id In objIds
				If String.IsNullOrWhiteSpace(sql) Then
					sql = String.Format("SELECT [json] , {2} as tmp FROM [dbo].[{0}] WHERE ([guid] = '{1}')", Name, id, i)
				Else
					sql += String.Format(" Union SELECT [json] , {2} as tmp FROM [dbo].[{0}] WHERE ([guid] = '{1}')", Name, id, i)
				End If
				i += 1
			Next
			sql += " order by tmp"

			Dim jsonObjList = MSSQLSRVUtils.GetObjectList(ConnectionString, sql)
			If (jsonObjList IsNot Nothing) Then
				Dim tmpList = jsonObjList.Select(Function(j) CType(JsonConverter.Deserialize(j(0).ToString, SupportedType), ObjBase))
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
		If (Not String.IsNullOrWhiteSpace(idFromDb) AndAlso idFromDb.ToString = id) Then
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
		Dim indexTableName = String.Format("{0}_{1}", Name, indexing.Name.Replace(".", "_"))

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

					Dim value = crit.Value
					If (TypeOf (crit.Value) Is DateTime) Then
						value = CType(crit.Value, DateTime).Ticks
					End If
					parameters.Add(New SqlParameter(pName, value))
					i += 1
				Else
					Throw New Exception("Поле " + crit.Field + " не является индексируемым")
				End If
			Next
		End If

		If String.IsNullOrWhiteSpace(where) Then
			Return Nothing
		Else
			Return New SqlHelper(" WHERE " + where, parameters)
		End If
	End Function

	Private Sub Save(connStr As String, id As String, json As String)
		Dim sql = String.Format(My.Resources.InsertMainSQL, Name, id, json)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
	End Sub

	Private Sub Update(connStr As String, id As String, json As String)
		Dim sql = String.Format("UPDATE [{0}] SET [json] = @p1 WHERE [guid] = @p2", Name)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql, {New SqlParameter("@p1", json), New SqlParameter("@p2", id)})
	End Sub

	Private Shared Function GetBetweenIds(objIds As String(), Optional bp As BetweenParam = Nothing)
		If objIds IsNot Nothing Then
			If bp Is Nothing Then
				Return objIds
			Else
				Dim start = bp.StartValue
				If (start < 0) Then
					start = 0
				End If
				If start >= objIds.Length Then
					start = objIds.Length - 1
				End If

				Dim endV = bp.EndValue
				If (endV < 0) Then
					endV = 0
				End If
				If endV >= objIds.Length Then
					endV = objIds.Length - 1
				End If

				Dim len = endV - start + 1
				Dim arr(len - 1) As String
				Array.Copy(objIds, start, arr, 0, len)
				Return arr
			End If
		End If
		Return Nothing
	End Function

	Public Overrides Sub RemoveAllObjects()
		CheckDB()
		Dim sql = String.Format("DELETE FROM [dbo].[{0}]", Name)
		MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
	End Sub
End Class
