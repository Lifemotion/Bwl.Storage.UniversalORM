Public Class FileStorageManager
	Implements IObjStorageManager

	Private _folder As String

	Public Sub New(folder As String)
		_folder = folder
		Utils.TestFolderFsm(_folder)
	End Sub

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
		Return stor
	End Function

	Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
		Return CreateStorage(name, GetType(T))
	End Function
End Class
