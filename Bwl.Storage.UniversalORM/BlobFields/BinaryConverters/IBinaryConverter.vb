''' <summary>
''' Преобразователь типа данных в массив байтов и обратно.
''' </summary>
''' <remarks></remarks>
Public Interface IBinaryConverter
    ReadOnly Property SupportedTypes As IEnumerable(Of Type)
    Function ToBinary(blob As Object) As Byte()
    Function FromBinary(data As Byte(), blobType As Type) As Object
End Interface
