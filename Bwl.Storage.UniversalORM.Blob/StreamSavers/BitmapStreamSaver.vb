Imports System.Drawing

Public Class BitmapStreamSaver
	Implements Blob.IBlobStreamSaver

	Public Function FromStream(stream As IO.Stream, blobType As Type) As Object Implements IBlobStreamSaver.FromStream
		If (stream IsNot Nothing) AndAlso (blobType = GetType(Bitmap)) Then
			Dim bmp = New Bitmap(stream)
			Return bmp
		End If
		Return Nothing
	End Function

	Public Sub ToStream(blob As Object, stream As IO.Stream) Implements IBlobStreamSaver.ToStream
		If (blob IsNot Nothing) AndAlso (stream IsNot Nothing) AndAlso (blob.GetType = GetType(Bitmap)) Then
			Dim bmp = CType(blob, Bitmap)
			bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg)
		End If
	End Sub

	Public ReadOnly Property SupportedTypes As IEnumerable(Of Type) Implements IBlobStreamSaver.SupportedTypes
		Get
			Return {GetType(Bitmap)}
		End Get
	End Property
End Class