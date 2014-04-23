Public Class BetweenParam
	Public Sub New()

	End Sub
	Public Sub New(startValue As Long, endValue As Long)
		Me.StartValue = startValue
		Me.EndValue = endValue
	End Sub

	Public Property StartValue As Long
	Public Property EndValue As Long
End Class
