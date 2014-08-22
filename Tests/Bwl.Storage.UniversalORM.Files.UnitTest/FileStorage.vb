Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Windows.Forms
Imports System.Drawing

<TestClass()> Public Class FileStorage

	Private _stor As FileObjStorage
	Private _data1 As TestData
	Private _data2 As TestData
	Private _data3 As TestData
	Private _data4 As TestData
	Private _data5 As TestData
	Private _data6 As TestData

	<TestInitialize()>
	Public Sub Init()
		Dim path = "D:\CleverFlow\bwl.storage.universalorm\Tests\Bwl.Storage.UniversalORM.Files.UnitTest\data"
		Dim manager As New FileStorageManager(path)
		_stor = manager.CreateStorage("TestDataStor", GetType(TestData))

		_data1 = New TestData
		_data1.Cat = "cat11"
		_data1.ID = "111"
		_data1.Image = New Bitmap(100, 100)

		_data2 = New TestData
		_data2.Cat = "cat22"
		_data2.ID = "222"
		_data2.Image = New Bitmap(30, 47)

		_data3 = New TestData
		_data3.Cat = "cat33"
		_data3.ID = "333"
		_data3.Image = New Bitmap(67, 80)

		_data4 = New TestData
		_data4.Cat = "cat44"
		_data4.ID = "444"
		_data4.Image = New Bitmap(36, 33)

		_data5 = New TestData
		_data5.Cat = "cat55"
		_data5.ID = "555"
		_data5.Image = New Bitmap(100, 100)

		_data6 = New TestData
		_data6.Cat = "cat66"
		_data6.ID = "666"
		_data6.Image = New Bitmap(30, 47)
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


End Class