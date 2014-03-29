Imports System.Reflection

Public Class FileObjStorage(Of T As ObjBase)
	Inherits CommonObjStorage(Of T)

	Private _folder As String

	Friend Sub New(folder As String)
		_folder = folder
	End Sub

	Private Function GetIndexPath(index As String)
		Dim path = _folder + Utils.Sep + "index_" + index
		If Not IO.Directory.Exists(path) Then IO.Directory.CreateDirectory(path)
		Return path
	End Function

	Private Function GetIndexPath(index As String, hash As String)
		Dim path = _folder + Utils.Sep + "index_" + index + Utils.Sep + hash
		If Not IO.Directory.Exists(path) Then IO.Directory.CreateDirectory(path)
		Return path
	End Function

	Public Property StorageDir As String
		Get
			Return _folder
		End Get
		Set(value As String)
			_folder = value
		End Set
	End Property

	Public Overrides Sub AddObj(obj As T)
		Dim file = GetFileName(obj.ID)
		If IO.File.Exists(file) Then Throw New Exception("Object Already Exists with this ID")
		Dim str = JsonConverter.Serialize(obj)
		IO.File.WriteAllText(file, str, Utils.Enc)
		For Each indexing In _indexingMembers
			Try
				Dim indexValue = ReflectionTools.GetMemberValue(indexing.Name, obj).ToString
				Dim path = GetIndexPath(indexing.Name, MD5.GetHash(indexValue))
				IO.File.WriteAllText(path + Utils.Sep + obj.ID + ".hash", "")
			Catch ex As Exception
				'...
			End Try
		Next
	End Sub

	Public Overrides Sub UpdateObj(obj As T)
		Dim file = GetFileName(obj.ID)
		If Not IO.File.Exists(file) Then Throw New Exception("Object Not Exists with this ID")

		IO.File.Delete(file)
		AddObj(obj)
	End Sub

	Public Overrides Sub RemoveObj(id As String)
		Dim file = GetFileName(id)
		If Not IO.File.Exists(file) Then Throw New Exception("Object Not Exists with this ID")

		IO.File.Delete(file)
	End Sub

	Private Function GetFileName(objId As String) As String
		Return _folder + Utils.Sep + objId + ".obj.json"
	End Function

	Public Overrides Function FindObj(criterias() As FindCriteria) As String()
		Return FindAllObjs()
	End Function

	Private Function FindAllObjs() As String()
		Dim files = IO.Directory.GetFiles(_folder, "*.obj.json")
		Dim result As New List(Of String)
		For Each file In files
			Dim fileParts = file.Split(Utils.Sep, "."c)
			result.Add(fileParts(fileParts.Length - 3))
		Next
		Return result.ToArray
	End Function

	Public Overrides Function GetObj(id As String) As T
		Dim file = GetFileName(id)
		If Not IO.File.Exists(file) Then Return Nothing
		Dim str = IO.File.ReadAllText(file, Utils.Enc)
		Dim obj = JsonConverter.Deserialize(Of T)(str)
		Return obj
	End Function
End Class
