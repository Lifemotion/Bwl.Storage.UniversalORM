Public Class MemoryStorageManager
	Implements IObjStorageManager

	Public Sub New()

	End Sub

	Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
		Return New MemoryStorage(type)
	End Function

	Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
		Return CreateStorage(name, GetType(T))
	End Function
End Class
