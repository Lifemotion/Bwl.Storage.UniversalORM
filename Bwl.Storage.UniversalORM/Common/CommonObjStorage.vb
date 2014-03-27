Public MustInherit Class CommonObjStorage(Of T As ObjBase)
	Implements IObjStorage(Of T)

	''' <summary>Описание индексируемых полей.</summary>
	Protected Shared ReadOnly _indexingMembers As New List(Of IndexInfo)()

	Shared Sub New()
		SyncLock (_indexingMembers)
			If (_indexingMembers IsNot Nothing) Then
				_indexingMembers.AddRange(ReflectionTools.GetIndexingMemberNames(GetType(T)))
			End If
		End SyncLock
	End Sub

	Public MustOverride Sub AddObj(obj As T) Implements IObjStorage(Of T).AddObj

	Public MustOverride Function FindObj(criterias() As FindCriteria) As String() Implements IObjStorage(Of T).FindObj

	Public MustOverride Function GetObj(id As String) As T Implements IObjStorage(Of T).GetObj

	Public MustOverride Sub RemoveObj(id As String) Implements IObjStorage(Of T).RemoveObj

	Public MustOverride Sub UpdateObj(obj As T) Implements IObjStorage(Of T).UpdateObj

End Class
