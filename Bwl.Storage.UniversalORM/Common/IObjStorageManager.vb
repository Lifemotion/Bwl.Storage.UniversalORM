Public Interface IObjStorageManager
	Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage(Of T)
End Interface
