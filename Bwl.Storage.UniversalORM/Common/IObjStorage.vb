﻿Public Interface IObjStorage
	Sub AddObj(obj As ObjBase)
	Sub AddObjects(obj As ObjBase())

	Sub UpdateObj(obj As ObjBase)

	Sub RemoveObj(id As String)
	Sub RemoveAllObjects()

	Function GetObj(Of T As ObjBase)(id As String) As T
	Function GetObj(id As String) As ObjBase

	Function GetObjects(objIds As String()) As IEnumerable(Of ObjBase)
	Function GetObjects(Of T As ObjBase)(objIds As String()) As IEnumerable(Of T)

	Function FindObj(searchParams As SearchParams) As String()
	Function FindObjCount(searchParams As SearchParams) As Long

	ReadOnly Property SupportedType As Type

	Function Contains(id As String) As Boolean
End Interface
