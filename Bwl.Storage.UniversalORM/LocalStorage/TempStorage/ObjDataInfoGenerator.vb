Imports Bwl.Storage.UniversalORM.LocalStorage
Imports Bwl.Storage.UniversalORM

Public Class ObjDataInfoGenerator
	Private _localStorage As LocalStorage
	Private _tempStorage As TempStorage

	Public Sub New()
		_tempStorage = New TempStorage(GetType(ObjBase))
		_localStorage = New LocalStorage(_tempStorage, _tempStorage)
	End Sub

	Public Function GetObjDataInfo(obj As ObjBase) As ObjDataInfo
		SyncLock (_tempStorage)
			_localStorage.AddObj(obj)
			Return _tempStorage.ObjDataInfo
		End SyncLock
	End Function

	Public Function GetObject(objDataInfo As ObjDataInfo) As ObjBase
		SyncLock (_tempStorage)
			_tempStorage.ObjDataInfo = objDataInfo
			Return _localStorage.GetObj(Of ObjBase)("1")
		End SyncLock
	End Function
End Class
