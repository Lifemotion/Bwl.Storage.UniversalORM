Public MustInherit Class CommonObjStorage
	Implements IObjStorage

	Private ReadOnly _supportedType As Type

	''' <summary>Описание индексируемых полей.</summary>
	Protected ReadOnly _indexingMembers As New List(Of IndexInfo)()

	Public Sub New(type As Type)
		_supportedType = type
		If (_indexingMembers IsNot Nothing) Then
			_indexingMembers.AddRange(ReflectionTools.GetIndexingMemberNames(_supportedType))
		End If
	End Sub

	Public MustOverride Function FindObj(searchParams As SearchParams) As String() Implements IObjStorage.FindObj

	Public MustOverride Function GetObj(id As String) As ObjBase Implements IObjStorage.GetObj

	Public MustOverride Function GetObj(Of T As ObjBase)(id As String) As T Implements IObjStorage.GetObj

	Public MustOverride Function GetObjects(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase) Implements IObjStorage.GetObjects

	Public MustOverride Function GetObjects(Of T As ObjBase)(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of T) Implements IObjStorage.GetObjects

	Public MustOverride Sub RemoveObj(id As String) Implements IObjStorage.RemoveObj

	Public MustOverride Sub UpdateObj(obj As ObjBase) Implements IObjStorage.UpdateObj

	Public MustOverride Sub AddObj(obj As ObjBase) Implements IObjStorage.AddObj

	Public MustOverride Function Contains(id As String) As Boolean Implements IObjStorage.Contains

	Public MustOverride Sub AddObjects(obj() As ObjBase) Implements IObjStorage.AddObjects

	Public ReadOnly Property SupportedType As Type Implements IObjStorage.SupportedType
		Get
			Return _supportedType
		End Get
	End Property

	Public MustOverride Sub RemoveAllObjects() Implements IObjStorage.RemoveAllObjects

	Public MustOverride Function FindObjCount(searchParams As SearchParams) As Long Implements IObjStorage.FindObjCount

	Public MustOverride Function GetSomeFieldDistinct(fieldName As String) As IEnumerable(Of String) Implements IObjStorage.GetSomeFieldDistinct

End Class
