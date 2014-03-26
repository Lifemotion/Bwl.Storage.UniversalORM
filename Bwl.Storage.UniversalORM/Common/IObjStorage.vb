Public Interface IObjStorage(Of T As ObjBase)
	Sub AddObj(obj As T)
	Sub UpdateObj(obj As T)
    Sub RemoveObj(id As String)
    Function GetObj(id As String) As T
    Function FindObj(criterias() As FindCriteria) As String()
End Interface
