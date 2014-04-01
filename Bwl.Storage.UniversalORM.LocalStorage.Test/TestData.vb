Imports Newtonsoft.Json
Imports Bwl.Storage.UniversalORM.Blob

Public Class TestData
	Implements ObjBase

	<Indexing>
	Public Property Cat As String = "fffg"

	Public Property Dog As Integer = 4

	<Indexing>
	Public Property Timestamp As DateTime = DateTime.Now

	<Indexing> <BlobContainer>
	Public Property Int As New TestDataInternal

	Public Property ID As String Implements ObjBase.ID

	<Blob> <JsonIgnore>
	Public Property Image As Bitmap = New Bitmap(100, 10)
End Class

Public Class TestDataInternal
	Public Property First As String = "fffg"
	<Indexing> Public Property Second As Integer = 4

	<JsonIgnore>
	Public Property SomeData As String

	<JsonIgnore> <Blob>
	Public Property SomeBytes As Byte() = {1, 2, 66, 88, 99, 3, 56, 32}
End Class
