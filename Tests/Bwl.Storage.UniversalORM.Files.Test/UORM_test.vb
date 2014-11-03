Public Class UORM_test
	Private manager As New FileStorageManager(Application.StartupPath + "\..\data")
	Private stor As FileObjStorage = manager.CreateStorage("TestDataStor", GetType(TestData))
	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		Dim testData1 = New TestData
		testData1.Cat = "CAT1111111"
		testData1.Dog = 22222222
		testData1.ID = Guid.NewGuid.ToString("B")
		testData1.Int = New TestDataInternal
		testData1.Int.First = "11111111111111"
		testData1.Int.Second = "1111111" '"2222222"
		testData1.Int.SomeData = "bad data"

		Dim tdi = New TestData2()
		tdi.F1 = "tdi"
		tdi.F2 = 391
		tdi.ID = Guid.NewGuid.ToString("B")
		stor.AddObj(tdi)
		stor.AddObj(testData1)
		Dim allobj = stor.FindObj(Nothing)

		Dim findCrit = {New FindCriteria("Cat", FindCondition.eqaul, "CAT1111111")}	', New FindCriteria("Int.Second", FindCondition.eqaul, "1111111")}	'"2222222")}
		Dim sort = New SortParam("Int.Second", SortMode.Descending)
		Dim selop = New SelectOptions(5, 10)
		Dim searchP As New SearchParams(findCriteria:=findCrit, sortParam:=sort, selectOptions:=selop)


		Dim stopwatch As New Stopwatch()
		Dim timesp As New TimeSpan()
		stopwatch.Reset()
		stopwatch.Start()
		Dim res = stor.FindObj(searchP)
		stopwatch.Stop()
		timesp = stopwatch.Elapsed
		Dim elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000000}", timesp.Hours, timesp.Minutes, timesp.Seconds, timesp.Milliseconds * 1000)

		MessageBox.Show(String.Format("Время выполнения: {0} Количество элементов: {1}", elapsedTime.ToString(), res.Length))
		'stor.RemoveObj(testData1.ID)
		'stor.AddObjects({testData1, tdi})

		Dim newData = stor.GetObj(testData1.ID)


		'Dim f = stor.FindObj(Nothing)
		'      For Each ff In f
		'	stor.RemoveObj(ff)
		'      Next
	End Sub
End Class
