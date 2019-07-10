﻿Imports System.IO
Imports Npgsql
Imports NpgsqlTypes

Public Class PgStorage
    Inherits CommonObjStorage

    Private ReadOnly _name As String
    Private ReadOnly _dbName As String

    Public Property ConnectionStringBld As NpgsqlConnectionStringBuilder

    Private ReadOnly Property ConnectionString As String
        Get
            Return ConnectionStringBld.ConnectionString
        End Get
    End Property

    Public Sub New(connStringBld As NpgsqlConnectionStringBuilder, type As Type, dbName As String)
        MyBase.New(type)
        _name = type.Name
        _dbName = dbName
        ConnectionStringBld = connStringBld

        CheckDb()
    End Sub

    Public Overrides Sub AddObj(obj As ObjBase)
        CheckDb()
        Dim json = CfJsonConverter.Serialize(obj)
        Save(ConnectionString, obj.ID, json, obj.GetType)

        For Each indexing In _indexingMembers
            Dim indexName = GetIndexName(indexing)
            Try
                Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
                If indexValue Is Nothing Then indexValue = "" 'TODO: исправить
                If (TypeOf (indexValue) Is DateTime) Then
                    indexValue = CType(indexValue, DateTime).Ticks
                End If
                SaveIndex(Name, indexName, obj.ID, indexValue)
            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try
        Next
    End Sub

    Public Overrides Function GetSomeFieldDistinct(fieldName As String) As IEnumerable(Of String)
        Dim res = New List(Of String)
        CheckDb()
        For Each indexing In _indexingMembers
            If indexing.Name.ToLower = fieldName.ToLower Then
                Dim indexName = GetIndexName(indexing)
                Dim query = "SELECT DISTINCT """ + indexName + """ FROM """ + _name + """"
                Dim values = PgUtils.GetObjectList(_ConnectionStringBld.ConnectionString, query)
                If values IsNot Nothing AndAlso values.Any Then
                    res.AddRange(values.Select(Function(v) v.First.ToString))
                End If
            End If
        Next
        Return res
    End Function

    Public Overrides Sub AddObjects(objects As ObjBase())
        For Each obj In objects
            AddObj(obj)
        Next
    End Sub

    Public Overrides Function FindObjCount(searchParams As SearchParams) As Long
        Dim res As Long = 0
        CheckDb()

        '''' TOP
        Dim topSql = String.Empty
        If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) Then
            If searchParams.SelectOptions.TopValue > 0 Then
                topSql = " LIMIT " + searchParams.SelectOptions.TopValue.ToString + " "
            End If
        End If

        ' '''' sorting
        'Dim sortModeStr = "ASC"
        'Dim sortField = "guid"
        'Dim sortColName = Name
        'If (searchParams IsNot Nothing) AndAlso (searchParams.SortParam IsNot Nothing) Then
        '	If searchParams.SortParam.SortMode = SortMode.Descending Then
        '		sortModeStr = "DESC"
        '	End If

        '	Dim indexInfo = _indexingMembers.FirstOrDefault(Function(indInf) indInf.Name = searchParams.SortParam.Field)
        '	If (indexInfo IsNot Nothing) Then
        '		sortField = "value"
        '		sortColName = GetIndexName(indexInfo)
        '	Else
        '		Throw New Exception("FBStorage.FindObj _ BadSortParam _ index " + searchParams.SortParam.Field + " not found.")
        '	End If
        'End If

        Dim crit As IEnumerable(Of FindCriteria) = Nothing
        If (searchParams IsNot Nothing) Then
            crit = searchParams.FindCriterias
        End If

        Dim sort As SortParam = Nothing
        If (searchParams IsNot Nothing) Then
            sort = searchParams.SortParam
        End If

        '''' where
        Dim whereSql = String.Empty
        Dim parameters As NpgsqlParameter() = Nothing
        Dim helper = GenerateWhereSql(crit, sort)
        If helper IsNot Nothing Then
            whereSql = helper.SQL
            parameters = helper.Parameters.ToArray
        End If

        '''' main sql
        Dim mainSelect = String.Format("SELECT COUNT(*) FROM (SELECT GUID FROM ""{0}"" {1} {2}) AS PSEUDONYM", Name, whereSql, topSql)
        Dim count = PgUtils.ExecSqlScalar(ConnectionString, mainSelect, parameters)
        If count IsNot Nothing Then
            res = Convert.ToInt64(count)
        End If
        Return res
    End Function

    Public Overrides Function FindObj(searchParams As SearchParams) As String()
        CheckDb()

        ' TODO: надо сделать проверку типа данных, т.к. postgresql сильно к ним привязывается и ничего не исправляет сам
        '''' TOP
        Dim topSql = String.Empty
        If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) AndAlso (searchParams.SelectOptions.SelectMode = SelectMode.Top) Then
            If searchParams.SelectOptions.TopValue > 0 Then
                topSql = " LIMIT " + searchParams.SelectOptions.TopValue.ToString + " "
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
        Dim parameters As NpgsqlParameter() = Nothing
        Dim helper = GenerateWhereSql(crit, sort)
        If helper IsNot Nothing Then
            whereSql = helper.SQL
            parameters = helper.Parameters.ToArray
        End If

        Dim betweenSql = String.Empty
        Dim mainSelect = String.Empty
        If (searchParams IsNot Nothing) AndAlso (searchParams.SelectOptions IsNot Nothing) AndAlso (searchParams.SelectOptions.SelectMode = SelectMode.Between) Then
            'BUG: 'mainSelect = String.Format("SELECT FIRST {1} SKIP {2} GUID FROM {0}", Name, searchParams.SelectOptions.EndValue - searchParams.SelectOptions.StartValue + 1, searchParams.SelectOptions.StartValue)
            mainSelect = String.Format("SELECT GUID FROM ""{0}"" {1} ORDER BY ""{4}"" {5} LIMIT {2} OFFSET {3}", Name, whereSql,
                                       searchParams.SelectOptions.EndValue - searchParams.SelectOptions.StartValue + 1,
                                       searchParams.SelectOptions.StartValue, sortField, sortModeStr)
        Else
            '''' main sql			
            If (searchParams Is Nothing) Or ((searchParams IsNot Nothing) AndAlso (searchParams.SortParam Is Nothing)) Then
                mainSelect = String.Format("SELECT GUID FROM ""{0}"" {1} {2}", Name, whereSql, topSql)
            ElseIf (searchParams.SortParam IsNot Nothing) Then
                mainSelect = String.Format("SELECT GUID FROM ""{0}"" {1} ORDER BY ""{3}"" {4} {2}", Name, whereSql, topSql, sortField, sortModeStr)
            End If
        End If
        Dim list = PgUtils.GetObjectList(ConnectionString, mainSelect, parameters)
        If (list IsNot Nothing AndAlso list.Any) Then
            Dim resList = list.Select(Function(d) d(0).ToString)
            Return resList.ToArray
        Else
            Return {}
        End If
    End Function

    Public Overrides Function GetObj(id As String) As ObjBase
        CheckDb()

        Dim res As ObjBase = Nothing
        Dim sql = String.Format("SELECT json, type FROM ""{0}"" WHERE guid = '{1}'", Name, id)
        Dim vals = PgUtils.GetObjectList(ConnectionString, sql)
        If vals IsNot Nothing AndAlso vals.Any Then
            Dim jsonObj = vals(0)(0)
            Dim typeName = vals(0)(1)

            If (typeName = "-") Or String.IsNullOrWhiteSpace(typeName) Then
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
        CheckDb()
        Dim sql = String.Format("DELETE FROM ""{0}"" WHERE guid like '{1}'", Name, id)
        PgUtils.ExecSql(ConnectionString, sql)
    End Sub

    Public Overrides Sub UpdateObj(obj As ObjBase)
        CheckDb()
        Dim json = CfJsonConverter.Serialize(obj)
        Update(ConnectionString, obj.ID, json)

        For Each indexing In _indexingMembers
            Dim indexName = GetIndexName(indexing)
            Try
                Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
                If indexValue Is Nothing Then indexValue = ""  'TODO: исправить
                If (TypeOf (indexValue) Is DateTime) Then
                    indexValue = CType(indexValue, DateTime).Ticks
                End If
                UpdateIndex(indexName, obj.ID, indexValue)
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
        CheckDb()

        '''' sorting
        Dim sortModeStr = "ASC"
        Dim sortField = ""
        If sortParam IsNot Nothing Then
            If sortParam.SortMode = SortMode.Descending Then
                sortModeStr = "DESC"
            End If

            Dim indexInfo = _indexingMembers.FirstOrDefault(Function(indInf) indInf.Name = sortParam.Field)
            If (indexInfo IsNot Nothing) Then
                sortField = GetIndexName(indexInfo)
            Else
                Throw New Exception("FBtorage.GetObjects _ BadSortParam _ index " + indexInfo.Name + " not found.")
            End If
        End If

        Dim resList = New List(Of ObjBase)
        If (objIds IsNot Nothing AndAlso objIds.Any) Then
            Dim strIds = " ( ( t.GUID = '" + String.Join("' ) or ( t.GUID = '", objIds) + "' ) ) "
            Dim sql = String.Format("SELECT JSON, TYPE {1} FROM ""{0}"" t  WHERE {2}", Name, If(Not String.IsNullOrWhiteSpace(sortField), ", """ + sortField + """", ""), strIds)

            If (sortField <> "") Then
                'sql += " and  (s.GUID = t.GUID) "
                sql += String.Format(" ORDER BY ""{0}"" {1}", sortField, sortModeStr)
            End If

            Dim valuesObjList = PgUtils.GetObjectList(ConnectionString, sql)
            If (valuesObjList IsNot Nothing) Then
                Dim tmpList = valuesObjList.Select(Function(j)
                                                       Dim typeName = j(1)
                                                       If (typeName = "-") Or String.IsNullOrWhiteSpace(typeName) Then
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
        CheckDb()

        Dim res = False
        Dim sql = String.Format("SELECT GUID FROM ""{0}"" WHERE GUID = '{1}'", Name, id)
        Dim idFromDb = PgUtils.ExecSqlScalar(ConnectionString, sql)
        If (Not String.IsNullOrWhiteSpace(idFromDb) AndAlso idFromDb.ToString.Replace(" ", "") = id) Then
            res = True
        End If

        Return res
    End Function

    Private Sub CheckDb()
        PgUtils.CreateDb(ConnectionStringBld, _dbName)
        CreateMainTable(ConnectionString, Name)
    End Sub

    Private Shared Sub CreateMainTable(connString As String, tableName As String)
        If (Not PgUtils.TableExists(connString, tableName)) Then
            Threading.Thread.Sleep(1000)
            If (Not PgUtils.TableExists(connString, tableName)) Then
                Dim sql = String.Format(My.Resources.CreateMainTableSQL, tableName)
                PgUtils.ExecSql(connString, sql)
            End If
        End If
    End Sub

    Private Sub SaveIndex(tableName As String, columnName As String, id As String, value As Object)
        Dim sql = String.Format(My.Resources.UpdateIndexSQL, tableName, columnName, id, "@p1")
        PgUtils.ExecSql(ConnectionString, sql, {New NpgsqlParameter("@p1", value)})
    End Sub

    Private Sub UpdateIndex(columnName As String, id As String, value As Object)
        Dim sql = String.Format("UPDATE ""{0}"" SET {1} = @p2 WHERE GUID = @p3", Name, """" + columnName + """")
        PgUtils.ExecSql(ConnectionString, sql, {New NpgsqlParameter("@p2", value), New NpgsqlParameter("@p3", id)})
    End Sub

    Private Function GetIndexName(indexing As IndexInfo) As String
        Dim indexName = indexing.Name.Replace(".", "_")

        Dim t = indexing.Type
        Dim indexesSql = String.Format(My.Resources.GetIndexesSql, Name)
        Dim sqLfields = String.Format("select R.column_name from
                                            information_schema.columns as F, 
                                            ({2}) as R 
                                        where (F.column_name = R.column_name 
                                            and R.table_name = '{0}' 
                                            and R.column_name = '{1}')",
                                      Name, indexName, indexesSql)
        Dim fields = PgUtils.ExecSqlScalar(ConnectionString, sqLfields)
        If fields Is Nothing Then
            Dim listQuery As New PgBatchExecution(New NpgsqlConnection(ConnectionString))
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
                Case GetType([Enum])
                    sql = String.Format(My.Resources.AddIntColumn, Name, indexName)
                Case GetType(Double)
                    sql = String.Format(My.Resources.AddFloatColumn, Name, indexName)
                Case GetType(DateTime)
                    sql = String.Format(My.Resources.AddBigIntColumn, Name, indexName)
                Case GetType(Long)
                    sql = String.Format(My.Resources.AddBigIntColumn, Name, indexName)
                Case GetType(Boolean)
                    sql = String.Format(My.Resources.AddBoolColumn, Name, indexName)
                Case Else
                    Dim enumType = Type.GetType("System.Enum, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
                    If t.BaseType = enumType Then
                        sql = String.Format(My.Resources.AddStringColumn, Name, indexName, Byte.MaxValue.ToString)
                    Else
                        Throw New Exception("Обнаружен не поддерживаемый тип индексируемого поля " + indexName + " _ " + t.FullName)
                    End If
            End Select
            listQuery.SqlStatements.Add(sql)
            listQuery.SqlStatements.Add(String.Format("CREATE INDEX ""IX_{0}_{1}"" ON ""{0}""(""{1}"")", Name, indexName))

            listQuery.Execute()
        End If

        Return indexName
    End Function

    Private Function GenerateWhereSql(criterias As IEnumerable(Of FindCriteria), sort As SortParam) As SqlHelper
        Dim where = String.Empty
        Dim parameters As New List(Of NpgsqlParameter)()
        Dim i = 0

        If criterias IsNot Nothing Then
            For Each crit In criterias
                Dim value = crit.Value
                If (TypeOf (value) Is DateTime) Then
                    value = CType(value, DateTime).Ticks
                End If
                Dim valueAsString = value.ToString()
                Dim valueType = NpgsqlDbType.Text
                Try
                    Dim intValue = Integer.Parse(valueAsString)
                    valueType = NpgsqlDbType.Integer
                Catch
                    Try
                        Dim longValue = Long.Parse(valueAsString)
                        valueType = NpgsqlDbType.Bigint
                    Catch
                        Try
                            Dim boolValue = Boolean.Parse(valueAsString)
                            valueType = NpgsqlDbType.Boolean
                        Catch
                        End Try
                    End Try
                End Try

                If crit.Field.ToLower = "id" Then
                    Dim pName = "@p" + i.ToString
                    Dim str = String.Format(" (GUID = {0}) ", pName)
                    If (String.IsNullOrEmpty(where)) Then
                        where += str
                    Else
                        where += " AND " + str
                    End If
                    parameters.Add(New NpgsqlParameter(pName, value))
                Else
                    Dim ind = _indexingMembers.FirstOrDefault(Function(f) f.Name = crit.Field)
                    If (ind IsNot Nothing) Then
                        Dim indexName = GetIndexName(ind)
                        Dim str = String.Empty
                        Dim pName = "@p" + i.ToString
                        Const QUOTE As String = """"
                        Select Case crit.Condition
                            Case FindCondition.equal
                                str = String.Format(" ({0} = {1}) ", QUOTE + indexName + QUOTE, pName)
                            Case FindCondition.greater
                                str = String.Format(" ({0} > {1}) ", QUOTE + indexName + QUOTE, pName)
                            Case FindCondition.less
                                str = String.Format(" ({0} < {1}) ", QUOTE + indexName + QUOTE, pName)
                            Case FindCondition.notEqual
                                str = String.Format(" ({0} <> {1}) ", QUOTE + indexName + QUOTE, pName)
                            Case FindCondition.likeEqual
                                str = If(valueType = NpgsqlDbType.Text,
                                        String.Format(" ({0} LIKE {1}) ", QUOTE + indexName + QUOTE, pName),
                                        String.Format(" ({0} = {1}) ", QUOTE + indexName + QUOTE, pName))
                            Case FindCondition.notLikeEqual
                                str = If(valueType = NpgsqlDbType.Text,
                                         String.Format(" ({0} NOT LIKE {1}) ", QUOTE + indexName + QUOTE, pName),
                                         String.Format(" ({0} <> {1}) ", QUOTE + indexName + QUOTE, pName))
                            Case FindCondition.greaterOrEqual
                                str = String.Format(" ({0} >= {1}) ", QUOTE + indexName + QUOTE, pName)
                            Case FindCondition.lessOrEqual
                                str = String.Format(" ({0} <= {1}) ", QUOTE + indexName + QUOTE, pName)
                        End Select

                        If (String.IsNullOrEmpty(where)) Then
                            where += str
                        Else
                            where += " AND " + str
                        End If

                        ' Костыль для поиска в БД данных по соответствующему типу
                        Dim param As NpgsqlParameter
                        Select Case valueType
                            Case NpgsqlDbType.Integer
                                Dim intValue = Integer.Parse(valueAsString)
                                param = New NpgsqlParameter(pName, valueType) With {
                                    .Value = intValue
                                    }
                            Case NpgsqlDbType.Bigint
                                Dim longValue = Long.Parse(valueAsString)
                                param = New NpgsqlParameter(pName, valueType) With {
                                    .Value = longValue
                                    }

                            Case NpgsqlDbType.Boolean
                                Dim boolValue = Boolean.Parse(valueAsString)
                                param = New NpgsqlParameter(pName, valueType) With {
                                    .Value = boolValue
                                    }
                            Case Else
                                param = New NpgsqlParameter(pName, valueType) With {
                                    .Value = valueAsString
                                    }
                        End Select

                        parameters.Add(param)
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
        Dim rtype = IIf(type.AssemblyQualifiedName = SupportedType.AssemblyQualifiedName, "-", type.AssemblyQualifiedName)
        Dim parameters = {New NpgsqlParameter("@p1", id), New NpgsqlParameter("@p2", json), New NpgsqlParameter("@p3", rtype)}
        Dim sql = String.Format("INSERT INTO ""{0}""(GUID ,JSON, TYPE) VALUES(@p1, @p2, @p3)", Name)
        PgUtils.ExecSql(ConnectionString, sql, parameters)
    End Sub

    Private Sub Update(connStr As String, id As String, json As String)
        Dim sql = String.Format("UPDATE ""{0}"" SET JSON = @p1 WHERE guid = @p2", Name)
        PgUtils.ExecSql(ConnectionString, sql, {New NpgsqlParameter("@p1", json), New NpgsqlParameter("@p2", id)})
    End Sub

    Public Overrides Sub RemoveAllObjects()
        CheckDb()
        Dim sql = String.Format("DELETE FROM ""{0}""", Name)
        PgUtils.ExecSql(ConnectionString, sql)
    End Sub

End Class