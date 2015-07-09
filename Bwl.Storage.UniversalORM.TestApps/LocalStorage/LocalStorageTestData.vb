Imports Newtonsoft.Json

Public Class LocalStorageTestData
    Implements ObjBase

    <Indexing>
    Public Property Cat As String

    Public Property Dog As Integer

    <Indexing>
    Public Property Timestamp As DateTime = DateTime.Now

    <Indexing> <BlobContainer>
    Public Property Int As New LocalStorageTestDataInternal

    Public Property ID As String Implements ObjBase.ID

    <Blob> <JsonIgnore>
    Public Property Image As Bitmap
End Class

Public Class LocalStorageTestDataInternal
    Implements ObjBase


    Public Property First As String
    <Indexing> Public Property Second As Integer

    <JsonIgnore>
    Public Property SomeData As String

    <JsonIgnore>
    Public Property SomeBytes As Byte()


    Public Property ID As String Implements ObjBase.ID
End Class
