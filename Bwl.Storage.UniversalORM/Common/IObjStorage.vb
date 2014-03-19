Public Interface IObjStorage(Of T As ObjBase)
    Sub Add(obj As T)
    Sub Remove(id As String)
    Function Find(id As String) As T
    Function Find(criterias() As FindCriteria) As T()
End Interface
