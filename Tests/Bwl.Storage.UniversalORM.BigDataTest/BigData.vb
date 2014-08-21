Imports Newtonsoft.Json
Imports Bwl.Storage.UniversalORM

Public Class BigData
	Implements ObjBase

	Public Property ID As String Implements ObjBase.ID
	<Indexing> Public Property iText As String
	Public Property Text As String
	<Indexing> Public Property iTimestamp As DateTime
	<Indexing> Public Property iInteger As Integer
	<Indexing> Public Property iFloat As Double
	<Indexing> <BlobContainer> Public Property iData As New Data
	Public Property Time As DateTime
	Public Property Float As Double
	<Blob> <JsonIgnore> Public Property Image As Bitmap
End Class

Public Class Data
	Implements ObjBase
	Public Property TextData As String
	<Indexing> Public Property iIntData As Integer

	<JsonIgnore>
	Public Property SomeBytesData As Byte()
	Public Property IDData As String Implements ObjBase.ID
End Class