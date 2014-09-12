Imports Bwl.Storage.UniversalORM
Imports System.Threading
Imports System.Drawing

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
		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		_localStorage.AddObj(_data3)
		Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim p3 = _localStorage.FindObjCount(GetType(TestData), Nothing)

		Assert.AreEqual(Convert.ToInt32(p1), 0)
		Assert.AreEqual(Convert.ToInt32(p2), 3)
		Assert.AreEqual(Convert.ToInt32(p3), 0)
	End Sub

	Protected Sub AddObj()
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		_localStorage.AddObj(_data3)
		Dim p3 = _localStorage.FindObjCount(GetType(TestData), Nothing)

		Assert.AreEqual(Convert.ToInt32(p1), 0)
		Assert.AreEqual(Convert.ToInt32(p2), 2)
		Assert.AreEqual(Convert.ToInt32(p3), 3)
	End Sub

	Protected Sub RemoveObj()
		_localStorage.RemoveAllObj(GetType(TestData))
		Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		_localStorage.AddObj(_data1)
		Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)
		_localStorage.RemoveObj(Of TestData)(_data1.ID)
		Dim p3 = _localStorage.FindObjCount(GetType(TestData), Nothing)

		Assert.AreEqual(Convert.ToInt32(p1), 0)
		Assert.AreEqual(Convert.ToInt32(p2), 1)
		Assert.AreEqual(Convert.ToInt32(p3), 0)
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
End Class
