Imports Newtonsoft.Json

Public Class BlobBigData
    Public Property BigText1 As String

    <JsonIgnore> <Blob>
    Public Property Bitmap As Bitmap

    <JsonIgnore> <Blob>
    Public Property VeryBigData As Byte()
End Class

Public Class BlobSomeData
    Public Property Text1 As String
    Public Property Text2 As String
    <JsonIgnore>
    Public Property Text3 As String

    <BlobContainer>
    Public Property Data As BlobBigData

    <BlobContainer>
    Public Property List As List(Of ObjContainer2)
End Class
