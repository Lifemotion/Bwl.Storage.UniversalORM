Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Threading

<TestClass()> Public Class FileStorageTest

	Private _stor As FileObjStorage
	Private _data1 As TestData
	Private _data2 As TestData
	Private _data3 As TestData
	Private _data4 As TestData
	Private _data5 As TestData
	Private _data6 As TestData

	<TestInitialize()>
	Public Sub Init()
		Dim path = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\data")
		Dim manager As New FileStorageManager(path)
		_stor = manager.CreateStorage("TestDataStor", GetType(TestData))

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
	End Sub

	<TestMethod()>
	Public Sub FileStorage_RemoveAll()
		_stor.RemoveAllObjects()
		Dim p1 = _stor.FindObjCount(Nothing)
		_stor.AddObj(_data1)
		_stor.AddObj(_data2)
		_stor.AddObj(_data3)
		Dim p2 = _stor.FindObjCount(Nothing)
		_stor.RemoveAllObjects()
		Dim p3 = _stor.FindObjCount(Nothing)

		Assert.AreEqual(Convert.ToInt32(p1), 0)
		Assert.AreEqual(Convert.ToInt32(p2), 3)
		Assert.AreEqual(Convert.ToInt32(p3), 0)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_AddObj()
		_stor.RemoveAllObjects()
		Dim p1 = _stor.FindObjCount(Nothing)
		_stor.AddObj(_data1)
		_stor.AddObj(_data2)
		Dim p2 = _stor.FindObjCount(Nothing)
		_stor.AddObj(_data3)
		Dim p3 = _stor.FindObjCount(Nothing)

		Assert.AreEqual(Convert.ToInt32(p1), 0)
		Assert.AreEqual(Convert.ToInt32(p2), 2)
		Assert.AreEqual(Convert.ToInt32(p3), 3)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_RemoveObj()
		_stor.RemoveAllObjects()
		Dim p1 = _stor.FindObjCount(Nothing)
		_stor.AddObj(_data1)
		Dim p2 = _stor.FindObjCount(Nothing)
		_stor.RemoveObj(_data1.ID)
		Dim p3 = _stor.FindObjCount(Nothing)

		Assert.AreEqual(Convert.ToInt32(p1), 0)
		Assert.AreEqual(Convert.ToInt32(p2), 1)
		Assert.AreEqual(Convert.ToInt32(p3), 0)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_GetObj()
		_stor.RemoveAllObjects()
		_stor.AddObj(_data1)
		_stor.AddObj(_data2)
		_stor.AddObj(_data3)

		Dim p1 = _stor.GetObj(_data1.ID)
		Dim p2 = _stor.GetObj(_data2.ID)
		Dim p3 = _stor.GetObj(_data3.ID)

		Assert.AreEqual(p1.ID, _data1.ID)
		Assert.AreEqual(p2.ID, _data2.ID)
		Assert.AreEqual(p3.ID, _data3.ID)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_GetObjects()
		_stor.RemoveAllObjects()
		_stor.AddObj(_data1)
		_stor.AddObj(_data2)
		_stor.AddObj(_data3)

		Dim ids = New String() {_data1.ID, _data2.ID, _data3.ID}
		Dim p1 = _stor.GetObjects(ids)
		Assert.AreEqual(p1.Count, 3)
		Assert.AreEqual(p1(0).ID, _data1.ID)
		Assert.AreEqual(p1(1).ID, _data2.ID)
		Assert.AreEqual(p1(2).ID, _data3.ID)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_Criteria()
		_stor.RemoveAllObjects()
		_stor.AddObj(_data1)
		_stor.AddObj(_data2)
		_stor.AddObj(_data3)
		_stor.AddObj(_data4)
		_stor.AddObj(_data5)
		_stor.AddObj(_data6)

		Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.eqaul, "happycat")})
		Dim p1 = _stor.FindObj(sp)
		Assert.AreEqual(p1.Count, 3)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_SelectOption()
		_stor.RemoveAllObjects()
		_stor.AddObj(_data2)
		_stor.AddObj(_data4)
		_stor.AddObj(_data5)

		Dim so = New SelectOptions(2)
		Dim sp = New SearchParams(selectOptions:=so)
		Dim p1 = _stor.FindObj(sp)

		so = New SelectOptions(1, 2)
		sp = New SearchParams(selectOptions:=so)
		Dim p2 = _stor.FindObj(sp)

		Assert.AreEqual(p1.Count, 2)
		Assert.AreEqual(p1(0), _data2.ID)
		Assert.AreEqual(p1(1), _data4.ID)
		Assert.AreEqual(p2.Count, 2)
		Assert.AreEqual(p2(0), _data4.ID)
		Assert.AreEqual(p2(1), _data5.ID)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_1()
		_stor.RemoveAllObjects()
		_stor.AddObj(_data1)

		Dim dt = DateTime.Now

		_data2.Timestamp = dt
		_stor.AddObj(_data2)


		Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.eqaul, dt)})
		Dim p1 = _stor.FindObj(sp)
		Assert.AreEqual(p1.Count, 1)
		Assert.AreEqual(p1.First, _data2.ID)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_2()
		_stor.RemoveAllObjects()
		_stor.AddObj(_data1)
		Dim dt = DateTime.Now
		_data2.Timestamp = dt
		_stor.AddObj(_data2)

		Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.greater, DateTime.MinValue)})
		Dim p1 = _stor.FindObj(sp)
		Assert.AreEqual(p1.Count, 2)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_3()
		_stor.RemoveAllObjects()
		_stor.AddObj(_data1)

		Dim dt = DateTime.Now

		_data2.Timestamp = dt
		_stor.AddObj(_data2)

		Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.less, DateTime.MinValue)})
		Dim p1 = _stor.FindObj(sp)
		Assert.AreEqual(p1.Count, 0)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_4()
		_stor.RemoveAllObjects()
		_stor.AddObj(_data1)

		Dim dt = DateTime.Now

		_data2.Timestamp = dt
		_stor.AddObj(_data2)

		Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.less, DateTime.MaxValue)})
		Dim p1 = _stor.FindObj(sp)
		Assert.AreEqual(p1.Count, 2)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_and_string()
		_stor.RemoveAllObjects()
		_stor.AddObj(_data1)

		Dim dt = DateTime.Now

		_data2.Timestamp = dt
		_data2.Cat = "tttttttttttttttttttttt"
		_stor.AddObj(_data2)

		Dim sp = New SearchParams({
								  New FindCriteria("Timestamp", FindCondition.less, DateTime.MaxValue),
								  New FindCriteria("Cat", FindCondition.eqaul, _data2.Cat)
								  })
		Dim p1 = _stor.FindObj(sp)
		Assert.AreEqual(p1.Count, 1)
		Assert.AreEqual(p1.First, _data2.ID)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_SortParam()
		_stor.RemoveAllObjects()

		_data1.Timestamp = DateTime.MinValue
		_data2.Timestamp = DateTime.Now
		_data3.Timestamp = DateTime.MaxValue

		_stor.AddObj(_data1)
		_stor.AddObj(_data2)
		_stor.AddObj(_data3)

		Dim so = New SelectOptions(2)
		Dim sp = New SearchParams(selectOptions:=so)
		Dim p1 = _stor.FindObj(sp)

		Dim sortp = New SortParam("Timestamp", SortMode.Ascending)
		sp.SortParam = sortp
		Dim p2 = _stor.FindObj(sp)

		sortp = New SortParam("Timestamp", SortMode.Descending)
		sp.SortParam = sortp
		Dim p3 = _stor.FindObj(sp)

		Assert.AreEqual(p1.Count, 2)
		Assert.AreEqual(p1(0), _data1.ID)
		Assert.AreEqual(p1(1), _data2.ID)

		Assert.AreEqual(p2(0), _data1.ID)
		Assert.AreEqual(p2(1), _data2.ID)

		Assert.AreEqual(p3(0), _data3.ID)
		Assert.AreEqual(p3(1), _data2.ID)
	End Sub

	<TestMethod()>
	Public Sub FileStorage_Contains()
		_stor.RemoveAllObjects()
		Dim p1 = _stor.Contains(_data1.ID)
		_stor.AddObj(_data1)
		Dim p2 = _stor.Contains(_data1.ID)

		Assert.AreEqual(p1, False)
		Assert.AreEqual(p2, True)
	End Sub
End Class