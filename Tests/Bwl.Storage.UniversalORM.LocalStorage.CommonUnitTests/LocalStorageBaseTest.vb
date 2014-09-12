Imports Bwl.Storage.UniversalORM
Imports System.Threading
Imports System.Drawing
Imports Bwl.Storage.UniversalORM.LocalStorage

<TestClass()>
Public MustInherit Class LocalStorageBaseTest
	Private _localStorage As ILocalStorage

	Protected ReadOnly Property LocalStorage As ILocalStorage
		Get
			Return _localStorage
		End Get
	End Property

	Private _data1 As TestData
	Private _data2 As TestData
	Private _data3 As TestData
	Private _data4 As TestData
	Private _data5 As TestData
	Private _data6 As TestData

	Private _dataInt1 As TestDataInternal
	Private _dataInt2 As TestDataInternal

	<TestInitialize()>
	Public Sub Init()

		_localStorage = CreateLocalStorage()

		_data1 = New TestData
		_data1.Cat = "happycat"
		_data1.ID = "111"
		_data1.Image = New Bitmap(100, 100)

		'чтобы было отличие во времени создания
		Thread.Sleep(10)

		_data2 = New TestData
		_data2.Cat = "cat22"
		_data2.ID = "222"
		_data2.Image = New Bitmap(30, 47)

		'чтобы было отличие во времени создания
		Thread.Sleep(10)

		_data3 = New TestData
		_data3.Cat = "happycat"
		_data3.ID = "333"
		_data3.Image = New Bitmap(67, 80)

		'чтобы было отличие во времени создания
		Thread.Sleep(10)

		_data4 = New TestData
		_data4.Cat = "cat44"
		_data4.ID = "444"
		_data4.Image = New Bitmap(36, 33)

		'чтобы было отличие во времени создания
		Thread.Sleep(10)

		_data5 = New TestData
		_data5.Cat = "cat55"
		_data5.ID = "555"
		_data5.Image = New Bitmap(100, 100)

		'чтобы было отличие во времени создания
		Thread.Sleep(10)

		_data6 = New TestData
		_data6.Cat = "happycat"
		_data6.ID = "666"
		_data6.Image = New Bitmap(30, 47)

		'чтобы было отличие во времени создания
		Thread.Sleep(10)

		_dataInt1 = New TestDataInternal
		_dataInt1.First = "happycat111"

		'чтобы было отличие во времени создания
		Thread.Sleep(10)

		_dataInt2 = New TestDataInternal
		_dataInt2.First = "3rlosdf8"

		If SubInit() = False Then
			Throw New Exception()
		End If
	End Sub

	Protected MustOverride Function CreateLocalStorage() As ILocalStorage

	Protected Overridable Function SubInit() As Boolean
		Return True
	End Function

	Protected Sub RemoveAll()
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		Assert.AreEqual(p1, 0L)

		_localStorage.RemoveAllObj(GetType(TestDataInternal))
		Dim p2 = _localStorage.FindObjCount(GetType(TestDataInternal), Nothing)

		Dim TestDataCount = _localStorage.FindObj(Of TestData)()
		Assert.AreNotEqual(TestDataCount, Nothing)
		Assert.AreEqual(TestDataCount.Count, 0)

		Dim TestDataInternalCount = _localStorage.FindObj(Of TestDataInternal)()
		Assert.AreNotEqual(TestDataInternalCount, Nothing)
		Assert.AreEqual(TestDataInternalCount.Count, 0)

		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		_localStorage.AddObj(_data3)

		Dim p3 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		Assert.AreEqual(p3, 3L)

		Dim p4 = _localStorage.FindObjCount(GetType(TestDataInternal), Nothing)
		Assert.AreEqual(p4, 0L)

		_localStorage.RemoveAllObj(GetType(TestData))

		Dim p5 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		Assert.AreEqual(p5, 0L)
	End Sub

	Protected Sub AddObj()
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		_localStorage.AddObj(_data3)
		Dim p3 = _localStorage.FindObjCount(GetType(TestData), Nothing)

		Assert.AreEqual(p1, 0L)
		Assert.AreEqual(p2, 2L)
		Assert.AreEqual(p3, 3L)
	End Sub

	Protected Sub RemoveObj()
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		_localStorage.AddObj(_data1)
		Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		_localStorage.RemoveObj(Of TestData)(_data1.ID)
		Dim p3 = _localStorage.FindObjCount(GetType(TestData), Nothing)

		Assert.AreEqual(p1, 0L)
		Assert.AreEqual(p2, 1L)
		Assert.AreEqual(p3, 0L)
	End Sub

	Protected Sub GetObj()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		_localStorage.AddObj(_data3)

		Dim p1 = _localStorage.GetObj(Of TestData)(_data1.ID)
		Dim p2 = _localStorage.GetObj(Of TestData)(_data2.ID)
		Dim p3 = _localStorage.GetObj(Of TestData)(_data3.ID)

		Assert.AreEqual(p1.ID, _data1.ID)
		Assert.AreEqual(p2.ID, _data2.ID)
		Assert.AreEqual(p3.ID, _data3.ID)
	End Sub

	Protected Sub GetObjects()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		_localStorage.AddObj(_data3)

		Dim ids = New String() {_data1.ID, _data2.ID, _data3.ID}
		Dim p1 = _localStorage.GetObjects(Of TestData)(ids)
		Assert.AreEqual(p1.Count, 3)
		Assert.AreEqual(p1(0).ID, _data1.ID)
		Assert.AreEqual(p1(1).ID, _data2.ID)
		Assert.AreEqual(p1(2).ID, _data3.ID)
	End Sub

	Protected Sub FindObj_Criteria()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		_localStorage.AddObj(_data3)
		_localStorage.AddObj(_data4)
		_localStorage.AddObj(_data5)
		_localStorage.AddObj(_data6)

		Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.eqaul, "happycat")})
		Dim p1 = _localStorage.FindObj(Of TestData)(sp)
		Assert.AreEqual(p1.Count, 3)
	End Sub

	Protected Sub FindObj_SelectOption()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data2)
		_localStorage.AddObj(_data4)
		_localStorage.AddObj(_data5)

		Dim so = New SelectOptions(2)
		Dim sp = New SearchParams(selectOptions:=so)
		Dim p1 = _localStorage.FindObj(Of TestData)(sp)

		so = New SelectOptions(1, 2)
		sp = New SearchParams(selectOptions:=so)
		Dim p2 = _localStorage.FindObj(Of TestData)(sp)

		Assert.AreEqual(p1.Count, 2)
		Assert.AreEqual(p1(0), _data2.ID)
		Assert.AreEqual(p1(1), _data4.ID)
		Assert.AreEqual(p2.Count, 2)
		Assert.AreEqual(p2(0), _data4.ID)
		Assert.AreEqual(p2(1), _data5.ID)
	End Sub

	Protected Sub FindObj_timestamp_1()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)

		Dim dt = DateTime.Now

		_data2.Timestamp = dt
		_localStorage.AddObj(_data2)


		Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.eqaul, dt)})
		Dim p1 = _localStorage.FindObj(Of TestData)(sp)
		Assert.AreEqual(p1.Count, 1)
		Assert.AreEqual(p1.First, _data2.ID)
	End Sub

	Protected Sub FindObj_timestamp_2()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)
		Dim dt = DateTime.Now
		_data2.Timestamp = dt
		_localStorage.AddObj(_data2)

		Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.greater, DateTime.MinValue)})
		Dim p1 = _localStorage.FindObj(Of TestData)(sp)
		Assert.AreEqual(p1.Count, 2)
	End Sub

	Protected Sub FindObj_timestamp_3()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)

		Dim dt = DateTime.Now

		_data2.Timestamp = dt
		_localStorage.AddObj(_data2)

		Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.less, DateTime.MinValue)})
		Dim p1 = _localStorage.FindObj(Of TestData)(sp)
		Assert.AreEqual(p1.Count, 0)
	End Sub

	Protected Sub FindObj_timestamp_4()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)

		Dim dt = DateTime.Now

		_data2.Timestamp = dt
		_localStorage.AddObj(_data2)

		Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.less, DateTime.MaxValue)})
		Dim p1 = _localStorage.FindObj(Of TestData)(sp)
		Assert.AreEqual(p1.Count, 2)
	End Sub

	Protected Sub FindObj_timestamp_and_string()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)

		Dim dt = DateTime.Now

		_data2.Timestamp = dt
		_data2.Cat = "tttttttttttttttttttttt"
		_localStorage.AddObj(_data2)

		Dim sp = New SearchParams({
								  New FindCriteria("Timestamp", FindCondition.less, DateTime.MaxValue),
								  New FindCriteria("Cat", FindCondition.eqaul, _data2.Cat)
								  })
		Dim p1 = _localStorage.FindObj(Of TestData)(sp)
		Assert.AreEqual(p1.Count, 1)
		Assert.AreEqual(p1.First, _data2.ID)
	End Sub

	Protected Sub FindObj_SortParam()
		_localStorage.RemoveAllObj(GetType(TestData))

		_data1.Timestamp = DateTime.MinValue
		_data2.Timestamp = DateTime.Now
		_data3.Timestamp = DateTime.MaxValue

		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		_localStorage.AddObj(_data3)

		Dim so = New SelectOptions(2)
		Dim sp = New SearchParams(selectOptions:=so)
		Dim p1 = _localStorage.FindObj(Of TestData)(sp)

		Dim sortp = New SortParam("Timestamp", SortMode.Ascending)
		sp.SortParam = sortp
		Dim p2 = _localStorage.FindObj(Of TestData)(sp)

		sortp = New SortParam("Timestamp", SortMode.Descending)
		sp.SortParam = sortp
		Dim p3 = _localStorage.FindObj(Of TestData)(sp)

		Assert.AreEqual(p1.Count, 2)
		Assert.AreEqual(p1(0), _data1.ID)
		Assert.AreEqual(p1(1), _data2.ID)

		Assert.AreEqual(p2(0), _data1.ID)
		Assert.AreEqual(p2(1), _data2.ID)

		Assert.AreEqual(p3(0), _data3.ID)
		Assert.AreEqual(p3(1), _data2.ID)
	End Sub

	Protected Sub Contains()
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim p1 = _localStorage.Contains(Of TestData)(_data1.ID)
		_localStorage.AddObj(_data1)
		Dim p2 = _localStorage.Contains(Of TestData)(_data1.ID)

		Assert.AreEqual(p1, False)
		Assert.AreEqual(p2, True)
	End Sub

	Protected Sub FindObjBetween()
		Dim td1 = New TestData
		td1.Cat = "td"
		td1.Dog = 111
		td1.ID = "{00000000-0000-0000-0000-000000000000}"
		td1.Int = New TestDataInternal
		td1.Int.First = "1111"
		td1.Int.Second = 1112
		td1.Int.SomeData = "bad data"
		td1.Int.ID = "{000}"

		Dim td2 = New TestData
		td2.Cat = "td"
		td2.Dog = 111
		td2.ID = "{11111111-1111-1111-1111-111111111111}"
		td2.Int = New TestDataInternal
		td2.Int.First = "2221"
		td2.Int.Second = 2222
		td2.Int.SomeData = "bad data"
		td2.Int.ID = "{111}"

		Dim td3 = New TestData
		td3.Cat = "td"
		td3.Dog = 111
		td3.ID = "{22222222-2222-2222-2222-222222222222}"
		td3.Int = New TestDataInternal
		td3.Int.First = "3331"
		td3.Int.Second = 3332
		td3.Int.SomeData = "bad data"
		td3.Int.ID = "{222}"

		Dim td4 = New TestData
		td4.Cat = "td"
		td4.Dog = 111
		td4.ID = "{33333333-3333-3333-3333-333333333333}"
		td4.Int = New TestDataInternal
		td4.Int.First = "4441"
		td4.Int.Second = 4442
		td4.Int.SomeData = "bad data"
		td4.Int.ID = "{333}"

		Dim td5 = New TestData
		td5.Cat = "td"
		td5.Dog = 111
		td5.ID = "{44444444-4444-4444-4444-444444444444}"
		td5.Int = New TestDataInternal
		td5.Int.First = "5551"
		td5.Int.Second = 5552
		td5.Int.SomeData = "bad data"
		td5.Int.ID = "{444}"
		Dim massAdd As TestData()
		massAdd = {td1, td2, td3, td4, td5}

		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.RemoveAllObj(GetType(TestDataInternal))

		_localStorage.AddObjects(massAdd)

		Dim spadd As New SearchParams({New FindCriteria("Cat", FindCondition.eqaul, "td")})
		spadd.SelectOptions = New SelectOptions(0, 3)
		Dim F1 = _localStorage.FindObj(Of TestData)(spadd)
		Assert.AreEqual(4, F1.Count)

		spadd.SelectOptions = New SelectOptions(0, 2)
		Dim F2 = _localStorage.FindObj(Of TestData)(spadd)
		Assert.AreEqual(3, F2.Count)
	End Sub

	Protected Sub TestData_Add_GetObj()
		_localStorage.RemoveAllObj(GetType(TestData))

		Dim data = New TestData()
		data.Cat = "111111"
		data.Dog = "222222"
		data.Image = New Bitmap(33, 44)
		data.Int.SomeBytes = {1, 1, 1, 2, 2, 3, 3, 3, 44, 55, 23}
		data.Int.First = "44444444"
		data.Int.Second = "555555555"
		data.Int.SomeData = "999999999999"
		_localStorage.AddObj(data)

		Dim data2 = New TestData
		_localStorage.AddObj(data2)

		Dim ob = _localStorage.GetObj(Of TestData)(data.ID)

		Assert.AreNotEqual(ob, Nothing)
		Assert.AreEqual(ob.Cat, data.Cat)
		Assert.AreEqual(ob.Dog, data.Dog)
		Assert.AreEqual(ob.Image.Width, data.Image.Width)
		Assert.AreEqual(ob.Int.SomeData, Nothing)
		Assert.AreEqual(ob.Int.SomeBytes, Nothing)
		Assert.AreEqual(ob.Int.First, data.Int.First)
		Assert.AreEqual(ob.Int.Second, data.Int.Second)

		Dim obWithoutBlob = _localStorage.GetObj(Of TestData)(data.ID, False)

		Assert.AreNotEqual(obWithoutBlob, Nothing)
		Assert.AreEqual(obWithoutBlob.Cat, data.Cat)
		Assert.AreEqual(obWithoutBlob.Dog, data.Dog)
		Assert.AreEqual(obWithoutBlob.Image, Nothing)
		Assert.AreEqual(obWithoutBlob.Int.SomeData, Nothing)
		Assert.AreEqual(obWithoutBlob.Int.SomeBytes, Nothing)
		Assert.AreEqual(obWithoutBlob.Int.First, data.Int.First)
		Assert.AreEqual(obWithoutBlob.Int.Second, data.Int.Second)
	End Sub

	Protected Sub GetBadObjById()
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim id = Guid.NewGuid.ToString("B")
		Dim obj = _localStorage.GetObj(Of TestData)(id)
		Assert.AreEqual(obj, Nothing)
	End Sub

	Protected Sub FindByBadField()
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim sp = New SearchParams
		sp.FindCriterias = {New FindCriteria("Fffiieiwefjolijef", FindCondition.eqaul, "edfsdf")}

		Dim exc As Exception = Nothing
		Try
			Dim ids = _localStorage.FindObj(Of TestDataInternal)(sp)
		Catch ex As Exception
			exc = ex
		End Try
		Assert.AreNotEqual(exc, Nothing)
	End Sub

	Protected Sub ContainsBadObjById()
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim contains = _localStorage.Contains(Of TestData)("{FE61FF34-CA73-4E8D-9515-5C8D47859B73}")
		Assert.AreEqual(contains, False)
	End Sub


	''' <summary>
	''' Надо вынести отдельно
	''' </summary>
	''' <remarks></remarks>
	<TestMethod()> Public Sub FirebirdLocalStorageDB_GetDataInfo()
		Dim tempS = New ObjDataInfoGenerator()
		Dim pp = tempS.GetObjDataInfo(_data1)
		Dim f = pp.GetOneFileForWeb
		Dim ob_ttt = tempS.GetObject(ObjDataInfo.GetFromOneFile(f))
		Dim obttt = CType(ob_ttt, TestData)
		Assert.AreEqual(_data1.ID, obttt.ID)
	End Sub
End Class
