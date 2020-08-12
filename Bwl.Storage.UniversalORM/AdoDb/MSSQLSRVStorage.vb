Imports System.Data.SqlClient

Public Class MSSQLSRVStorage
    Inherits CommonObjStorage

    Private _name As String
    Private _dbName As String
    Private _connectionStringBld As SqlConnectionStringBuilder

    Public Sub New(connStringBld As SqlConnectionStringBuilder, type As Type, dbName As String)
        MyBase.New(type)
        _name = type.Name
        _dbName = dbName

        ConnectionStringBld = New SqlConnectionStringBuilder(connStringBld.ConnectionString)
        _connectionStringBld.InitialCatalog = _dbName

        CheckDB()
    End Sub


    Public Overrides Sub AddObj(obj As ObjBase)
        CheckDB()
        Dim json = CfJsonConverter.Serialize(obj)
        Save(ConnectionString, obj.ID, json, obj.GetType)
        For Each indexing In _indexingMembers
            Dim indexTableName = GetIndexTableName(indexing)
            Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
            If (TypeOf (indexValue) Is DateTime) Then
                indexValue = CType(indexValue, DateTime).Ticks
            End If
            AddOrUpdateIndex(indexTableName, obj.ID, indexValue)
        Next
    End Sub

    Public Overrides Sub AddObjects(objects As ObjBase())
        For Each obj In objects
            AddObj(obj)
        Next
    End Sub

    Public Overrides Function GetSomeFieldDistinct(fieldName As String) As IEnumerable(Of String)
        Dim res = New List(Of String)
        CheckDB()
        For Each indexing In _indexingMembers
            If indexing.Name.ToLower = fieldName.ToLower Then
                Dim indexTableName = GetIndexTableName(indexing)
                Dim query = "SELECT DISTINCT [value] FROM " + indexTableName
                Dim values = MSSQLSRVUtils.GetObjectList(_connectionStringBld.ConnectionString, query)
                If values IsNot Nothing AndAlso values.Any Then
                    res.AddRange(values.Select(Function(v) v.First.ToString))
                End If
            End If
        Next
        Return res
    End Function

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

    <Obsolete("DO NOT use this method unless absolutely necessary", False)>
    Public Overrides Function ExecSqlGetObjects(sqlString As String) As List(Of List(Of Object))
        Return MSSQLSRVUtils.GetObjectList(ConnectionString, sqlString)
    End Function

    <Obsolete("DO NOT use this method unless absolutely necessary", False)>
    Public Overrides Sub ExecSql(sqlString As String)
        MSSQLSRVUtils.ExecSQL(ConnectionString, sqlString)
    End Sub

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
        Dim sortField = "id"
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
        Dim mainSelect = String.Empty
        If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) AndAlso (searchParams.SelectOptions.SelectMode = SelectMode.Between) Then
            betweenSql = String.Format("OFFSET {2} ROWS FETCH NEXT {3} ROWS ONLY", Name, fromSql, searchParams.SelectOptions.StartValue, searchParams.SelectOptions.EndValue - searchParams.SelectOptions.StartValue + 1)
        End If
        mainSelect = String.Format("Select {0} [{1}].[guid] FROM {2} {3} ORDER BY [{4}].[{5}] {6} {7}", topSql, Name, fromSql, whereSql, sortTableName, sortField, sortModeStr, betweenSql)
        If ((topSql <> "") And (searchParams IsNot Nothing AndAlso searchParams.SortParam Is Nothing)) Then
            mainSelect = String.Format("Select {0} [{1}].[guid] FROM {2} {3}", topSql, Name, fromSql, whereSql)
        End If
        If (searchParams Is Nothing) Then 'Or ((searchParams IsNot Nothing) AndAlso (searchParams.SortParam Is Nothing)) Then
            mainSelect = String.Format("SELECT {3} [{0}].[guid] FROM {1} {2}", Name, fromSql, whereSql, topSql)
        End If

        '''' main sql

        Dim list = MSSQLSRVUtils.GetObjectList(ConnectionString, mainSelect, parameters)
        If (list IsNot Nothing AndAlso list.Any) Then
            Dim resList = list.Select(Function(d) d(0).ToString)
            Return resList.ToArray
        Else
            Return {}
        End If
    End Function

    Public Overrides Function StrToObj(jsonObj As String, typeName As String) As ObjBase
        Dim res As ObjBase = Nothing
        If (typeName = "-") Or String.IsNullOrWhiteSpace(typeName) Then
            typeName = SupportedType.AssemblyQualifiedName
        End If
        If (jsonObj IsNot Nothing) Then
            Dim json = jsonObj.ToString
            res = CfJsonConverter.Deserialize(json, Type.GetType(typeName.ToString))
        End If
        Return res
    End Function

    Public Overloads Overrides Function StrToObj(Of T As ObjBase)(jsonObj As String, typeName As String) As T
        Return StrToObj(jsonObj, typeName)
    End Function

    Public Overrides Function GetObj(id As String) As ObjBase
        CheckDB()
        Dim res As ObjBase = Nothing
        Dim sql = String.Format("SELECT [json], [type] FROM [dbo].[{0}] WHERE [guid] = '{1}'", Name, id)
        Dim vals = MSSQLSRVUtils.GetObjectList(ConnectionString, sql)
        If vals IsNot Nothing AndAlso vals.Any Then
            Dim jsonObj = vals(0)(0)
            Dim typeName = vals(0)(1)

            If (typeName = "-") Or String.IsNullOrWhiteSpace(typeName) Then
                typeName = SupportedType.AssemblyQualifiedName
            End If

            If (jsonObj IsNot Nothing) Then
                Dim json = jsonObj.ToString
                Try
                    res = CfJsonConverter.Deserialize(json, Type.GetType(typeName.ToString))
                Catch exc As Exception
                    Dim polo = exc.Message
                End Try
            End If
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
            Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
            If (TypeOf (indexValue) Is DateTime) Then
                indexValue = CType(indexValue, DateTime).Ticks
            End If
            AddOrUpdateIndex(indexTableName, obj.ID, indexValue)
        Next
    End Sub

    Public ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    Public Property ConnectionStringBld As SqlConnectionStringBuilder
        Get
            Return _connectionStringBld
        End Get
        Set(value As SqlConnectionStringBuilder)
            _connectionStringBld = value
        End Set
    End Property

    Public Overrides Function GetObjects(Of T As ObjBase)(searchParams As SearchParams) As IEnumerable(Of T)
        Return GetObjects(searchParams).Select(Function(o) CType(o, T))
    End Function

    Public Overrides Function GetObjects(searchParams As SearchParams) As IEnumerable(Of ObjBase)
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
        Dim sortField = "id"
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
                Throw New Exception("MSSQLSRVStorage.GetObjects _ BadSortParam _ index " + indexInfo.Name + " not found.")
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
        Dim mainSelect = String.Empty
        If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) AndAlso (searchParams.SelectOptions.SelectMode = SelectMode.Between) Then
            betweenSql = String.Format("OFFSET {2} ROWS FETCH NEXT {3} ROWS ONLY", Name, fromSql, searchParams.SelectOptions.StartValue, searchParams.SelectOptions.EndValue - searchParams.SelectOptions.StartValue + 1)
        End If
        mainSelect = String.Format("Select {0} [{1}].[guid], [{1}].[json], [{1}].[type] FROM {2} {3} ORDER BY [{4}].[{5}] {6} {7}", topSql, Name, fromSql, whereSql, sortTableName, sortField, sortModeStr, betweenSql)
        If ((topSql <> "") And (searchParams IsNot Nothing AndAlso searchParams.SortParam Is Nothing)) Then
            mainSelect = String.Format("Select {0} [{1}].[guid], [{1}].[json], [{1}].[type] FROM {2} {3}", topSql, Name, fromSql, whereSql)
        End If
        If (searchParams Is Nothing) Then 'Or ((searchParams IsNot Nothing) AndAlso (searchParams.SortParam Is Nothing)) Then
            mainSelect = String.Format("SELECT {3} [{0}].[guid], [{0}].[json], [{0}].[type] FROM {1} {2}", Name, fromSql, whereSql, topSql)
        End If

        '''' main sql
        Dim resList = New List(Of ObjBase)
        Dim valuesObjList = MSSQLSRVUtils.GetObjectList(ConnectionString, mainSelect, parameters)
        If (valuesObjList IsNot Nothing) Then
            Dim tmpList = valuesObjList.Select(
                Function(val)
                    Dim jsonObj = val(1)
                    Dim typeName = val(2)
                    If (typeName = "-") Or String.IsNullOrWhiteSpace(typeName) Then
                        typeName = SupportedType.AssemblyQualifiedName
                    End If

                    Dim res As ObjBase = Nothing
                    If (jsonObj IsNot Nothing) Then
                        Dim json = jsonObj.ToString
                        Try
                            res = CfJsonConverter.Deserialize(json, Type.GetType(typeName.ToString))
                        Catch exc As Exception
                            Dim polo = exc.Message
                        End Try
                    End If
                    Return res
                End Function)
            resList.AddRange(tmpList.ToArray)
        End If

        Return resList
    End Function

    Public Overrides Function GetObjects(Of T As ObjBase)(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of T)
        Return GetObjects(objIds, sortParam).Select(Function(o) CType(o, T))
    End Function

    Public Overrides Function GetObjects(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase)
        CheckDB()

        '''' sorting
        Dim sortModeStr = "ASC"
        Dim sortField = ""
        Dim sortTableName = ""
        If sortParam IsNot Nothing Then
            If sortParam.SortMode = SortMode.Descending Then
                sortModeStr = "DESC"
            End If

            Dim indexInfo = _indexingMembers.FirstOrDefault(Function(indInf) indInf.Name = sortParam.Field)
            If (indexInfo IsNot Nothing) Then
                sortField = " , [value]"
                sortTableName = ", [dbo].[" + GetIndexTableName(indexInfo) + "] s "
            Else
                Throw New Exception("MSSQLSRVStorage.GetObjects _ BadSortParam _ index " + indexInfo.Name + " not found.")
            End If
        End If

        Dim resList = New List(Of ObjBase)
        If (objIds IsNot Nothing AndAlso objIds.Any) Then
            Dim strIds = " ( ( t.[guid] = '" + String.Join("' ) or ( t.[guid] = '", objIds) + "' ) ) "
            Dim sql = String.Format("SELECT [json] , [type] " + sortField + " FROM [dbo].[{0}] t " + sortTableName + "  WHERE {1}", Name, strIds)

            If (sortTableName <> "") Then
                sql += " and  (s.[guid] = t.[guid]) "
                sql += " order by [value] " + sortModeStr
            End If

            Dim ValuesObjList = MSSQLSRVUtils.GetObjectList(ConnectionString, sql)
            If (ValuesObjList IsNot Nothing) Then
                Dim tmpList = ValuesObjList.Select(
                    Function(val)
                        Dim jsonObj = val(0)
                        Dim typeName = val(1)
                        If (typeName = "-") Or String.IsNullOrWhiteSpace(typeName) Then
                            typeName = SupportedType.AssemblyQualifiedName
                        End If

                        Dim res As ObjBase = Nothing
                        If (jsonObj IsNot Nothing) Then
                            Dim json = jsonObj.ToString
                            Try
                                res = CfJsonConverter.Deserialize(json, Type.GetType(typeName.ToString))
                            Catch exc As Exception
                                Dim polo = exc.Message
                            End Try
                        End If
                        Return res
                    End Function)
                resList.AddRange(tmpList.ToArray)
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
    End Sub

    'Private Sub SaveIndex(tableName As String, id As String, value As Object)
    '	Dim sql = String.Format(My.Resources.InsertIndexSQL, tableName, id, "@p1")
    '	MSSQLSRVUtils.ExecSQL(ConnectionString, sql, {New SqlParameter("@p1", value)})
    'End Sub

    Private Sub AddOrUpdateIndex(tableName As String, id As String, value As Object)
        Dim sql = String.Empty
        Dim params = {New SqlParameter("@p1", value)}
        If value IsNot Nothing Then
            sql = String.Format("select [guid] from [{0}] where [guid]='{1}'", tableName, id)
            Dim indexId = MSSQLSRVUtils.ExecSQLScalar(ConnectionString, sql)
            If Not String.IsNullOrEmpty(indexId) Then
                sql = String.Format("UPDATE [{0}] SET [value] = @p1 WHERE [guid] = '{1}'", tableName, id)
            Else
                sql = String.Format("INSERT INTO [dbo].[{0}] ([guid],[value]) VALUES('{1}',@p1)", tableName, id)
            End If
        Else
            sql = String.Format("DELETE FROM [{0}] WHERE [guid] = '{1}'", tableName, id)
            params = Nothing
        End If
        MSSQLSRVUtils.ExecSQL(ConnectionString, sql, params)
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
        Dim fromSQl As String = " [" + Name + "] "
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

    Private Function GenerateWhereSql(criterias As IEnumerable(Of FindCriteria), sort As SortParam, Optional paramStartValue As Integer = 0) As SqlHelper
        Dim where = String.Empty
        Dim multipleConditions = New FindCondition() {FindCondition.multipleEqual,
                                                      FindCondition.multipleLikeEqual,
                                                      FindCondition.multipleNotEqual,
                                                      FindCondition.multipleNotLikeEqual,
                                                      FindCondition.multipleGreater,
                                                      FindCondition.multipleLess,
                                                      FindCondition.multipleGreaterOrEqual,
                                                      FindCondition.multipleLessOrEqual}
        Dim findCriteriaConditions = New FindCondition() {FindCondition.findCriteria,
                                                          FindCondition.findCriteriaNegative}
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
        Dim i = paramStartValue
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
                        If multipleConditions.Any(Function(f) f = crit.Condition) Then
                            str = GetMultipleConditionString(i, parameters, crit.Condition, indexTableName, value)
                        ElseIf findCriteriaConditions.Any(Function(f) f = crit.Condition) Then
                            Dim findCriteria = CfJsonConverter.Deserialize(Of FindCriteria())(value)
                            Dim val = GenerateWhereSql(findCriteria, sort, i)
                            parameters.AddRange(val.Parameters)
                            str = If(crit.Condition = FindCondition.findCriteriaNegative, " NOT (", " (") + val.SQL.Remove(0, 7) + ") "
                            i += (val.Parameters.Count + 1)
                        Else

                            Dim pName = "@p" + i.ToString
                            Select Case crit.Condition
                                Case FindCondition.equal
                                    str = String.Format(" ([{0}].[value] = {1}) ", indexTableName, pName)
                                Case FindCondition.greater
                                    str = String.Format(" ([{0}].[value] > {1}) ", indexTableName, pName)
                                Case FindCondition.less
                                    str = String.Format(" ([{0}].[value] < {1}) ", indexTableName, pName)
                                Case FindCondition.notEqual
                                    str = String.Format(" ([{0}].[value] <> {1}) ", indexTableName, pName)
                                Case FindCondition.likeEqual
                                    str = String.Format(" ([{0}].[value] LIKE {1}) ", indexTableName, pName)
                                Case FindCondition.notLikeEqual
                                    str = String.Format(" ([{0}].[value] NOT LIKE {1}) ", indexTableName, pName)
                                Case FindCondition.greaterOrEqual
                                    str = String.Format(" ([{0}].[value] >= {1}) ", indexTableName, pName)
                                Case FindCondition.lessOrEqual
                                    str = String.Format(" ([{0}].[value] <= {1}) ", indexTableName, pName)
                            End Select
                            If (TypeOf (value) Is DateTime) Then
                                value = CType(value, DateTime).Ticks
                            End If
                            parameters.Add(New SqlParameter(pName, value))
                            i += 1
                        End If
                        If (String.IsNullOrEmpty(where)) Then
                            where += str
                        Else
                            where += " AND " + str
                        End If
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

    Private Shared Function GetMultipleConditionString(ByRef i As Integer, ByRef parameters As List(Of SqlParameter), condition As FindCondition, indexTableName As String, jsonValues As String) As String
        Dim res = ""
        Dim valuesFromArrayOfStrings = CfJsonConverter.Deserialize(Of String())(jsonValues)
        Dim valuesToAggregate = New List(Of String)
        Dim multipleNegativeConditions = New FindCondition() {FindCondition.multipleNotEqual,
                                                              FindCondition.multipleNotLikeEqual}
        Dim multipleValueAggregator = If(multipleNegativeConditions.Any(Function(f) f = condition), " AND ", " OR ")
        For Each value As String In valuesFromArrayOfStrings
            Dim pName = "@p" + i.ToString
            Select Case condition
                Case FindCondition.multipleEqual
                    valuesToAggregate.Add(String.Format(" ([{0}].[value] = {1}) ", indexTableName, pName))
                Case FindCondition.multipleGreater
                    valuesToAggregate.Add(String.Format(" ([{0}].[value] > {1}) ", indexTableName, pName))
                Case FindCondition.multipleLess
                    valuesToAggregate.Add(String.Format(" ([{0}].[value] < {1}) ", indexTableName, pName))
                Case FindCondition.multipleNotEqual
                    valuesToAggregate.Add(String.Format(" ([{0}].[value] <> {1}) ", indexTableName, pName))
                Case FindCondition.multipleLikeEqual
                    valuesToAggregate.Add(String.Format(" ([{0}].[value] LIKE {1}) ", indexTableName, pName))
                Case FindCondition.multipleNotLikeEqual
                    valuesToAggregate.Add(String.Format(" ([{0}].[value] NOT LIKE {1}) ", indexTableName, pName))
                Case FindCondition.multipleGreaterOrEqual
                    valuesToAggregate.Add(String.Format(" ([{0}].[value] >= {1}) ", indexTableName, pName))
                Case FindCondition.multipleLessOrEqual
                    valuesToAggregate.Add(String.Format(" ([{0}].[value] <= {1}) ", indexTableName, pName))
            End Select
            Dim dateResult As Date
            If (Date.TryParse(value, dateResult)) Then
                value = dateResult.Ticks.ToString()
            End If
            parameters.Add(New SqlParameter(pName, value))
            i += 1
        Next
        res = String.Format("({0})", valuesToAggregate.Aggregate(Function(f, t) f + multipleValueAggregator + t))
        Return res
    End Function

    Private Sub Save(connStr As String, id As String, json As String, type As Type)
        Dim rtype = IIf(type.AssemblyQualifiedName = SupportedType.AssemblyQualifiedName, "-", type.AssemblyQualifiedName)
        Dim parameters = {New SqlParameter("@p1", id), New SqlParameter("@p2", json), New SqlParameter("@p3", rtype)}
        Dim sql = String.Format("INSERT INTO [dbo].[{0}] ([guid] ,[json], [type]) VALUES(@p1, @p2, @p3)", Name)
        MSSQLSRVUtils.ExecSQL(ConnectionString, sql, parameters)
    End Sub

    'Private Sub Save_old(connStr As String, id As String, json As String, type As Type)
    '	Dim rtype = IIf(type.AssemblyQualifiedName = SupportedType.AssemblyQualifiedName, "-", type.AssemblyQualifiedName)
    '	Dim sql = String.Format("INSERT INTO [dbo].[{0}] ([guid] ,[json], [type]) VALUES('{1}', '{2}', '{3}')", Name, id, json, rtype)
    '	MSSQLSRVUtils.ExecSQL(ConnectionString, sql)
    'End Sub

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
