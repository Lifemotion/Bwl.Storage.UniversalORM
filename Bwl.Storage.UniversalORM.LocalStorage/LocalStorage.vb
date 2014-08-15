Imports System.IO
Imports Bwl.Storage.UniversalORM.AdoDb
Imports Bwl.Storage.UniversalORM
Imports Bwl.Storage.UniversalORM.Blob

''' <summary>
''' Локальное хранилище объектов в JSON + BLOB.
''' </summary>
''' <remarks></remarks>
Public Class LocalStorage
	Implements ILocalStorage

	Protected ReadOnly _blobStorage As CommonBlobStorage
	Protected ReadOnly _blobSaver As IBlobSaver
	Protected ReadOnly _storages As New Dictionary(Of Type, IObjStorage)
	Protected ReadOnly _storageManager As IObjStorageManager

	''' <summary>
	''' Локальное хранилище объектов в JSON + BLOB.
	''' </summary>
	''' <param name="storageManager">Менеджер хранилищ JSON описания.</param>
	''' <param name="blobSaver">Модуль для хранения BLOB данных.</param>
	''' <param name="blobStremSavers">Преобразователи BLOB данных в потоки.</param>
	''' <remarks></remarks>
	Public Sub New(storageManager As IObjStorageManager, blobSaver As IBlobSaver, Optional blobStremSavers As IBlobBinarySaver() = Nothing)
		_blobSaver = blobSaver
		_blobStorage = New CommonBlobStorage()
		_blobStorage.AddSaver(_blobSaver)

		If (blobStremSavers IsNot Nothing AndAlso blobStremSavers.Any) Then
			For Each streamSaver In blobStremSavers
				_blobStorage.AddStreamSaver(streamSaver)
			Next
		End If
		_storageManager = storageManager
	End Sub

	Private Function GetStorage(type As Type) As IObjStorage
		If type Is Nothing Then Return Nothing
		SyncLock (_storages)
			If (Not _storages.ContainsKey(type)) Then
				Dim storage = _storageManager.CreateStorage(type.Name, type)
				_storages.Add(type, storage)
			End If
			Return _storages(type)
		End SyncLock
	End Function

	Public Overridable Sub AddObj(obj As ObjBase, Optional type As Type = Nothing) Implements ILocalStorage.AddObj
		Try
			If type Is Nothing Then
				type = obj.GetType
			End If
			Dim storage = GetStorage(type)
			If (String.IsNullOrEmpty(obj.ID)) Then
				obj.ID = Guid.NewGuid.ToString("B")
			End If
			storage.AddObj(obj)
			_blobStorage.SaveBlobs(obj, obj.ID)
		Catch ex As Exception
			Dim objStr = "Nothing"
			If obj IsNot Nothing Then
				objStr = obj.ToString
			End If
			Dim tStr = "Nothing"
			If type IsNot Nothing Then
				tStr = type.ToString
			End If

			Throw New Exception("Local.Storage(" + objStr + ", " + tStr + vbCrLf + ex.ToString, ex)
		End Try
	End Sub

	Public Overridable Sub AddObjects(objects() As ObjBase, Optional type As Type = Nothing) Implements ILocalStorage.AddObjects
		Dim objIds = New List(Of String)
		For Each obj In objects
			If (String.IsNullOrEmpty(obj.ID)) Then
				obj.ID = Guid.NewGuid.ToString("B")
			End If
			objIds.Add(obj.ID)
		Next

		If type Is Nothing Then
			type = objects.First.GetType
		End If
		Dim storage = GetStorage(type)
		If storage IsNot Nothing Then
			storage.AddObjects(objects)
			_blobStorage.SaveBlobs(objects, objIds.ToArray)
		End If
	End Sub

	Public Overridable Sub UpdateObj(obj As ObjBase, Optional type As Type = Nothing) Implements ILocalStorage.UpdateObj
		If type Is Nothing Then
			type = obj.GetType
		End If
		Dim storage = GetStorage(type)
		If storage IsNot Nothing Then
			storage.UpdateObj(obj)
		End If
		_blobStorage.SaveBlobs(obj, obj.ID)
	End Sub

	Public Overridable Function Contains(id As String, type As Type) As Boolean Implements ILocalStorage.Contains
		Dim storage = GetStorage(type)
		If storage IsNot Nothing Then
			Return storage.Contains(id)
		End If
		Return Nothing
	End Function

	Public Overridable Function Contains(Of T As ObjBase)(id As String) As Boolean Implements ILocalStorage.Contains
		Return Contains(id, GetType(T))
	End Function

	Public Overridable Function FindObj(type As Type, Optional searchParams As SearchParams = Nothing) As String() Implements ILocalStorage.FindObj
		Dim storage = GetStorage(type)
		If storage IsNot Nothing Then
			Return storage.FindObj(searchParams)
		End If
		Return Nothing
	End Function

	Public Function FindObjCount(type As Type, Optional searchParams As SearchParams = Nothing) As Long Implements ILocalStorage.FindObjCount
		Dim res As Long = 0
		Dim storage = GetStorage(type)
		If storage IsNot Nothing Then
			res = storage.FindObjCount(searchParams)
		End If
		Return res
	End Function

	Public Overridable Function FindObj(Of T As ObjBase)(Optional searchParams As SearchParams = Nothing) As String() Implements ILocalStorage.FindObj
		Return FindObj(GetType(T), searchParams)
	End Function

	Public Overridable Function GetObj(id As String, type As Type, Optional loadBlob As Boolean = True) As ObjBase Implements ILocalStorage.GetObj
		Dim storage = GetStorage(type)
		Dim obj = Nothing
		If storage IsNot Nothing Then
			obj = storage.GetObj(id)
			If obj IsNot Nothing AndAlso loadBlob Then
				_blobStorage.LoadBlobs(obj, obj.ID)
			End If
		End If
		Return obj
	End Function

	Public Overridable Function GetObj(Of T As ObjBase)(id As String, Optional loadBlob As Boolean = True) As T Implements ILocalStorage.GetObj
		Return GetObj(id, GetType(T), loadBlob)
	End Function

	Public Overridable Function GetObjects(objIds As String(), type As Type, Optional loadBlob As Boolean = True, Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase) Implements ILocalStorage.GetObjects
		Dim storage = GetStorage(type)
		If storage IsNot Nothing Then
			Dim objects = storage.GetObjects(objIds, sortParam)
			If (loadBlob) Then
				_blobStorage.LoadBlobs(objects.ToArray, objIds.ToArray)
			End If
			Return objects
		End If
		Return Nothing
	End Function

	Public Overridable Function GetObjects(Of T As ObjBase)(objIds As String(), Optional loadBlob As Boolean = True, Optional sortParam As SortParam = Nothing) As IEnumerable(Of T) Implements ILocalStorage.GetObjects
		Dim storage = GetStorage(GetType(T))
		If storage IsNot Nothing Then
			Dim objects = storage.GetObjects(Of T)(objIds, sortParam)
			If (loadBlob) Then
				_blobStorage.LoadBlobs(objects.ToArray, objIds.ToArray)
			End If
			Return objects
		End If
		Return Nothing
	End Function

	Public Overridable Sub RemoveObj(id As String, type As Type) Implements ILocalStorage.RemoveObj
		Dim storage = GetStorage(type)
		If storage IsNot Nothing Then
			storage.RemoveObj(id)
		End If
		_blobStorage.Remove(id)
	End Sub

	Public Overridable Sub RemoveObj(Of T As ObjBase)(id As String) Implements ILocalStorage.RemoveObj
		RemoveObj(id, GetType(T))
	End Sub

	Public ReadOnly Property BlobStorage As IBlobStorage
		Get
			Return _blobStorage
		End Get
	End Property

	Public ReadOnly Property BlobSaver As IBlobSaver
		Get
			Return _blobSaver
		End Get
	End Property

	Public Overridable Sub RemoveAllObj(type As Type) Implements ILocalStorage.RemoveAllObj
		Dim storage = GetStorage(type)
		If storage IsNot Nothing Then
			Dim ids = storage.FindObj(Nothing)
			For Each id In ids
				_blobStorage.Remove(id)
			Next
			storage.RemoveAllObjects()
		End If
	End Sub
End Class
