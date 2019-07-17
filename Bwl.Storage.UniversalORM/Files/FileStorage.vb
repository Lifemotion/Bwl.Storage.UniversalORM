Imports System.Reflection
Imports System.IO

Public Class FileObjStorage
    Inherits CommonObjStorage

    Private _folder As String
    Private _useUndexing As Boolean = False

    Friend Sub New(folder As String, type As Type)
        MyBase.New(type)
        _folder = folder
    End Sub

    'Private Function GetIndexPath(index As String)
    '	Dim path = _folder + Utils.Sep + "index_" + index
    '	If Not IO.Directory.Exists(path) Then IO.Directory.CreateDirectory(path)
    '	Return path
    'End Function

    'Private Function GetIndexPath(index As String, hash As String) As String
    '	Dim path = _folder + Utils.Sep + "index_" + index + Utils.Sep + hash
    '	If Not IO.Directory.Exists(path) Then IO.Directory.CreateDirectory(path)
    '	Return path
    'End Function

    Public Property UseIndexing As Boolean
        Get
            Return _useUndexing
        End Get
        Set(value As Boolean)
            _useUndexing = value
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
        Return Nothing
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

            'For Each indexing In _indexingMembers
            '	Try
            '		Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj).ToString
            '		Dim path = GetIndexPath(indexing.Name, MD5.GetHash(obj.ID))
            '		IO.File.WriteAllText(path + Utils.Sep + obj.ID + ".hash", indexValue.ToString)
            '	Catch ex As Exception
            '		'...
            '	End Try
            'Next
        End If
    End Sub

    Public Overrides Sub UpdateObj(obj As ObjBase)
        If Utils.TestFolderFsm(_folder) AndAlso obj IsNot Nothing Then
            Dim file = GetFileName(obj.ID)
            If Not IO.File.Exists(file) Then Throw New Exception("Object Not Exists with this ID")

            IO.File.Delete(file)
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

            'For Each indexing In _indexingMembers
            '	Try
            '		Dim path = GetIndexPath(indexing.Name, MD5.GetHash(id))
            '		Dim fNameIndex = path + Utils.Sep + id + ".hash"

            '		If (File.Exists(fNameIndex)) Then
            '			File.Delete(fNameIndex)
            '		End If

            '		Try
            '			If (Not Directory.GetFiles(path).Any) And (Not Directory.GetDirectories(path).Any) Then
            '				Directory.Delete(path)
            '			End If
            '		Catch ex As Exception
            '			'...
            '		End Try
            '	Catch ex As Exception
            '		'...
            '	End Try
            'Next
        End If
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
                    Dim fileReader = My.Computer.FileSystem.OpenTextFileReader(path)
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

                Dim fileReader = My.Computer.FileSystem.OpenTextFileReader(path)
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
                Dim fileReader = My.Computer.FileSystem.OpenTextFileReader(indexFileName)
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
                                        value = CTypeDynamic(valueStr, indexInfo.Type)
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
        Dim result As New List(Of String)
        If Not UseIndexing Then
            'If searchParams IsNot Nothing Then
            '	Throw New Exception("FileStorage: searchParams not null при отключенном индексировании")
            'Else
            result.AddRange(FindAllObjs())
            Return result.ToArray
            'End If
        End If

        If searchParams Is Nothing OrElse searchParams.FindCriterias Is Nothing Then
            result.AddRange(FindAllObjs())
        Else
            Dim listResults As New List(Of List(Of String))()
            For Each crit In searchParams.FindCriterias
                Dim tmpResult As New List(Of String)()
                Dim indexFileName = String.Empty
                If searchParams.FindCriterias IsNot Nothing Then
                    Dim indexInfo = _indexingMembers.Find(Function(x) x.Name = crit.Field)
                    If indexInfo IsNot Nothing Then
                        indexFileName = GetIndexFileName(Type.GetType(SupportedType.AssemblyQualifiedName), indexInfo.Name)
                        Dim fileReader = My.Computer.FileSystem.OpenTextFileReader(indexFileName)
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
                                            value = CTypeDynamic(valueStr, indexInfo.Type)
                                        End If

                                        Select Case (crit.Condition)
                                            Case FindCondition.equal
                                                If (value = crit.Value) Then
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
                                                Throw New NotSupportedException
                                            Case FindCondition.notLikeEqual
                                                Throw New NotSupportedException
                                            Case FindCondition.multipleEqual
                                                Throw New NotSupportedException
                                            Case FindCondition.multipleNotEqual
                                                Throw New NotSupportedException
                                            Case FindCondition.notEqual
                                                If (value <> crit.Value) Then
                                                    tmpResult.Add(idStr)
                                                End If
                                        End Select
                                    Catch ex As Exception
                                    End Try
                                End If
                            End While
                        Finally
                            fileReader.Dispose()
                        End Try
                        listResults.Add(tmpResult)
                    Else
                        Throw New Exception(String.Format("Указанный тип ({0}) не является индексируемым", searchParams.SortParam.Field))
                    End If
                End If
            Next
            If listResults.Count > 0 Then
                'сравнение результатов поиска по каждому критерию
                For Each id In listResults(0)
                    Dim exists = True
                    For i = 1 To listResults.Count - 1
                        If Not listResults(i).Contains(id) Then
                            exists = False
                            Exit For
                        End If
                    Next
                    If (exists) AndAlso Not result.Contains(id) Then
                        result.Add(id)
                    End If
                Next
            End If
        End If

        If searchParams IsNot Nothing AndAlso searchParams.SortParam IsNot Nothing Then
            If result.Count > 0 Then
                result = Sort(result, searchParams.SortParam)
            End If
        End If

        If searchParams IsNot Nothing AndAlso searchParams.SelectOptions IsNot Nothing Then
            If searchParams.SelectOptions.SelectMode = SelectMode.Top Then
                'Top
                If result.Count > 0 Then
                    Dim topVal = Convert.ToInt32(searchParams.SelectOptions.TopValue)
                    result = result.GetRange(0, topVal)
                End If
            Else
                'Between
                If result.Count > 0 Then
                    Dim startVal = Convert.ToInt32(searchParams.SelectOptions.StartValue)
                    Dim endVal = Convert.ToInt32(searchParams.SelectOptions.EndValue)
                    result = result.GetRange(startVal, endVal - startVal + 1)
                End If
            End If
        End If

        Return result.ToArray()
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

    Public Overrides Function GetObjects(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase)
        Dim res = New List(Of ObjBase)
        For Each id In objIds
            res.Add(GetObj(id))
        Next
        Return res
    End Function

    Public Overrides Function GetObjects(Of T As ObjBase)(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of T)
        Dim res = New List(Of T)
        For Each id In objIds
            res.Add(GetObj(Of T)(id))
        Next
        Return res
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
End Class
