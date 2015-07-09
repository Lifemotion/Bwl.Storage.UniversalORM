Imports System.IO


Public Class BlobUORM
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim blobStorage = New CommonBlobStorage()

        blobStorage.AddSaver(New FileBlobSaver(Path.Combine(Application.StartupPath, "data")))

        Dim testData = New BlobSomeData
        testData.Text1 = "111111111111"
        testData.Text2 = "222222222222"
        testData.Text3 = "333333333333"
        testData.Data = New BlobBigData
        testData.Data.BigText1 = "44444444444"
        testData.Data.Bitmap = New Bitmap(1000, 1000)
        testData.Data.VeryBigData = {0, 1, 2, 3, 4, 5, 65, 44, 2, 3, 6, 34, 77, 24, 78, 35, 57, 78}

        testData.List = New List(Of ObjContainer2)



        Dim testData2 = New BlobSomeData
        testData2.Data = New BlobBigData
        testData2.Data.BigText1 = "2222"
        testData2.Data.Bitmap = New Bitmap(12, 13)
        testData2.Data.VeryBigData = {5, 6, 7, 8, 9, 0, 4, 3, 0}


        Dim objCont = New ObjContainer2
        objCont.Obj = testData2
        testData.List.Add(objCont)

        Dim id = Guid.NewGuid.ToString
        Dim json = CfJsonConverter.Serialize(testData)
        blobStorage.SaveBlobs(testData, id)



        Dim newData = CfJsonConverter.Deserialize(Of BlobSomeData)(json)
        blobStorage.LoadBlobs(newData, id)

        blobStorage.Remove(id)
        Dim newData2 = CfJsonConverter.Deserialize(Of BlobSomeData)(json)
        blobStorage.LoadBlobs(newData2, id)

    End Sub

    Private Sub BlobUORM_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
