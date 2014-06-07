Imports System.Reflection
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
		If Utils.TestFolderFsm(_folder) Then
			Dim file = GetFileName(obj.ID)
			If IO.File.Exists(file) Then Throw New Exception("Object Already Exists with this ID")
			Dim oi = New ObjInfo
			oi.Obj = JsonConverter.Serialize(obj)
			oi.ObjType = obj.GetType
			Dim str = JsonConverter.Serialize(oi)
			IO.File.WriteAllText(file, str, Utils.Enc)
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
		If Utils.TestFolderFsm(_folder) Then
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

	Private Function GetFileName(objId As String) As String
		Return _folder + Utils.Sep + objId + ".obj.json"
	End Function

	Public Overrides Function FindObj(searchParams As SearchParams) As String()
		Return FindAllObjs()
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
			If IO.File.Exists(file) Then
				Dim str = IO.File.ReadAllText(file, Utils.Enc)
				Dim oi = CType(JsonConverter.Deserialize(str, GetType(ObjInfo)), ObjInfo)
				obj = CType(JsonConverter.Deserialize(oi.Obj, oi.ObjType), ObjBase)
			End If
		End If
		Return obj
	End Function

	Public Overrides Function GetObj(Of T As ObjBase)(id As String) As T
		Return CType(GetObj(id), T)
	End Function

	Public Overrides Function GetObjects(objIds As String()) As IEnumerable(Of ObjBase)
		Dim res = New List(Of ObjBase)
		For Each id In objIds
			res.Add(GetObj(id))
		Next
		Return res
	End Function

	Public Overrides Function GetObjects(Of T As ObjBase)(objIds As String()) As IEnumerable(Of T)
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
