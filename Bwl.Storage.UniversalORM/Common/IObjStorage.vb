Public Interface IObjStorage(Of T As ObjBase)
	Sub AddObj(obj As T)
	Sub UpdateObj(obj As T)
    Sub RemoveObj(id As String)
	Function GetObj(id As String) As T
	Function GetObjects(objIds As IEnumerable(Of String)) As IEnumerable(Of T)
	Function FindObj(searchParams As SearchParams) As IEnumerable(Of String)
End Interface
