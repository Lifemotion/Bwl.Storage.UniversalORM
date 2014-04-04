Public Class SearchParams

	Dim _findCriterias As IEnumerable(Of FindCriteria)
	Dim _sortParam As SortParam
	Dim _selectoptions As SelectOptions


	Sub New(Optional findCriteria As IEnumerable(Of FindCriteria) = Nothing, Optional sortParam As SortParam = Nothing, Optional selectOptions As SelectOptions = Nothing)
		_findCriterias = findCriteria
		_sortParam = sortParam
		_selectoptions = selectOptions
	End Sub

	Public ReadOnly Property FindCriterias As IEnumerable(Of FindCriteria)
		Get
			Return _findCriterias
		End Get
	End Property

	Public ReadOnly Property SortParam As SortParam
		Get
			Return _sortParam
		End Get
	End Property

	Public ReadOnly Property SelectOptions As SelectOptions
		Get
			Return _selectoptions
		End Get
	End Property
End Class
