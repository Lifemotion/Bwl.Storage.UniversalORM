Public Class SelectOptions

	Dim _selectMode As UniversalORM.SelectMode
	Dim _topValue As Long
	Dim _endValue As Long
	Dim _startValue As Long
	Dim _maxRecordsCount As Long

	Public Sub New()

	End Sub

	Public Sub New(topValue As Long, Optional maxRecordsCount As Long = 0)
		_topValue = topValue
		_selectMode = UniversalORM.SelectMode.Top
		_maxRecordsCount = maxRecordsCount
	End Sub

	Public Sub New(startValue As Long, endValue As Long, maxRecordsCount As Long)
		_selectMode = UniversalORM.SelectMode.Between
		_startValue = startValue
		_endValue = endValue
		_maxRecordsCount = maxRecordsCount
	End Sub

	Public Property MaxRecordsCount As Long
		Get
			Return _maxRecordsCount
		End Get
		Set(value As Long)
			_maxRecordsCount = value
		End Set
	End Property

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
