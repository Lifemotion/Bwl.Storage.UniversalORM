Public Class FileStorageManager
	Implements IObjStorageManager

	Private _folder As String

	Private ReadOnly _storages As New List(Of FileObjStorage)()

	Private _useUndexing As Boolean = False

	Public Sub New(folder As String)
		_folder = folder
		Utils.TestFolderFsm(_folder)
	End Sub

	Public Property UseIndexing As Boolean
		Get
			Return _useUndexing
		End Get
		Set(value As Boolean)
			_useUndexing = value
			SyncLock (_storages)
				For Each st In _storages
					st.UseIndexing = _useUndexing
				Next
			End SyncLock
		End Set
	End Property

	Public Property FileStorageDir As String
		Get
			Return _folder
		End Get
		Set(value As String)
			_folder = value
		End Set
	End Property

	Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
		Dim path = _folder + Utils.Sep + name
		Dim stor As FileObjStorage = Nothing
		If Utils.TestFolderFsm(path) Then
			stor = New FileObjStorage(path, type)
		End If

		stor.UseIndexing = _useUndexing

		SyncLock (_storages)
			_storages.Add(stor)
		End SyncLock

		Return stor
	End Function

	Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
		Return CreateStorage(name, GetType(T))
	End Function
End Class
