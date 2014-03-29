Imports System.Drawing
Imports System.IO

Public Class BitmapStreamSaver
	Implements Blob.IBlobBinarySaver

	Public Function FromBinary(data As Byte(), blobType As Type) As Object Implements IBlobBinarySaver.FromBinary
		If (data IsNot Nothing) AndAlso (blobType = GetType(Bitmap)) Then
			Dim stream = New MemoryStream(data)
			Dim bmp = New Bitmap(stream)
			stream.Dispose()
			Return bmp
		End If
		Return Nothing
	End Function

	Public Function ToBinary(blob As Object) As Byte() Implements IBlobBinarySaver.ToBinary
		If (blob IsNot Nothing) AndAlso (blob.GetType = GetType(Bitmap)) Then
			Dim bmp = CType(blob, Bitmap)
			Dim stream = New MemoryStream
			bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg)
			Dim bytes(stream.Length) As Byte
			stream.Position = 0
			stream.Read(bytes, 0, stream.Length)
			stream.Dispose()
			Return bytes
		End If
		Return Nothing
	End Function

	Public ReadOnly Property SupportedTypes As IEnumerable(Of Type) Implements IBlobBinarySaver.SupportedTypes
		Get
			Return {GetType(Bitmap)}
		End Get
	End Property
End Class