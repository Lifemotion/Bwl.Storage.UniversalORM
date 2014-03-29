Imports System.Drawing

Public Class BytesStreamSaver
	Implements Blob.IBlobStreamSaver

	Public Function FromStream(stream As IO.Stream, blobType As Type) As Object Implements IBlobStreamSaver.FromStream
		If (stream IsNot Nothing) AndAlso (blobType = GetType(Byte())) Then
			Dim bytes(stream.Length - 1) As Byte
			stream.Position = 0
			stream.Read(bytes, 0, stream.Length)
			Return bytes
		End If
		Return Nothing
	End Function

	Public Sub ToStream(blob As Object, stream As IO.Stream) Implements IBlobStreamSaver.ToStream
		If (blob IsNot Nothing) AndAlso (stream IsNot Nothing) AndAlso (blob.GetType = GetType(Byte())) Then
			Dim bytes = CType(blob, Byte())
			stream.Position = 0
			stream.Write(bytes, 0, bytes.Length)
		End If
	End Sub

	Public ReadOnly Property SupportedTypes As IEnumerable(Of Type) Implements IBlobStreamSaver.SupportedTypes
		Get
			Return {GetType(Byte())}
		End Get
	End Property
End Class