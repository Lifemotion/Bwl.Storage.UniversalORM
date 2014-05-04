Public Class SelectOptions

	Dim _selectMode As UniversalORM.SelectMode
	Dim _topValue As Long
	Dim _endValue As Long
	Dim _startValue As Long
	Dim _maxRecordsCount As Long

	Public Sub New()

	End Sub

	Public Sub New(topValue As Long)
		_topValue = topValue
		_selectMode = UniversalORM.SelectMode.Top
	End Sub

	Public Sub New(startValue As Long, endValue As Long)
		_selectMode = UniversalORM.SelectMode.Between
		_startValue = startValue
		_endValue = endValue
	End Sub

	Public Property TopValue As Long
		Get
			Return _topValue
		End Get
		Set(value As Long)
			_topValue = value
		End Set
	End Property

	Public Property StartValue As Long
		Get
			Return _startValue
		End Get
		Set(value As Long)
			_startValue = value
		End Set
	End Property

	Public Property EndValue As Long
		Get
			Return _endValue
		End Get
		Set(value As Long)
			_endValue = value
		End Set
	End Property

	Public Property SelectMode As SelectMode
		Get
			Return _selectMode
		End Get
		Set(value As SelectMode)
			_selectMode = value
		End Set
	End Property

End Class

