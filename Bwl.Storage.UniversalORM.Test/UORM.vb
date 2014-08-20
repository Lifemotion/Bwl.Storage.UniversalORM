Public Class UORM
    Private manager As New FileStorageManager(Application.StartupPath + "\..\data")
	Private stor As FileObjStorage = manager.CreateStorage("TestDataStor", GetType(TestData))
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		Dim testData1 = New TestData
		testData1.Cat = "CAT1111111"
		testData1.Dog = 22222222
		testData1.ID = Guid.NewGuid.ToString("B")
		testData1.Int = New TestDataInternal
		testData1.Int.First = "11111111111111"
		testData1.Int.Second = "2222222"
		testData1.Int.SomeData = "bad data"

		Dim tdi = New TestData2()
		tdi.F1 = "tdi"
		tdi.F2 = 391
		tdi.ID = Guid.NewGuid.ToString("B")
		'stor.AddObj(tdi)
		stor.AddObj(testData1)
		stor.RemoveObj(testData1.ID)
		'stor.AddObjects({testData1, tdi})

		Dim newData = stor.GetObj(testData1.ID)


		'Dim f = stor.FindObj(Nothing)
		'      For Each ff In f
		'	stor.RemoveObj(ff)
		'      Next
    End Sub
End Class
