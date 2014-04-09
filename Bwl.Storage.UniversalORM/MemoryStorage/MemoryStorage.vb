Imports System.Reflection
Imports System.IO

Public Class MemoryStorage
	Inherits CommonObjStorage

	Private ReadOnly _objects As New Dictionary(Of String, ObjBase)

	Friend Sub New(type As Type)
		MyBase.New(type)
	End Sub

	Public Overrides Sub AddObj(obj As ObjBase)
		_objects(obj.ID) = obj
	End Sub

	Public Overrides Sub UpdateObj(obj As ObjBase)
		Throw New NotSupportedException()
	End Sub

	Public Overrides Sub RemoveObj(id As String)
		If Contains(id) Then
			_objects.Remove(id)
		End If
	End Sub

	Public Overrides Function FindObj(searchParams As SearchParams) As IEnumerable(Of String)
		Return _objects.Keys
	End Function

	Public Overrides Function GetObj(id As String) As ObjBase
		Return _objects(id)
	End Function

	Public Overrides Function GetObj(Of T As ObjBase)(id As String) As T
		Return CType(GetObj(id), T)
	End Function

	Public Overrides Function GetObjects(objIds As IEnumerable(Of String)) As IEnumerable(Of ObjBase)
		Return Nothing
	End Function

	Public Overrides Function Contains(id As String) As Boolean
		Return _objects.ContainsKey(id)
	End Function

	Public Overrides Function GetObjects(Of T As ObjBase)(objIds As IEnumerable(Of String)) As IEnumerable(Of T)
		Return Nothing
	End Function

	Public Overrides Sub AddObjects(objects() As ObjBase)
		For Each obj In objects
			AddObj(obj)
		Next
	End Sub
End Class
