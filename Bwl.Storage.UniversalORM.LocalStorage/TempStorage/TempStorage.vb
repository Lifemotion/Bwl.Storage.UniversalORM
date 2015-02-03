Imports Bwl.Storage.UniversalORM
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class TempStorage
	Inherits Bwl.Storage.UniversalORM.CommonObjStorage
	Implements Bwl.Storage.UniversalORM.IObjStorageManager
	Implements Bwl.Storage.UniversalORM.Blob.IBlobSaver

	Public Property ObjDataInfo As ObjDataInfo

	Public Sub New(type As Type)
		MyBase.New(type)
	End Sub

	Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
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

	Public Overrides Sub AddObj(obj As ObjBase)
		ObjDataInfo = New ObjDataInfo
		ObjDataInfo.ObjInfo = New ObjInfo
		ObjDataInfo.ObjInfo.ObjType = obj.GetType
		ObjDataInfo.ObjInfo.ObjJson = Bwl.Storage.UniversalORM.CfJsonConverter.Serialize(obj)
	End Sub

	Public Overrides Function FindObj(searchParams As SearchParams) As String()
		Return Nothing
	End Function

	Public Overrides Function GetObj(id As String) As ObjBase
		Dim type = ObjDataInfo.ObjInfo.ObjType
		Dim obj = CfJsonConverter.Deserialize(ObjDataInfo.ObjInfo.ObjJson, type)
		Try
			If TypeOf obj Is ObjContainer Then
				Dim objCont = CType(obj, ObjContainer)
				Dim t = objCont.Type
				objCont.Obj = CfJsonConverter.Deserialize(objCont.Obj.ToString, t)
			End If
		Catch ex As Exception
		End Try
		Return obj
	End Function

	Public Overrides Function GetSomeFieldDistinct(fieldName As String) As IEnumerable(Of String)
		Return Nothing
	End Function

	Public Overrides Sub RemoveObj(id As String)

	End Sub

	Public Overrides Sub UpdateObj(obj As ObjBase)

	End Sub

	Public Overrides Function GetObjects(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase)
		Return Nothing
	End Function

	Public Overrides Function Contains(id As String) As Boolean
		Return False
	End Function

	Public Overloads Overrides Function GetObj(Of T As ObjBase)(id As String) As T
		Return Nothing
	End Function

	Public Overloads Overrides Function GetObjects(Of T As ObjBase)(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of T)
		Return Nothing
	End Function

	Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
		Return Me
	End Function

	Public Overrides Sub AddObjects(obj() As ObjBase)
	End Sub

	Public Overrides Sub RemoveAllObjects()

	End Sub

	Public Overrides Function FindObjCount(searchParams As SearchParams) As Long
		Return 0
	End Function
End Class
