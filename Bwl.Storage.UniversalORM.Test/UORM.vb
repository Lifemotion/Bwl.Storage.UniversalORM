Public Class UORM
    Private manager As New FileStorageManager(Application.StartupPath + "\..\data")
    Private stor As FileObjStorage(Of TestData) = manager.CreateStorage(Of TestData)("TestDataStor")
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		Dim testData1 = New TestData
		testData1.Cat = "CAT1111111"
		testData1.Dog = 22222222
		testData1.ID = Guid.NewGuid.ToString("B")
		testData1.Int = New TestDataInternal
		testData1.Int.First = "11111111111111"
		testData1.Int.Second = "2222222"
		testData1.Int.SomeData = "bad data"

		stor.AddObj(testData1)

		Dim newData = stor.GetObj(testData1.ID)

        Dim f = stor.FindObj({})
        For Each ff In f
			stor.RemoveObj(ff)
        Next
    End Sub
End Class
