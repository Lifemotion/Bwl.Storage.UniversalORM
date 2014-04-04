Imports System.IO
Imports Bwl.Storage.UniversalORM.AdoDb
Imports Bwl.Storage.UniversalORM
Imports Bwl.Storage.UniversalORM.Blob

''' <summary>
''' Локальное хранилище объектов в JSON + BLOB.
''' </summary>
''' <remarks></remarks>
Public Class LocalStorage
	Private ReadOnly _blobStorage As CommonBlobStorage
	Private ReadOnly _fileBlobSaver As FileBlobSaver
	Private ReadOnly _storages As New Dictionary(Of Type, Object)
	Private ReadOnly _storageManager As IObjStorageManager

	''' <summary>
	''' Локальное хранилище объектов в JSON + BLOB.
	''' </summary>
	''' <param name="storageManager">Менеджер хранилищ JSON описания.</param>
	''' <param name="blobSaver">Модуль для хранения BLOB данных.</param>
	''' <param name="blobStremSavers">Преобразователи BLOB данных в потоки.</param>
	''' <remarks></remarks>
	Public Sub New(storageManager As IObjStorageManager, blobSaver As IBlobSaver, Optional blobStremSavers As IBlobBinarySaver() = Nothing)
		_blobStorage = New CommonBlobStorage()
		_blobStorage.AddSaver(blobSaver)

		If (blobStremSavers IsNot Nothing AndAlso blobStremSavers.Any) Then
			For Each streamSaver In blobStremSavers
				_blobStorage.AddStreamSaver(streamSaver)
			Next
		End If
		_storageManager = storageManager
	End Sub

	Public Function Save(Of T As ObjBase)(obj As T) As String
		Dim storage = GetStorage(Of T)()
		If (String.IsNullOrEmpty(obj.ID)) Then
			obj.ID = Guid.NewGuid.ToString("B")
		End If
		storage.AddObj(obj)
		_blobStorage.SaveBlobs(obj, obj.ID)
		Return obj.ID
	End Function

	Public Function Load(Of T As ObjBase)(id As String) As T
		Dim storage = GetStorage(Of T)()
		Dim obj = storage.GetObj(id)
		_blobStorage.LoadBlobs(obj, id)
		Return obj
	End Function

	Public Function LoadObjects(Of T As ObjBase)(objIds As IEnumerable(Of String)) As IEnumerable(Of T)
		Dim storage = GetStorage(Of T)()
		Dim objects = storage.GetObjects(objIds)
		If (objects IsNot Nothing) Then
			For Each obj In objects
				_blobStorage.LoadBlobs(obj, obj.ID)
			Next
		End If
		Return objects
	End Function

	Public Sub Update(Of T As ObjBase)(obj As T)
		Dim storage = GetStorage(Of T)()
		If storage IsNot Nothing Then
			storage.UpdateObj(obj)
		End If
	End Sub

	Private Function GetStorage(Of T As ObjBase)() As IObjStorage(Of T)
		SyncLock (_storages)
			Dim type = GetType(T)
			If (Not _storages.ContainsKey(type)) Then
				Dim storage = _storageManager.CreateStorage(Of T)(type.Name)
				_storages.Add(type, storage)
			End If
			Return _storages(type)
		End SyncLock
	End Function

	Public Function FindObj(Of T As ObjBase)(Optional searchParams As SearchParams = Nothing) As IEnumerable(Of String)
		Dim storage = GetStorage(Of T)()
		If storage IsNot Nothing Then
			Return storage.FindObj(searchParams)
		End If
		Return Nothing
	End Function

	Public Sub Remove(Of T As ObjBase)(id As String)
		Dim storage = GetStorage(Of T)()
		If storage IsNot Nothing Then
			storage.RemoveObj(id)
		End If
		_blobStorage.Remove(id)
	End Sub
End Class
