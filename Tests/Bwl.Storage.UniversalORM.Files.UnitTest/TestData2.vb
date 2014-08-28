Imports Newtonsoft.Json
Imports Bwl.Storage.UniversalORM
Imports System.Drawing

Public Class TestData2
	Implements ObjBase

	<Indexing>
	Public Property F1 As String

	Public Property F2 As Integer

	<Indexing>
	Public Property Timestamp As DateTime

	<Indexing> <BlobContainer>
	Public Property Int As New TestDataInternal

	Public Property ID As String Implements ObjBase.ID

	<Blob> <JsonIgnore>
	Public Property Image As Bitmap
End Class
