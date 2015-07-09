Imports Newtonsoft.Json

Public Class FireBirdTestData
    Implements ObjBase
    '<Indexing> Public Property Cat As String = "fffg"
    'Public Property Dog As Integer = 4
    '<Indexing> Public Property Int As New TestDataInternal
    'Public Property ID As String = "fff" Implements ObjBase.ID
    '<Blob> <JsonIgnore>


    <Indexing>
    Public Property Cat As String

    Public Property Dog As Integer

    <Indexing>
    Public Property Timestamp As DateTime

    <Indexing> <BlobContainer>
    Public Property Int As New FireBirdTestDataInternal

    Public Property ID As String Implements ObjBase.ID

    <Blob> <JsonIgnore>
    Public Property Image As Bitmap
End Class

'Public Class TestDataInternal
'	Public Property First As String = "fffg"
'	<Indexing> Public Property Second As Integer = 4

'	<JsonIgnore>
'	Public Property SomeData As String
'End Class
Public Class FireBirdTestDataInternal
    Implements ObjBase
    Public Property First As String
    <Indexing> Public Property Second As Integer

    <JsonIgnore>
    Public Property SomeData As String

    <JsonIgnore>
    Public Property SomeBytes As Byte()
    Public Property ID As String Implements ObjBase.ID
End Class