﻿Public Interface ILocalStorage
	Sub AddObj(obj As ObjBase)
	Sub AddObjects(obj As ObjBase())

	Sub UpdateObj(obj As ObjBase)

	Sub RemoveObj(id As String, type As Type)
	Sub RemoveObj(Of T As ObjBase)(id As String)

	Function GetObj(Of T As ObjBase)(id As String, Optional loadBlob As Boolean = True) As T
	Function GetObj(id As String, type As Type, Optional loadBlob As Boolean = True) As ObjBase

	Function GetObjects(objIds As IEnumerable(Of String), type As Type, Optional loadBlob As Boolean = True) As IEnumerable(Of ObjBase)
	Function GetObjects(Of T As ObjBase)(objIds As IEnumerable(Of String), Optional loadBlob As Boolean = True) As IEnumerable(Of T)

	Function FindObj(type As Type, Optional searchParams As SearchParams = Nothing) As IEnumerable(Of String)
	Function FindObj(Of T As ObjBase)(Optional searchParams As SearchParams = Nothing) As IEnumerable(Of String)

	Function Contains(id As String, type As Type) As Boolean
	Function Contains(Of T As ObjBase)(id As String) As Boolean
End Interface