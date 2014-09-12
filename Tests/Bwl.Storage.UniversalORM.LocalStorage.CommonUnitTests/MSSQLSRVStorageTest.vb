Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Data.SqlClient
Imports System.IO
Imports Bwl.Storage.UniversalORM
Imports System.Drawing
Imports Bwl.Storage.UniversalORM.LocalStorage
Imports Bwl.Storage.UniversalORM.AdoDb

<TestClass()> Public Class MSSQLSRVStorageTest
	Private _conStrBuidMSSQLSRV As SqlConnectionStringBuilder
	Private _managerMSSQLSRV As MSSQLSRVStorageManager
	Private _storageMSSQLSRV As MSSQLSRVStorage

	Private _data1 As TestData
	Private _data2 As TestData
	Private _data3 As TestData
	Private _data4 As TestDataInternal
	Private _data5 As TestData2
	Private _data6 As TestData2

	<TestInitialize()>
	Public Sub Init()
		_conStrBuidMSSQLSRV = New SqlConnectionStringBuilder()
		_conStrBuidMSSQLSRV.InitialCatalog = "BigData1"
		_conStrBuidMSSQLSRV.UserID = "DrFenazepam-ПК\DrFenazepam" '"sa"
		_conStrBuidMSSQLSRV.Password = "" '"123"
		_conStrBuidMSSQLSRV.DataSource = "DRFENAZEPAM-ПК\SQLEXPRESS" '"(local)"
		_conStrBuidMSSQLSRV.IntegratedSecurity = True
		_conStrBuidMSSQLSRV.ConnectTimeout = 1
		_managerMSSQLSRV = New MSSQLSRVStorageManager(_conStrBuidMSSQLSRV)
		_managerMSSQLSRV.SetDbForType(GetType(TestData), "UnitTestData")
		_managerMSSQLSRV.SetDbForType(GetType(TestData2), "UnitTestData2")
		_storageMSSQLSRV = _managerMSSQLSRV.CreateStorage(Of TestData)("")


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

		_data4 = New TestDataInternal
		_data4.First = "first"
		_data4.ID = "444"
		_data4.Second = 11111

		_data5 = New TestData2
		_data5.F1 = "testdata21"
		_data5.ID = "555"
		_data5.F2 = 555

		_data6 = New TestData2
		_data6.F1 = "testdata22"
		_data6.ID = "666"
		_data6.F2 = 666
	End Sub

	<TestMethod()>
	Public Sub MSSQLSRVStorage_AddObj()
		Threading.Thread.Sleep(1000)
		_storageMSSQLSRV.RemoveAllObjects()
		Dim count1 = _storageMSSQLSRV.FindObjCount(Nothing)
		_storageMSSQLSRV.AddObj(_data1)
		_storageMSSQLSRV.AddObj(_data2)
		Dim count2 = _storageMSSQLSRV.FindObjCount(Nothing)

		'Assert.AreEqual(Convert.ToInt32(count1), 0)
		Assert.AreEqual(Convert.ToInt32(count2), 2)
	End Sub

	<TestMethod()>
	Public Sub MSSQLSRVStorage_RemoveAll()
		Dim count1 = _storageMSSQLSRV.FindObjCount(Nothing)
		_storageMSSQLSRV.RemoveAllObjects()
		Dim count2 = _storageMSSQLSRV.FindObjCount(Nothing)

		Assert.AreNotEqual(Convert.ToInt32(count1), 0)
		Assert.AreEqual(Convert.ToInt32(count2), 0)
	End Sub

	<TestMethod()>
	Public Sub MSSQLSRVStorage_FindObj()
		_storageMSSQLSRV.RemoveAllObjects()
		_storageMSSQLSRV.AddObj(_data1)
		_storageMSSQLSRV.AddObj(_data2)
		Dim count1 = _storageMSSQLSRV.FindObjCount(Nothing)
		Dim sp As New SearchParams()
		sp.SelectOptions = New SelectOptions(1)
		Dim objfind = _storageMSSQLSRV.FindObj(sp)(0).Split(" "c)(0)

		Assert.AreEqual(Convert.ToInt32(count1), 2)
		Assert.AreEqual(_data1.ID, objfind)
	End Sub

	<TestMethod()>
	Public Sub MSSQLSRVStorage_Contains()
		_storageMSSQLSRV.RemoveAllObjects()
		_storageMSSQLSRV.AddObj(_data1)
		_storageMSSQLSRV.AddObj(_data2)
		_storageMSSQLSRV.AddObj(_data3)
		Dim res1 = _storageMSSQLSRV.Contains(_data3.ID)
		Dim res2 = _storageMSSQLSRV.Contains(_data4.ID)

		Assert.AreEqual(res1, True)
		Assert.AreEqual(res2, False)
	End Sub

	<TestMethod()>
	Public Sub MSSQLSRVStorage_SearchParam()
		_storageMSSQLSRV.RemoveAllObjects()
		_storageMSSQLSRV.AddObj(_data1)
		_storageMSSQLSRV.AddObj(_data2)
		_storageMSSQLSRV.AddObj(_data3)
		Dim crit = {New FindCriteria("Cat", FindCondition.likeEqaul, "cat")}
		Dim sp = New SearchParams(crit)
		Dim selectOpt = New SelectOptions(10)
		sp = New SearchParams(selectOptions:=selectOpt)
		Dim res1 = _storageMSSQLSRV.FindObj(sp)

		Assert.AreEqual(res1.Length, 3)
	End Sub

	<TestMethod()>
	Public Sub MSSQLSRVStorage_GetObj()
		_storageMSSQLSRV.RemoveAllObjects()
		Dim obj1 = _storageMSSQLSRV.GetObj(_data3.ID)
		_storageMSSQLSRV.AddObj(_data1)
		_storageMSSQLSRV.AddObj(_data2)
		_storageMSSQLSRV.AddObj(_data3)
		Dim obj2 = _storageMSSQLSRV.GetObj(_data3.ID)

		Assert.AreEqual(obj1, Nothing)
		Assert.AreEqual(obj2.ID, _data3.ID)
	End Sub
End Class