Public Interface IObjStorageManager
	Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage
	Function CreateStorage(name As String, type As Type) As IObjStorage
End Interface
