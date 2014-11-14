Public Class ObjContainer
	Implements ObjBase

	Private _obj As Object

	Public Property Obj As Object
		Get
			Return _obj
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

Public Class ObjContainer2
	Implements ObjBase

	Private _objStr As String
	Private _obj As Object
	Private _type As Object

	<BlobContainer>
	<JsonIgnore>
	Public Property Obj As Object
		Get
			Return _obj
		End Get
		Set(value As Object)
			_obj = value
			If (_obj IsNot Nothing) Then
				_type = _obj.GetType
				_objStr = CfJsonConverter.Serialize(_obj)
			End If
		End Set
	End Property

	Public Property ObjStr As String
		Get
			Return _objStr
		End Get
		Set(value As String)
			_objStr = value
			If (_objStr IsNot Nothing AndAlso _type IsNot Nothing) Then
				_obj = CfJsonConverter.Deserialize(_objStr, Type)
			End If
		End Set
	End Property

	Public Property Type As Type
		Get
			Return _type
		End Get
		Set(value As Type)
			_type = value
			If (_objStr IsNot Nothing AndAlso _type IsNot Nothing) Then
				_obj = CfJsonConverter.Deserialize(_objStr, _type)
			End If
		End Set
	End Property

	Public Property ID As String Implements ObjBase.ID
End Class
