Imports System.Reflection

Public Class FileObjStorage(Of T As ObjBase)
    Implements IObjStorage(Of T)
    Private _folder As String
    Private _indexingMembers As String()
    Private _md5 As System.Security.Cryptography.MD5 = System.Security.Cryptography.MD5.Create
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

    Public Sub Add(obj As T) Implements IObjStorage(Of T).Add
        Dim file = GetFileName(obj.ID)
        If IO.File.Exists(file) Then Throw New Exception("Object Already Exists with this ID")
        Dim str = JsonConverter.Serialize(obj)
        '  IO.File.WriteAllText(file, str, Utils.Enc)
        For Each indexing In _indexingMembers
            Dim indexValue = ReflectionTools.GetMemberValue(indexing, obj).ToString
            Dim bytes = Utils.Enc.GetBytes(indexValue)
            Dim hash = System.Convert.ToBase64String(_md5.ComputeHash(bytes)).Replace("/", "-")
            Dim path = GetIndexPath(indexing, hash)
            IO.File.WriteAllText(path + Utils.Sep + obj.ID + ".hash", "")
        Next
    End Sub

    Public Function Find(criterias() As FindCriteria) As T() Implements IObjStorage(Of T).Find

    End Function

    Public Function Find(id As String) As T Implements IObjStorage(Of T).Find
        Dim file = GetFileName(id)
        If Not IO.File.Exists(file) Then Return Nothing
        Dim str = IO.File.ReadAllText(file, Utils.Enc)
        Dim obj = JsonConverter.Deserialize(Of T)(str)
        Return obj
    End Function

    Public Sub Remove(id As String) Implements IObjStorage(Of T).Remove
        Dim file = GetFileName(id)
        If Not IO.File.Exists(file) Then Throw New Exception("Object Not Exists with this ID")
        IO.File.Delete(file)
    End Sub

    Private Function GetFileName(objId As String) As String
        Return _folder + Utils.Sep + "obj_" + objId + ".json"
    End Function
End Class
