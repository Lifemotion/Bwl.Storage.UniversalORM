Imports System.Drawing
Imports System.IO

Public Class BitmapBinaryConverter
    Implements IBinaryConverter

    Public Function FromBinary(data As Byte(), blobType As Type) As Object Implements IBinaryConverter.FromBinary
        If (data IsNot Nothing) AndAlso (blobType = GetType(Bitmap)) Then
            Dim stream = New MemoryStream(data)
            Dim bmp = Bitmap.FromStream(stream, True, True)
            'stream.Dispose()
            Return bmp
        End If
        Return Nothing
    End Function

    Public Function ToBinary(blob As Object) As Byte() Implements IBinaryConverter.ToBinary
        If (blob IsNot Nothing) AndAlso (blob.GetType = GetType(Bitmap)) Then
            Dim bmp = CType(blob, Bitmap)
            Dim stream = New MemoryStream
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg)
            Dim len = Convert.ToInt32(stream.Length)
            Dim bytes(len - 1) As Byte
            stream.Position = 0
            stream.Read(bytes, 0, len)
            stream.Dispose()
            Return bytes
        End If
        Return Nothing
    End Function

    Public ReadOnly Property SupportedTypes As IEnumerable(Of Type) Implements IBinaryConverter.SupportedTypes
        Get
            Return {GetType(Bitmap)}
        End Get
    End Property
End Class