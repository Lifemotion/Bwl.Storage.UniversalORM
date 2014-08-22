﻿Imports System.Reflection
Imports System.IO

Public Class FileObjStorage
	Inherits CommonObjStorage

	Private _folder As String

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

	Public Property StorageDir As String
		Get
			Return _folder
		End Get
		Set(value As String)
			_folder = value
		End Set
	End Property

	Public Overrides Sub AddObj(obj As ObjBase)
		If Utils.TestFolderFsm(_folder) AndAlso obj IsNot Nothing Then
			Dim file = GetFileName(obj.ID)
			If IO.File.Exists(file) Then Throw New Exception("Object Already Exists with this ID")
			Dim oi = New ObjInfo
			oi.Obj = CfJsonConverter.Serialize(obj)
			oi.ObjType = obj.GetType
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
			Dim oldobj = GetObj(id)
			IO.File.Delete(fileMain)
			DeleteIndex(oldobj)
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

	Private Function CreateIndex(obj As ObjBase) As Boolean
		For Each indexing In _indexingMembers
			Try
				Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj).ToString
				Dim path = GetIndexFileName(obj.GetType, indexing.Name)
				Dim value = obj.ID + " " + indexValue + vbCrLf

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
			Catch
				Return False
			End Try
		Next
		Return True
	End Function

	Private Function DeleteIndex(obj As ObjBase) As Boolean
		Dim lst As New List(Of ObjBase)()
		For Each Indexing In _indexingMembers
			Try
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
			Catch exc As Exception
				MessageBox.Show(exc.Message)
			End Try
		Next

		Return False
	End Function

	Private Function GetFileName(objId As String) As String
		Return _folder + Utils.Sep + objId + ".obj.json"
	End Function

	Private Function GetIndexFileName(type As Type, index As String) As String
		Return _folder + Utils.Sep + type.Name + "." + index + ".index"
	End Function

	Private Function SortDictionary(dictionary As Dictionary(Of String, String), sortParam As SortParam) As List(Of String)
		Dim result As New List(Of String)
		Dim sortedDictionary As IEnumerable(Of KeyValuePair(Of String, String))
		If (sortParam.SortMode = SortMode.Ascending) Then
			sortedDictionary = dictionary.OrderBy(Function(v) v.Value)
		Else
			Dim sorted = From pair In dictionary Order By pair.Value Descending
			sortedDictionary = dictionary.OrderByDescending(Function(v) v.Value)
		End If
		For Each item In sortedDictionary
			result.Add(item.Key)
		Next
		Return result
	End Function

	Private Function Sort(sortParam As SortParam) As List(Of String)
		Dim result As New List(Of String)
		Dim dictionary As New Dictionary(Of String, String)
		Dim indexInfo = _indexingMembers.Find(Function(x) x.Name = sortParam.Field)
		If indexInfo IsNot Nothing Then
			Dim indexFileName = GetIndexFileName(Type.GetType(SupportedType.AssemblyQualifiedName), indexInfo.Name)
			Dim fileReader = My.Computer.FileSystem.OpenTextFileReader(indexFileName)
			Dim stringReader = String.Empty
			While fileReader.Peek <> -1
				stringReader = fileReader.ReadLine()
				If stringReader <> String.Empty Then
					Dim line = stringReader.Split(" "c)
					Dim value = String.Empty
					For i = 1 To line.Count - 1
						value += line(i)
					Next

					dictionary.Add(line(0), value)
				End If
			End While
			fileReader.Close()
			result = SortDictionary(dictionary, sortParam)
		End If
		Return result
	End Function

	Private Function Sort(list As List(Of String), sortParam As SortParam) As List(Of String)
		Dim result As New List(Of String)
		Dim dictionary As New Dictionary(Of String, String)
		Dim indexInfo = _indexingMembers.Find(Function(x) x.Name = sortParam.Field)
		If indexInfo IsNot Nothing Then
			Dim indexFileName = GetIndexFileName(Type.GetType(SupportedType.AssemblyQualifiedName), indexInfo.Name)
			Dim fileReader = My.Computer.FileSystem.OpenTextFileReader(indexFileName)
			Dim stringReader = String.Empty
			While fileReader.Peek <> -1
				stringReader = fileReader.ReadLine()
				If stringReader <> String.Empty Then
					Dim line = stringReader.Split(" "c)
					Dim value = String.Empty
					For i = 1 To line.Count - 1
						value += line(i)
					Next
					For Each item In list
						If item.Contains(line(0)) Then
							dictionary.Add(line(0), value)
						End If
					Next
				End If
			End While
			fileReader.Close()
			result = SortDictionary(dictionary, sortParam)
		End If
		Return result
	End Function

	Public Overrides Function FindObj(searchParams As SearchParams) As String()
		If searchParams Is Nothing Then Return FindAllObjs()

		Dim result As New List(Of String)()
		Dim tmpResult As New List(Of String)()
		Dim listResults As New List(Of List(Of String))()

		If searchParams.FindCriterias IsNot Nothing Then
			For Each crit In searchParams.FindCriterias
				tmpResult.Clear()
				Dim indexFileName = String.Empty
				If searchParams.FindCriterias IsNot Nothing Then
					Dim indexInfo = _indexingMembers.Find(Function(x) x.Name = crit.Field)
					If indexInfo IsNot Nothing Then

						indexFileName = GetIndexFileName(Type.GetType(SupportedType.AssemblyQualifiedName), indexInfo.Name)
						Dim fileReader = My.Computer.FileSystem.OpenTextFileReader(indexFileName)
						Dim stringReader = String.Empty
						While fileReader.Peek <> -1
							stringReader = fileReader.ReadLine()
							If stringReader <> String.Empty Then
								Dim line = stringReader.Split(" "c)
								Dim value = String.Empty
								For i = 1 To line.Count - 1
									value += line(i)
								Next
								Select Case (crit.Condition)
									Case FindCondition.eqaul
										If (value = crit.Value) Then tmpResult.Add(line(0))
									Case FindCondition.greater
										If (value > crit.Value) Then tmpResult.Add(line(0))
									Case FindCondition.greaterOrEqual
										If (value >= crit.Value) Then tmpResult.Add(line(0))
									Case FindCondition.less
										If (value < crit.Value) Then tmpResult.Add(line(0))
									Case FindCondition.lessOrEqual
										If (value <= crit.Value) Then tmpResult.Add(line(0))
									Case FindCondition.likeEqaul
										Throw New NotSupportedException
									Case FindCondition.notEqual
										If (value <> crit.Value) Then tmpResult.Add(line(0))
								End Select
							End If
						End While
						fileReader.Close()
						listResults.Add(tmpResult.Select(Function(x) x.Clone().ToString()).ToList())
					Else
						MessageBox.Show(String.Format("Указанный тип ({0}) не является индексируемым"), searchParams.SortParam.Field)
					End If
				End If
			Next
		End If
		If listResults.Count = 1 Then
			result = listResults(0)
		ElseIf listResults.Count > 1 Then
			'находим список с минимальной длиной для сокращения перебора
			Dim minres As List(Of String) = Nothing
			For Each item In listResults
				If (minres Is Nothing Or ((minres IsNot Nothing) AndAlso (minres.Count > item.Count))) Then
					minres = item
				End If
			Next
			'сравнение результатов поиска по каждому критерию
			For Each id In minres
				Dim exists As Boolean = True
				For i = 0 To listResults.Count - 1
					If ((listResults IsNot minres) AndAlso Not (listResults(i).Contains(id))) Then
						exists = False
						Exit For
					End If
				Next
				If (exists) AndAlso Not result.Contains(id) Then
					result.Add(id)
				End If
			Next
		End If

		If searchParams.SortParam IsNot Nothing Then
			If result.Count > 0 Then
				result = Sort(result, searchParams.SortParam)
			Else
				result = Sort(searchParams.SortParam)
			End If
		End If

		If searchParams.SelectOptions IsNot Nothing Then
			If searchParams.SelectOptions.SelectMode = SelectMode.Top Then
				'Top
				If result.Count > 0 Then
					result = result.GetRange(0, Convert.ToInt32(searchParams.SelectOptions.TopValue))
				Else
					result = FindAllObjs().ToList().GetRange(0, Convert.ToInt32(searchParams.SelectOptions.TopValue))
				End If
			Else
				'Between
				If result.Count > 0 Then
					result = result.GetRange(Convert.ToInt32(searchParams.SelectOptions.StartValue),
															 Convert.ToInt32(searchParams.SelectOptions.EndValue - Convert.ToInt32(searchParams.SelectOptions.StartValue) + 1))
				Else
					result = FindAllObjs().ToList().GetRange(Convert.ToInt32(searchParams.SelectOptions.StartValue),
															 Convert.ToInt32(searchParams.SelectOptions.EndValue - Convert.ToInt32(searchParams.SelectOptions.StartValue) + 1))
				End If
			End If
		End If
		Return result.ToArray()
	End Function

	Public Overrides Function FindObjCount(searchParams As SearchParams) As Long
		Dim res As Long = 0
		If Utils.TestFolderFsm(_folder) Then
			Dim files = IO.Directory.GetFiles(_folder, "*.obj.json")
			If files IsNot Nothing Then
				res = files.Length
			End If
		End If
		Return res
	End Function

	Private Function FindAllObjs() As String()
		Dim result As New List(Of String)
		If Utils.TestFolderFsm(_folder) Then
			Dim files = IO.Directory.GetFiles(_folder, "*.obj.json")
			For Each file In files
				Dim fileParts = file.Split(Utils.Sep, "."c)
				result.Add(fileParts(fileParts.Length - 3))
			Next
		End If
		Return result.ToArray
	End Function

	Public Overrides Function GetObj(id As String) As ObjBase
		Dim obj As ObjBase = Nothing
		If Utils.TestFolderFsm(_folder) Then
			Dim file = GetFileName(id)
			Try

				If IO.File.Exists(file) Then
					Dim str = IO.File.ReadAllText(file, Utils.Enc)
					Dim oi = CType(CfJsonConverter.Deserialize(str, GetType(ObjInfo)), ObjInfo)
					obj = CType(CfJsonConverter.Deserialize(oi.Obj, oi.ObjType), ObjBase)
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
		Directory.Delete(_folder, True)
		Utils.TestFolderFsm(_folder)
	End Sub
End Class
