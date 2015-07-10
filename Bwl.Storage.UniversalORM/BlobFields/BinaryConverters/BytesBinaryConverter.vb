Imports System.Drawing

Public Class BytesBinaryConverter
    Implements IBinaryConverter

    Public Function FromBinary(data As Byte(), blobType As Type) As Object Implements IBinaryConverter.FromBinary
        Return data
    End Function

    Public Function ToBinary(blob As Object) As Byte() Implements IBinaryConverter.ToBinary
        If (blob IsNot Nothing) AndAlso (blob.GetType = GetType(Byte())) Then
            Return CType(blob, Byte())
        End If
        Return Nothing
    End Function

    Public ReadOnly Property SupportedTypes As IEnumerable(Of Type) Implements IBinaryConverter.SupportedTypes
        Get
            Return {GetType(Byte())}
        End Get
    End Property
End Class