Public Class Indexing
	Inherits Attribute
	Public Sub New(Optional length As Byte = Byte.MaxValue)
		Me.Length = length
	End Sub
	Public Property Length As UShort
End Class
