Imports System.IO

''' <summary>
''' Преобразователь BLOB поля в поток и обратно.
''' </summary>
''' <remarks></remarks>
Public Interface IBlobStreamSaver
	ReadOnly Property SupportedTypes As IEnumerable(Of Type)
	Sub ToStream(blob As Object, stream As Stream)
	Function FromStream(stream As Stream, blobType As Type)
End Interface
