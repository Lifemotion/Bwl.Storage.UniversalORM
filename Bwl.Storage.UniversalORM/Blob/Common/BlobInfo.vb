Imports System.IO
Imports Newtonsoft.Json

''' <summary>
''' Данные одного поля.
''' </summary>
''' <remarks></remarks>
Public Class BlobInfo
	Public Property BlobId As String
	Public Property FieldName As String
	Public Property FieldType As Type
	<JsonIgnore>
	Public Property Data As Byte()
End Class
