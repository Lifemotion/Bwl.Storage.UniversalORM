Imports System.Reflection

Public Class FileObjStorage(Of T As ObjBase)
    Implements IObjStorage(Of T)
    Private _folder As String
    Private _indexingMembers As String()

    Friend Sub New(folder As String)
        _folder = folder
        _indexingMembers = ReflectionTools.GetIndexingMemberNames(GetType(T))

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

	Public Sub Add(obj As T) Implements IObjStorage(Of T).AddObj
		Dim file = GetFileName(obj.ID)
		If IO.File.Exists(file) Then Throw New Exception("Object Already Exists with this ID")
		Dim str = JsonConverter.Serialize(obj)
		IO.File.WriteAllText(file, str, Utils.Enc)
		For Each indexing In _indexingMembers
			Dim indexValue = ReflectionTools.GetMemberValue(indexing, obj).ToString
			Dim path = GetIndexPath(indexing, MD5.GetHash(indexValue))
			IO.File.WriteAllText(path + Utils.Sep + obj.ID + ".hash", "")
		Next
	End Sub

	Public Sub UpdateObj(obj As T) Implements IObjStorage(Of T).UpdateObj
		Dim file = GetFileName(obj.ID)
		If Not IO.File.Exists(file) Then Throw New Exception("Object Not Exists with this ID")

		IO.File.Delete(file)
		Add(obj)
	End Sub

    Public Sub Remove(id As String) Implements IObjStorage(Of T).RemoveObj
        Dim file = GetFileName(id)
        If Not IO.File.Exists(file) Then Throw New Exception("Object Not Exists with this ID")

        IO.File.Delete(file)
    End Sub

    Private Function GetFileName(objId As String) As String
        Return _folder + Utils.Sep + objId + ".obj.json"
    End Function

    Public Function FindObj(criterias() As FindCriteria) As String() Implements IObjStorage(Of T).FindObj
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

    Public Function GetObj(id As String) As T Implements IObjStorage(Of T).GetObj
        Dim file = GetFileName(id)
        If Not IO.File.Exists(file) Then Return Nothing
        Dim str = IO.File.ReadAllText(file, Utils.Enc)
        Dim obj = JsonConverter.Deserialize(Of T)(str)
        Return obj
    End Function
End Class
