Imports SkiaSharp

Public Class SKBitmapBinaryConverter
    Implements IBinaryConverter

    Public Function FromBinary(data As Byte(), blobType As Type) As Object Implements IBinaryConverter.FromBinary
        If (data IsNot Nothing) AndAlso (blobType = GetType(SKBitmap)) Then
            Return SKBitmap.Decode(data)
        End If
        Return Nothing
    End Function

    Public Function ToBinary(blob As Object) As Byte() Implements IBinaryConverter.ToBinary
        If (blob IsNot Nothing) AndAlso (blob.GetType = GetType(SKBitmap)) Then
            Dim bmp = CType(blob, SKBitmap)
            Return bmp.Encode(SKEncodedImageFormat.Jpeg, 95).AsSpan().ToArray()
        End If
        Return Nothing
    End Function

    Public ReadOnly Property SupportedTypes As IEnumerable(Of Type) Implements IBinaryConverter.SupportedTypes
        Get
            Return {GetType(SKBitmap)}
        End Get
    End Property
End Class