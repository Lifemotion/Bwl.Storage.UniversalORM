Public Class FindCriteria
    Public Property Value As Object
    Public Property Field As String
    Public Property Condition As FindCondition

    ''' <summary>
    ''' Новое условие поиска
    ''' </summary>
    ''' <param name="field">Поле</param>
    ''' <param name="cond">Условие</param>
    ''' <param name="value">При multiple-условии указать сериализованный в json массив строк</param>
    Public Sub New(field As String, cond As FindCondition, value As Object)
        _Field = field
        _Value = value
        _Condition = cond
    End Sub
End Class
