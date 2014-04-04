﻿Imports Newtonsoft.Json
Imports Bwl.Storage.UniversalORM.Blob

Public Class TestData
	Implements ObjBase

	<Indexing>
	Public Property Cat As String

	Public Property Dog As Integer

	<Indexing>
	Public Property Timestamp As DateTime

	<Indexing> <BlobContainer>
	Public Property Int As New TestDataInternal

	Public Property ID As String Implements ObjBase.ID

	<Blob> <JsonIgnore>
	Public Property Image As Bitmap
End Class

Public Class TestDataInternal
	Public Property First As String
	<Indexing> Public Property Second As Integer

	<JsonIgnore>
	Public Property SomeData As String

	<JsonIgnore> <Blob>
	Public Property SomeBytes As Byte()
End Class
