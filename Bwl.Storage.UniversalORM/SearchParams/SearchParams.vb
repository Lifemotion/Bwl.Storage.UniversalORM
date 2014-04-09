Public Class SearchParams

	Dim _findCriterias As IEnumerable(Of FindCriteria)
	Dim _sortParam As SortParam
	Dim _selectoptions As SelectOptions

	Public Sub New()
		Me.New(Nothing, Nothing, Nothing)
	End Sub

	Sub New(Optional findCriteria As IEnumerable(Of FindCriteria) = Nothing, Optional sortParam As SortParam = Nothing, Optional selectOptions As SelectOptions = Nothing)
		_findCriterias = findCriteria
		_sortParam = sortParam
		_selectoptions = selectOptions
	End Sub

	Public Property FindCriterias As IEnumerable(Of FindCriteria)
		Get
			Return _findCriterias
		End Get
		Set(value As IEnumerable(Of FindCriteria))
			_findCriterias = value
		End Set
	End Property

	Public Property SortParam As SortParam
		Get
			Return _sortParam
		End Get
		Set(value As SortParam)
			_sortParam = value
		End Set
	End Property

	Public Property SelectOptions As SelectOptions
		Get
			Return _selectoptions
		End Get
		Set(value As SelectOptions)
			_selectoptions = value
		End Set
	End Property
End Class
