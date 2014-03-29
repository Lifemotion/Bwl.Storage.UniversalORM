Imports System.IO


Public Class UORM
	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

		Dim blobStorage = New CommonBlobStorage()

		blobStorage.AddSaver(New FileBlobSaver(Path.Combine(Application.StartupPath, "data")))

		Dim testData = New SomeData
		testData.Text1 = "111111111111"
		testData.Text2 = "222222222222"
		testData.Text3 = "333333333333"
		testData.Data = New BigData
		testData.Data.BigText1 = "44444444444"
		testData.Data.Bitmap = New Bitmap(1000, 1000)
		testData.Data.VeryBigData = {0, 1, 2, 3, 4, 5, 65, 44, 2, 3, 6, 34, 77, 24, 78, 35, 57, 78}

		Dim id = Guid.NewGuid.ToString
		Dim json = JsonUtils.ToJson(testData)
		blobStorage.SaveBlobs(testData, id)



		Dim newData = JsonUtils.LoadFromJsonString(Of SomeData)(json)
		blobStorage.LoadBlobs(newData, id)

		blobStorage.Remove(id)
		Dim newData2 = JsonUtils.LoadFromJsonString(Of SomeData)(json)
		blobStorage.LoadBlobs(newData2, id)

	End Sub
End Class
