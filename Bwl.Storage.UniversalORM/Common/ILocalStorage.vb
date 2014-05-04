Public Interface ILocalStorage
	Sub AddObj(obj As ObjBase)

	''' <summary>
	''' Добоавление объектов одинакового типа в хранилище.
	''' </summary>
	''' <param name="obj"></param>
	''' <remarks></remarks>
	Sub AddObjects(obj As ObjBase())

	Sub UpdateObj(obj As ObjBase)

	Sub RemoveObj(id As String, type As Type)
	Sub RemoveObj(Of T As ObjBase)(id As String)

	Sub RemoveAllObj(type As Type)

	Function GetObj(Of T As ObjBase)(id As String, Optional loadBlob As Boolean = True) As T
	Function GetObj(id As String, type As Type, Optional loadBlob As Boolean = True) As ObjBase

	Function GetObjects(objIds As String(), type As Type, Optional loadBlob As Boolean = True) As IEnumerable(Of ObjBase)
	Function GetObjects(Of T As ObjBase)(objIds As String(), Optional loadBlob As Boolean = True) As IEnumerable(Of T)

	Function FindObj(type As Type, Optional searchParams As SearchParams = Nothing) As String()
	Function FindObj(Of T As ObjBase)(Optional searchParams As SearchParams = Nothing) As String()

	Function FindObjCount(type As Type, Optional searchParams As SearchParams = Nothing) As Long

	Function Contains(id As String, type As Type) As Boolean
	Function Contains(Of T As ObjBase)(id As String) As Boolean
End Interface
