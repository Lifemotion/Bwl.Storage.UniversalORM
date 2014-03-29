Imports System.IO

''' <summary>
''' Преобразователь BLOB поля в поток и обратно.
''' </summary>
''' <remarks></remarks>
Public Interface IBlobBinarySaver
	ReadOnly Property SupportedTypes As IEnumerable(Of Type)
	Function ToBinary(blob As Object) As Byte()
	Function FromBinary(data As Byte(), blobType As Type) As Object
End Interface
