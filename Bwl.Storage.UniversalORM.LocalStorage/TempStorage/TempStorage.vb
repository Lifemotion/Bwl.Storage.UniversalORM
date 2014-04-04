Imports Bwl.Storage.UniversalORM
Imports Newtonsoft.Json

Public Class TempStorage(Of T As ObjBase)
	Inherits Bwl.Storage.UniversalORM.CommonObjStorage(Of T)
	Implements Bwl.Storage.UniversalORM.IObjStorageManager
	Implements Bwl.Storage.UniversalORM.Blob.IBlobSaver

	Public Property ObjDataInfo As ObjDataInfo

	Public Function CreateStorage(Of T1 As ObjBase)(name As String) As IObjStorage(Of T1) Implements IObjStorageManager.CreateStorage
		Return Me
	End Function

	Public Function Load(parentObjId As String) As Blob.ObjBlobInfo Implements Blob.IBlobSaver.Load
		Return ObjDataInfo.ObjBlobInfo
	End Function

	Public Sub Remove(parentObjId As String) Implements Blob.IBlobSaver.Remove

	End Sub

	Public Sub Save(objBlobInfo As Blob.ObjBlobInfo) Implements Blob.IBlobSaver.Save
		ObjDataInfo.ObjBlobInfo = objBlobInfo
	End Sub

	Public Overrides Sub AddObj(obj As T)
		ObjDataInfo = New ObjDataInfo
		ObjDataInfo.ObjInfo = New ObjInfo
		ObjDataInfo.ObjInfo.ObjType = obj.GetType
		ObjDataInfo.ObjInfo.ObjJson = Bwl.Storage.UniversalORM.JsonConverter.Serialize(obj)
	End Sub

	Public Overrides Function FindObj(searchParams As SearchParams) As IEnumerable(Of String)
		Return Nothing
	End Function

	Public Overrides Function GetObj(id As String) As T
		Dim t = ObjDataInfo.ObjInfo.ObjType
		Return JsonConvert.DeserializeObject(ObjDataInfo.ObjInfo.ObjJson, t)
	End Function

	Public Overrides Sub RemoveObj(id As String)

	End Sub

	Public Overrides Sub UpdateObj(obj As T)

	End Sub

	Public Overrides Function GetObjects(objIds As IEnumerable(Of String)) As IEnumerable(Of T)
		Return Nothing
	End Function
End Class
