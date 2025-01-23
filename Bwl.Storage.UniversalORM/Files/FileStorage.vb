Imports System.Reflection
Imports System.IO
Imports System.Collections.Concurrent
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

Public Class FileObjStorage
    Inherits CommonObjStorage

    Private _folder As String
    Private _useIndexing As Boolean = False

    Friend Sub New(folder As String, type As Type)
        MyBase.New(type)
        _folder = folder
    End Sub

    Public Property UseIndexing As Boolean
        Get
            Return _useIndexing
        End Get
        Set(value As Boolean)
            _useIndexing = value
        End Set
    End Property

    Public Property StorageDir As String
        Get
            Return _folder
        End Get
        Set(value As String)
            _folder = value
        End Set
    End Property

    Public Overrides Function GetSomeFieldDistinct(fieldName As String) As IEnumerable(Of String)
        Dim distinctValues = New ConcurrentBag(Of String)
        If _useIndexing Then
            If _indexingMembers.Any(Function(f) f.Name = fieldName) Then
                Dim path = GetIndexFileName(SupportedType, fieldName)
                Dim values = File.ReadAllLines(path).Select(Function(f) f.Split({"}"c}, 2)(1).Trim()).Distinct()
                Parallel.ForEach(values,
                             Sub(value)
                                 If Not distinctValues.Contains(value) Then
                                     distinctValues.Add(value)
                                 End If
                             End Sub)
            End If
        Else
            Dim ids = FindAllObjs()
            Parallel.ForEach(ids,
                             Sub(id)
                                 Dim obj = GetObj(id)
                                 If obj IsNot Nothing Then
                                     Dim value = ReflectionTools.GetMemberValue(fieldName, obj).ToString
                                     If Not distinctValues.Contains(value) Then
                                         distinctValues.Add(value)
                                     End If
                                 End If
                             End Sub)
        End If
        Return distinctValues.Distinct().ToList()
    End Function

    Public Overrides Sub AddObj(obj As ObjBase)
        If Utils.TestFolderFsm(_folder) AndAlso obj IsNot Nothing Then
            Dim file = GetFileName(obj.ID)
            If IO.File.Exists(file) Then Throw New Exception("Object Already Exists with this ID")
            Dim oi = New ObjInfo
            oi.Obj = CfJsonConverter.Serialize(obj)

            Dim rtype = obj.GetType
            If obj.GetType.AssemblyQualifiedName = SupportedType.AssemblyQualifiedName Then
                rtype = Nothing
            End If
            oi.ObjType = rtype
            Dim str = CfJsonConverter.Serialize(oi)
            IO.File.WriteAllText(file, str, Utils.Enc)

            CreateIndex(obj)
        End If
    End Sub

    Public Overrides Sub UpdateObj(obj As ObjBase)
        If Utils.TestFolderFsm(_folder) AndAlso obj IsNot Nothing Then
            Dim file = GetFileName(obj.ID)
            If Not IO.File.Exists(file) Then Throw New Exception("Object Not Exists with this ID")
            IO.File.Delete(file)
            DeleteIndex(obj)
            AddObj(obj)
        End If
    End Sub

    Public Overrides Sub RemoveObj(id As String)
        If Utils.TestFolderFsm(_folder) Then
            Dim fileMain = GetFileName(id)
            If Not IO.File.Exists(fileMain) Then Throw New Exception("Object Not Exists with this ID")

            Try
                Dim oldobj = GetObj(id)
                DeleteIndex(oldobj)
            Catch ex As Exception

            End Try

            IO.File.Delete(fileMain)
        End If
    End Sub

    Public Overrides Sub RemoveObjs(ids As String())
        For Each id As String In ids
            RemoveObj(id)
        Next
    End Sub

    Private Sub CreateIndex(obj As ObjBase)
        If UseIndexing Then
            Dim res = String.Empty
            For Each indexing In _indexingMembers
                Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj)
                If indexing.Type = GetType(DateTime) Then
                    indexValue = CType(indexValue, DateTime).Ticks
                End If

                Dim path = GetIndexFileName(obj.GetType, indexing.Name)
                Dim indValStr = String.Empty
                If indexValue IsNot Nothing Then
                    indValStr = indexValue.ToString
                End If

                Dim value = obj.ID + " " + indValStr + vbCrLf

                Dim existsID As Boolean = False
                If File.Exists(path) Then
                    Dim fileReader As New StreamReader(path)
                    Dim lineVal As String()
                    While fileReader.Peek <> -1
                        lineVal = fileReader.ReadLine().Split(" "c)
                        If lineVal(0) = obj.ID Then existsID = True
                    End While
                    fileReader.Close()
                End If
                If Not existsID Then
                    IO.File.AppendAllText(path, value)
                End If
            Next
        End If
    End Sub

    Private Sub DeleteIndex(obj As ObjBase)
        If UseIndexing Then
            Dim lst As New List(Of ObjBase)()
            For Each Indexing In _indexingMembers
                Dim path = GetIndexFileName(obj.GetType, Indexing.Name)

                Dim fileReader As New StreamReader(path)
                Dim stringReader = String.Empty
                While fileReader.Peek <> -1
                    stringReader = fileReader.ReadLine()
                    If stringReader <> String.Empty Then
                        Dim line = stringReader.Split(" "c)
                        If line(0) <> obj.ID Then
                            Dim ob = GetObj(line(0))
                            If ob IsNot Nothing Then
                                lst.Add(ob)
                            End If
                        End If
                    End If
                End While
                fileReader.Close()
                Threading.Thread.Sleep(100)
                If File.Exists(path) Then
                    File.Delete(path)
                    Threading.Thread.Sleep(100)
                    If lst IsNot Nothing Then
                        For Each obj In lst
                            CreateIndex(obj)
                        Next
                    End If
                End If
            Next
        End If
    End Sub

    Private Function GetFileName(objId As String) As String
        Return _folder + Utils.Sep + objId + ".obj.json"
    End Function

    Private Function GetIndexFileName(type As Type, index As String) As String
        Return _folder + Utils.Sep + type.Name + "." + index + ".index"
    End Function

    Private Shared Function SortDictionary(dictionary As Dictionary(Of String, Object), sortParam As SortParam) As IEnumerable(Of String)
        Dim result As New List(Of String)
        If dictionary IsNot Nothing AndAlso dictionary.Any Then
            If (sortParam.SortMode = SortMode.Ascending) Then
                result.AddRange(dictionary.OrderBy(Function(pair) pair.Value).Select(Function(pair) pair.Key))
            Else
                result.AddRange(dictionary.OrderByDescending(Function(pair) pair.Value).Select(Function(pair) pair.Key))
            End If
        End If
        Return result
    End Function

    Private Function Sort(list As List(Of String), sortParam As SortParam) As List(Of String)
        Dim result As New List(Of String)
        If list IsNot Nothing AndAlso list.Any Then
            Dim dictionary As New Dictionary(Of String, Object)
            Dim indexInfo = _indexingMembers.Find(Function(x) x.Name = sortParam.Field)
            If indexInfo IsNot Nothing Then
                Dim indexFileName = GetIndexFileName(Type.GetType(SupportedType.AssemblyQualifiedName), indexInfo.Name)
                Dim fileReader As New StreamReader(indexFileName)
                Try
                    Dim stringReader = String.Empty
                    While fileReader.Peek <> -1
                        stringReader = fileReader.ReadLine()
                        If stringReader <> String.Empty Then
                            Dim line = stringReader
                            Dim startSpacePos = line.IndexOf(" "c)
                            Dim idStr = line.Substring(0, startSpacePos)
                            If list.Contains(idStr) Then
                                Dim valueStr = ""
                                If startSpacePos > 0 Then
                                    valueStr = line.Substring(startSpacePos + 1, line.Length - startSpacePos - 1)
                                End If
                                Try
                                    Dim value As Object = Nothing
                                    If indexInfo.Type = GetType(DateTime) Then
                                        value = New DateTime(Convert.ToInt64(valueStr))
                                    Else
                                        value = Convert.ChangeType(valueStr, indexInfo.Type)
                                    End If
                                    dictionary.Add(idStr, value)
                                Catch ex As Exception
                                End Try
                            End If
                        End If
                    End While
                Finally
                    fileReader.Dispose()
                End Try
                result.AddRange(SortDictionary(dictionary, sortParam))
            End If
        End If
        Return result
    End Function

    Public Overrides Function FindObj(searchParams As SearchParams) As String()
        If (_useIndexing) Then
            Return FindObjIndex(searchParams, False)
        Else
            Return FindObj(searchParams, False)
        End If
    End Function

    Private Function FindObjIndex(searchParams As SearchParams, isNegative As Boolean) As String()
        Dim result As New List(Of String)

        If searchParams Is Nothing OrElse searchParams.FindCriterias Is Nothing Then
            result.AddRange(FindAllObjs())
        Else
            Dim listResults As New List(Of List(Of String))()
            For Each crit In searchParams.FindCriterias
                Dim tmpResult As New List(Of String)()
                Dim indexFileName = String.Empty

                If ({FindCondition.findCriteria, FindCondition.findCriteriaNegative}.Any(Function(f) crit.Condition = f)) Then
                    Dim fc = CfJsonConverter.Deserialize(Of List(Of FindCriteria))(crit.Value)
                    Dim sp = New SearchParams(fc)
                    tmpResult.AddRange(FindObjIndex(sp, crit.Condition = FindCondition.findCriteriaNegative))
                    listResults.Add(tmpResult)
                    Continue For
                End If

                If searchParams.FindCriterias IsNot Nothing Then
                    Dim indexInfo = _indexingMembers.Find(Function(x) x.Name = crit.Field)
                    If indexInfo IsNot Nothing Then
                        indexFileName = GetIndexFileName(Type.GetType(SupportedType.AssemblyQualifiedName), indexInfo.Name)
                        Dim fileReader As New StreamReader(indexFileName)
                        Try
                            Dim stringReader = String.Empty
                            While fileReader.Peek <> -1
                                stringReader = fileReader.ReadLine()
                                If stringReader <> String.Empty Then
                                    Dim line = stringReader
                                    Dim startSpacePos = line.IndexOf(" "c)
                                    Dim idStr = line.Substring(0, startSpacePos)
                                    Dim valueStr = ""
                                    If startSpacePos > 0 Then
                                        valueStr = line.Substring(startSpacePos + 1, line.Length - startSpacePos - 1)
                                    End If

                                    Try
                                        Dim value As Object = Nothing
                                        If indexInfo.Type = GetType(DateTime) Then
                                            value = New DateTime(Convert.ToInt64(valueStr))
                                        Else
                                            value = Convert.ChangeType(valueStr, indexInfo.Type)
                                        End If

                                        Select Case (crit.Condition)
                                            Case FindCondition.equal
                                                If (value = crit.Value) Then
                                                    tmpResult.Add(idStr)
                                                End If
                                            Case FindCondition.notEqual
                                                If (value <> crit.Value) Then
                                                    tmpResult.Add(idStr)
                                                End If
                                            Case FindCondition.greater
                                                If (value > crit.Value) Then
                                                    tmpResult.Add(idStr)
                                                End If
                                            Case FindCondition.greaterOrEqual
                                                If (value >= crit.Value) Then
                                                    tmpResult.Add(idStr)
                                                End If
                                            Case FindCondition.less
                                                If (value < crit.Value) Then
                                                    tmpResult.Add(idStr)
                                                End If
                                            Case FindCondition.lessOrEqual
                                                If (value <= crit.Value) Then
                                                    tmpResult.Add(idStr)
                                                End If
                                            Case FindCondition.likeEqual
                                                If (StringLike(CStr(value), CStr(crit.Value))) Then
                                                    tmpResult.Add(idStr)
                                                End If
                                            Case FindCondition.notLikeEqual
                                                If Not StringLike(CStr(value), CStr(crit.Value)) Then
                                                    tmpResult.Add(idStr)
                                                End If
                                            Case FindCondition.multipleEqual
                                                Dim valuesArray As String() = CfJsonConverter.Deserialize(Of String())(crit.Value)
                                                For Each v In valuesArray
                                                    Dim convertedV As Object
                                                    If indexInfo.Type = GetType(DateTime) Then
                                                        convertedV = New DateTime(Convert.ToInt64(v))
                                                    Else
                                                        convertedV = Convert.ChangeType(v, indexInfo.Type)
                                                    End If
                                                    If value.Equals(convertedV) Then
                                                        tmpResult.Add(idStr)
                                                        Exit For
                                                    End If
                                                Next
                                            Case FindCondition.multipleNotEqual
                                                Dim valuesArray As String() = CfJsonConverter.Deserialize(Of String())(crit.Value)
                                                Dim matchFound As Boolean = False
                                                For Each v In valuesArray
                                                    Dim convertedV As Object
                                                    If indexInfo.Type = GetType(DateTime) Then
                                                        convertedV = New DateTime(Convert.ToInt64(v))
                                                    Else
                                                        convertedV = Convert.ChangeType(v, indexInfo.Type)
                                                    End If
                                                    If value.Equals(convertedV) Then
                                                        matchFound = True
                                                        Exit For
                                                    End If
                                                Next
                                                If Not matchFound Then
                                                    tmpResult.Add(idStr)
                                                End If
                                            Case FindCondition.multipleGreater
                                                Dim valuesArray As String() = CfJsonConverter.Deserialize(Of String())(crit.Value)
                                                For Each v In valuesArray
                                                    Dim convertedV As Object
                                                    If indexInfo.Type = GetType(DateTime) Then
                                                        convertedV = New DateTime(Convert.ToInt64(v))
                                                    Else
                                                        convertedV = Convert.ChangeType(v, indexInfo.Type)
                                                    End If
                                                    If value > convertedV Then
                                                        tmpResult.Add(idStr)
                                                        Exit For
                                                    End If
                                                Next
                                            Case FindCondition.multipleGreaterOrEqual
                                                Dim valuesArray As String() = CfJsonConverter.Deserialize(Of String())(crit.Value)
                                                For Each v In valuesArray
                                                    Dim convertedV As Object
                                                    If indexInfo.Type = GetType(DateTime) Then
                                                        convertedV = New DateTime(Convert.ToInt64(v))
                                                    Else
                                                        convertedV = Convert.ChangeType(v, indexInfo.Type)
                                                    End If
                                                    If value >= convertedV Then
                                                        tmpResult.Add(idStr)
                                                        Exit For
                                                    End If
                                                Next
                                            Case FindCondition.multipleLess
                                                Dim valuesArray As String() = CfJsonConverter.Deserialize(Of String())(crit.Value)
                                                For Each v In valuesArray
                                                    Dim convertedV As Object
                                                    If indexInfo.Type = GetType(DateTime) Then
                                                        convertedV = New DateTime(Convert.ToInt64(v))
                                                    Else
                                                        convertedV = Convert.ChangeType(v, indexInfo.Type)
                                                    End If
                                                    If value < convertedV Then
                                                        tmpResult.Add(idStr)
                                                        Exit For
                                                    End If
                                                Next
                                            Case FindCondition.multipleLessOrEqual
                                                Dim valuesArray As String() = CfJsonConverter.Deserialize(Of String())(crit.Value)
                                                For Each v In valuesArray
                                                    Dim convertedV As Object
                                                    If indexInfo.Type = GetType(DateTime) Then
                                                        convertedV = New DateTime(Convert.ToInt64(v))
                                                    Else
                                                        convertedV = Convert.ChangeType(v, indexInfo.Type)
                                                    End If
                                                    If value <= convertedV Then
                                                        tmpResult.Add(idStr)
                                                        Exit For
                                                    End If
                                                Next
                                            Case FindCondition.multipleLikeEqual
                                                Dim valuesArray As String() = CfJsonConverter.Deserialize(Of String())(crit.Value)
                                                For Each v In valuesArray
                                                    If StringLike(CStr(value), v) Then
                                                        tmpResult.Add(idStr)
                                                        Exit For
                                                    End If
                                                Next
                                            Case FindCondition.multipleNotLikeEqual
                                                Dim valuesArray As String() = CfJsonConverter.Deserialize(Of String())(crit.Value)
                                                Dim matchFound As Boolean = False
                                                For Each v In valuesArray
                                                    If StringLike(CStr(value), v) Then
                                                        matchFound = True
                                                        Exit For
                                                    End If
                                                Next
                                                If Not matchFound Then
                                                    tmpResult.Add(idStr)
                                                End If
                                        End Select
                                    Catch ex As Exception
                                        ' Handle conversion exceptions if necessary
                                    End Try
                                End If
                            End While
                        Finally
                            fileReader.Dispose()
                        End Try
                        listResults.Add(tmpResult)
                    Else
                        Throw New Exception(String.Format("The specified field ({0}) is not indexed", crit.Field))
                    End If
                End If
            Next
            If listResults.Count > 0 Then
                ' Intersect the results from all criteria
                result = listResults(0)
                For i = 1 To listResults.Count - 1
                    result = result.Intersect(listResults(i)).ToList()
                Next
            End If
            ' Handle negative condition
            If isNegative Then
                Dim tmp = result.ToArray()
                result.Clear()
                result.AddRange(FindAllObjs().Except(tmp))
            End If
        End If

        ' Apply sorting if specified
        If searchParams IsNot Nothing AndAlso searchParams.SortParam IsNot Nothing Then
            If result.Count > 0 Then
                result = Sort(result, searchParams.SortParam)
            End If
        End If

        ' Apply select options (Top or Between)
        If searchParams IsNot Nothing AndAlso searchParams.SelectOptions IsNot Nothing Then
            If searchParams.SelectOptions.SelectMode = SelectMode.Top Then
                ' Top
                If result.Count > 0 Then
                    Dim topVal = Convert.ToInt32(searchParams.SelectOptions.TopValue)
                    result = result.Take(topVal).ToList()
                End If
            Else
                ' Between
                If result.Count > 0 Then
                    Dim startVal = Convert.ToInt32(searchParams.SelectOptions.StartValue)
                    Dim endVal = Convert.ToInt32(searchParams.SelectOptions.EndValue)
                    result = result.Skip(startVal).Take(endVal - startVal + 1).ToList()
                End If
            End If
        End If

        Return result.ToArray()
    End Function

    Public Overloads Function FindObj(searchParams As SearchParams, isNegative As Boolean) As String()
        Dim result As New ConcurrentBag(Of String)()
        Dim ids = FindAllObjs().ToArray()

        If searchParams Is Nothing OrElse searchParams.FindCriterias Is Nothing OrElse Not searchParams.FindCriterias.Any() Then
            ' No criteria specified, return all object IDs
            For Each id In ids
                result.Add(id)
            Next
        Else
            ' Process the criteria
            Parallel.ForEach(ids, Sub(id)
                                      Dim obj = GetObj(id)
                                      If obj IsNot Nothing Then
                                          Dim matches As Boolean = CheckObjectAgainstCriteria(obj, searchParams.FindCriterias.ToList())
                                          If matches Then
                                              result.Add(id)
                                          End If
                                      End If
                                  End Sub)
        End If

        If isNegative Then
            ' Return IDs that are not in result
            Dim allIds = New HashSet(Of String)(ids)
            Dim matchedIds = result.ToArray()
            result = New ConcurrentBag(Of String)(allIds.Except(matchedIds))
        End If

        ' Convert result bag to list for sorting and selection
        Dim resultList As List(Of String) = result.ToList()

        ' Apply sorting if specified
        If searchParams IsNot Nothing AndAlso searchParams.SortParam IsNot Nothing Then
            If resultList.Count > 0 Then
                resultList = SortObjects(resultList, searchParams.SortParam)
            End If
        End If

        ' Apply select options (Top or Between)
        If searchParams IsNot Nothing AndAlso searchParams.SelectOptions IsNot Nothing Then
            If searchParams.SelectOptions.SelectMode = SelectMode.Top Then
                ' Top N items
                Dim topVal = Convert.ToInt32(searchParams.SelectOptions.TopValue)
                resultList = resultList.Take(topVal).ToList()
            Else
                ' Between
                Dim startVal = Convert.ToInt32(searchParams.SelectOptions.StartValue)
                Dim endVal = Convert.ToInt32(searchParams.SelectOptions.EndValue)
                resultList = resultList.Skip(startVal).Take(endVal - startVal + 1).ToList()
            End If
        End If

        Return resultList.ToArray()
    End Function

    Private Function CheckObjectAgainstCriteria(obj As ObjBase, criterias As List(Of FindCriteria)) As Boolean
        ' Check if the object matches all the criteria
        For Each crit In criterias
            Dim matches As Boolean

            If crit.Condition = FindCondition.findCriteria OrElse crit.Condition = FindCondition.findCriteriaNegative Then
                Dim subCriterias = CfJsonConverter.Deserialize(Of List(Of FindCriteria))(crit.Value)
                matches = CheckObjectAgainstCriteria(obj, subCriterias)
                If crit.Condition = FindCondition.findCriteriaNegative Then
                    matches = Not matches
                End If
            Else
                ' Evaluate the criterion against the object field
                matches = EvaluateCriterion(obj, crit)
            End If

            If Not matches Then
                ' If object doesn't match any of the criteria, return False
                Return False
            End If
        Next
        Return True
    End Function

    Private Function EvaluateCriterion(obj As ObjBase, crit As FindCriteria) As Boolean
        Dim fieldName = crit.Field
        Dim fieldValue As Object = ReflectionTools.GetMemberValue(fieldName, obj)
        Dim fieldType = crit.Value.GetType()

        Select Case crit.Condition
            Case FindCondition.equal
                Return Object.Equals(fieldValue, crit.Value)
            Case FindCondition.notEqual
                Return Not Object.Equals(fieldValue, crit.Value)
            Case FindCondition.greater
                Return Comparer.Default.Compare(fieldValue, crit.Value) > 0
            Case FindCondition.greaterOrEqual
                Return Comparer.Default.Compare(fieldValue, crit.Value) >= 0
            Case FindCondition.less
                Return Comparer.Default.Compare(fieldValue, crit.Value) < 0
            Case FindCondition.lessOrEqual
                Return Comparer.Default.Compare(fieldValue, crit.Value) <= 0
            Case FindCondition.likeEqual
                Return StringLike(fieldValue.ToString(), crit.Value.ToString())
            Case FindCondition.notLikeEqual
                Return Not StringLike(fieldValue.ToString(), crit.Value.ToString())
            Case FindCondition.multipleEqual
                Dim valuesArray As Object() = CfJsonConverter.Deserialize(Of Object())(crit.Value)
                For Each v In valuesArray
                    Dim convertedV = ConvertCriterionValue(v, fieldType)
                    If Object.Equals(fieldValue, convertedV) Then
                        Return True
                    End If
                Next
                Return False
            Case FindCondition.multipleNotEqual
                Dim valuesArray As Object() = CfJsonConverter.Deserialize(Of Object())(crit.Value)
                For Each v In valuesArray
                    Dim convertedV = ConvertCriterionValue(v, fieldType)
                    If Object.Equals(fieldValue, convertedV) Then
                        Return False
                    End If
                Next
                Return True
                ' Handle other multiple conditions similarly
            Case Else
                Return False
        End Select
    End Function

    Private Function ConvertCriterionValue(value As Object, targetType As Type) As Object
        If targetType Is GetType(DateTime) Then
            Return DateTime.Parse(value.ToString())
        Else
            Return Convert.ChangeType(value, targetType)
        End If
    End Function

    Private Function SortObjects(ids As List(Of String), sortParam As SortParam) As List(Of String)
        ' Load the objects along with their field values for sorting
        Dim objectsWithValues As New List(Of KeyValuePair(Of String, Object))()

        For Each id In ids
            Dim obj = GetObj(id)
            If obj IsNot Nothing Then
                Dim fieldValue = ReflectionTools.GetMemberValue(sortParam.Field, obj)
                objectsWithValues.Add(New KeyValuePair(Of String, Object)(id, fieldValue))
            End If
        Next

        ' Sort the objects based on the field value
        If sortParam.SortMode = SortMode.Ascending Then
            Return objectsWithValues.OrderBy(Function(kv) kv.Value).Select(Function(kv) kv.Key).ToList()
        Else
            Return objectsWithValues.OrderByDescending(Function(kv) kv.Value).Select(Function(kv) kv.Key).ToList()
        End If
    End Function

    Private Function StringLike(s1 As String, s2 As String) As Boolean
        ' Convert the SQL LIKE pattern to a regular expression pattern
        Dim regexPattern As String = "^" + Regex.Escape(s2).Replace("%", ".*").Replace("_", ".") + "$"
        Return Regex.IsMatch(s1, regexPattern, RegexOptions.IgnoreCase)
    End Function

    Public Overrides Function FindObjCount(searchParams As SearchParams) As Long
        Dim res As Long = 0
        Dim tmp = FindObj(searchParams)
        If tmp IsNot Nothing AndAlso tmp.Any Then
            res = tmp.Length
        End If
        Return res
    End Function

    Private Function FindAllObjs() As IEnumerable(Of String)
        Dim result As New List(Of String)
        If Utils.TestFolderFsm(_folder) Then
            Dim files = IO.Directory.GetFiles(_folder, "*.obj.json")
            For Each file In files
                Dim fileParts = file.Split(Utils.Sep, "."c)
                result.Add(fileParts(fileParts.Length - 3))
            Next
        End If
        Return result
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
        Dim obj As ObjBase = Nothing
        If Utils.TestFolderFsm(_folder) Then
            Dim file = GetFileName(id)
            Try
                If IO.File.Exists(file) Then
                    Dim str = IO.File.ReadAllText(file, Utils.Enc)
                    If String.IsNullOrWhiteSpace(str) Then
                        IO.File.Delete(file)
                    Else
                        Dim oi = CType(CfJsonConverter.Deserialize(str, GetType(ObjInfo)), ObjInfo)
                        If oi.ObjType = Nothing Then
                            oi.ObjType = SupportedType
                        End If
                        obj = CType(CfJsonConverter.Deserialize(oi.Obj, oi.ObjType), ObjBase)
                    End If
                End If
            Catch ex As Exception
                Dim err = "Err FileStorage.GetObj _ file: " + file + vbCrLf + ex.ToString
                Dim ex1 = New InvalidOperationException(err, ex)
                'Throw ex1
                obj = Nothing
            End Try
        End If
        Return obj
    End Function

    Public Overrides Function GetObj(Of T As ObjBase)(id As String) As T
        Return CType(GetObj(id), T)
    End Function

    Public Overrides Function GetObjects(Of T As ObjBase)(searchParams As SearchParams) As IEnumerable(Of T)
        Return GetObjects(searchParams).Select(Function(f) CType(f, T)).ToArray()
    End Function

    Public Overrides Function GetObjects(searchParams As SearchParams) As IEnumerable(Of ObjBase)
        Return FindObj(searchParams).Select(Function(f) GetObj(f)).ToArray()
    End Function

    Public Overrides Function GetObjects(Of T As ObjBase)(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of T)
        Return GetObjects(objIds, sortParam).Select(Function(f) CType(f, T)).ToArray()
    End Function

    Public Overrides Function GetObjects(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase)
        Return objIds.Select(Function(f) GetObj(f)).ToArray()
    End Function

    Public Overrides Function Contains(id As String) As Boolean
        Dim file = GetFileName(id)
        Return IO.File.Exists(file)
    End Function

    Public Overrides Sub AddObjects(objects() As ObjBase)
        For Each obj In objects
            AddObj(obj)
        Next
    End Sub

    Public Overrides Sub RemoveAllObjects()
        Try
            If Directory.Exists(_folder) Then
                Directory.Delete(_folder, True)
            End If
        Catch ex As Exception
        End Try
        Try
            Utils.TestFolderFsm(_folder)
        Catch ex As Exception
        End Try
    End Sub

    ' Следующие поля - только для SQL, в FileStorage они бесполезны
    <Obsolete("DO NOT use this method unless absolutely necessary", False)>
    Public Overrides Function ExecSqlGetObjects(sqlString As String) As List(Of List(Of Object))
        Throw New NotImplementedException()
    End Function

    <Obsolete("DO NOT use this method unless absolutely necessary", False)>
    Public Overrides Sub ExecSql(sqlString As String)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Function GetNullDataIds() As String()
        Throw New NotImplementedException()
    End Function

    Public Overrides Sub CleanNullData()
        Throw New NotImplementedException()
    End Sub
End Class
