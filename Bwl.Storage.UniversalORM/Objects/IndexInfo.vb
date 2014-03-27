Public Class IndexInfo
	Private _name As String
	Private _type As Type
	Private _length As UShort

	Public Sub New(name As String, type As Type, length As UShort)
		_name = name
		_type = type
		_length = length
	End Sub

	Public ReadOnly Property Name As String
		Get
			Return _name
		End Get
	End Property

	Public ReadOnly Property Type As Type
		Get
			Return _type
		End Get
	End Property

	Public ReadOnly Property Length As UShort
		Get
			Return _length
		End Get
	End Property
End Class
