Public Class SortParam

	Dim _sortMode As UniversalORM.SortMode
	Dim _field As String

	Public Sub New(field As String, sortMode As SortMode)
		_sortMode = sortMode
		_field = field
	End Sub

	Public ReadOnly Property SortMode As SortMode
		Get
			Return _sortMode
		End Get
	End Property

	Public ReadOnly Property Field As String
		Get
			Return _field
		End Get
	End Property
End Class
