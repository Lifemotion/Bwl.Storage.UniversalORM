Public Class SearchParams

    Private _findCriterias As IEnumerable(Of FindCriteria)
    Private _sortParam As SortParam
    Private _selectOptions As SelectOptions
    Private _generateIndex As Boolean

    Public Sub New()
        Me.New(Nothing, Nothing, Nothing)
    End Sub

    Sub New(Optional findCriteria As IEnumerable(Of FindCriteria) = Nothing, Optional sortParam As SortParam = Nothing, Optional selectOptions As SelectOptions = Nothing, Optional generateIndex As Boolean = False)
        _findCriterias = findCriteria
        _sortParam = sortParam
        _selectOptions = selectOptions
        _generateIndex = generateIndex
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
            Return _selectOptions
        End Get
        Set(value As SelectOptions)
            _selectOptions = value
        End Set
    End Property

    Public Property GenerateIndex As Boolean
        Get
            Return _generateIndex
        End Get
        Set(value As Boolean)
            _generateIndex = value
        End Set
    End Property
End Class
