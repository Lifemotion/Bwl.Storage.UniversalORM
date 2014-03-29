Imports Bwl.Storage.UniversalORM.LocalStorage
Imports Bwl.Storage.UniversalORM

Public Class ObjDataInfoGenerator
	Private _localStorage As LocalStorage
	Private _tempStorage As TempStorage(Of ObjBase)

	Public Sub New()
		_tempStorage = New TempStorage(Of ObjBase)
		_localStorage = New LocalStorage(_tempStorage, _tempStorage)
	End Sub

	Public Function GetObjDataInfo(obj As ObjBase) As ObjDataInfo
		_localStorage.Save(obj)
		Return _tempStorage.ObjDataInfo
	End Function

	Public Function GetObject(objDataInfo As ObjDataInfo) As ObjBase
		_tempStorage.ObjDataInfo = objDataInfo
		Return _localStorage.Load(Of ObjBase)("1")
	End Function
End Class
