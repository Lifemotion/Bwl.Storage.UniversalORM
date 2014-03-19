Public Class FindCriteria
    Public Property Value As Object
    Public Property Field As String
    Public Property Condition As FindCondition

    Public Sub New(field As String, cond As FindCondition, value As Object)
        _Field = field
        _Value = value
        _Condition = cond
    End Sub
End Class
