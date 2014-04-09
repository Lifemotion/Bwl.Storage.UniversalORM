Public Class ObjContainer
	Implements ObjBase

	Private _obj As Object

	Public Property Obj As Object
		Get
			Return _Obj
		End Get
		Set(value As Object)
			_obj = value
			If (_obj IsNot Nothing) Then
				Type = _obj.GetType
			End If
		End Set
	End Property

	Public Property ID As String Implements ObjBase.ID
	Public Property Type As Type
End Class
