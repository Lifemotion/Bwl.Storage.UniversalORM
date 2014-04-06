Imports System.Drawing

Public Class BytesBinarySaver
	Implements Blob.IBlobBinarySaver

	Public Function FromBinary(data As Byte(), blobType As Type) As Object Implements IBlobBinarySaver.FromBinary
		Return data
	End Function

	Public Function ToBinary(blob As Object) As Byte() Implements IBlobBinarySaver.ToBinary
		If (blob IsNot Nothing) AndAlso (blob.GetType = GetType(Byte())) Then
			Return CType(blob, Byte())
		End If
		Return Nothing
	End Function

	Public ReadOnly Property SupportedTypes As IEnumerable(Of Type) Implements IBlobBinarySaver.SupportedTypes
		Get
			Return {GetType(Byte())}
		End Get
	End Property
End Class