Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Data.SqlClient
Imports System.IO
Imports Bwl.Storage.UniversalORM
Imports System.Drawing
Imports Bwl.Storage.UniversalORM.LocalStorage

<TestClass()> Public Class LocalStorageDBTest

	Private _localStorage As ILocalStorage
	Private _data1 As TestData
	Private _data2 As TestData
	Private _data3 As TestData
	Private _data4 As TestData
	Private _data5 As TestData
	Private _data6 As TestData

	<TestInitialize()>
	Public Sub Init()
		Dim conStrBld = New SqlConnectionStringBuilder()
		conStrBld.InitialCatalog = "TestDB1"
		conStrBld.UserID = "DrFenazepam-ПК\DrFenazepam"	'"sa"
		conStrBld.Password = ""	'"123"
		conStrBld.DataSource = "DRFENAZEPAM-ПК\SQLEXPRESS" ' "(local)"
		conStrBld.IntegratedSecurity = True
		conStrBld.ConnectTimeout = 1

		Dim fileSaverDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileData")

		Dim storageManager = New AdoDb.MSSQLSRVStorageManager(conStrBld)
		'Dim storageManager = New Files.FileStorageManager(fileSaverDir)

		Dim blobSaverDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BlobData")

		Dim blobFileSaver = New Blob.FileBlobSaver(blobSaverDir)

		'Dim localstorage = New LocalStorage(storageManager, New Blob.MemorySaver())
		_localStorage = New Bwl.Storage.UniversalORM.LocalStorage.LocalStorage(storageManager, blobFileSaver)

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


	<TestMethod()> Public Sub LocalStorageDB_RemoveAll()

		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.RemoveAllObj(GetType(TestDataInternal))

		Dim TestDataCount = _localStorage.FindObj(Of TestData)()
		Dim TestDataInternalCount = _localStorage.FindObj(Of TestDataInternal)()

		Assert.AreNotEqual(TestDataCount, Nothing)
		Assert.AreEqual(TestDataCount.Count, 0)
		Assert.AreNotEqual(TestDataInternalCount, Nothing)
		Assert.AreEqual(TestDataInternalCount.Count, 0)
	End Sub

	<TestMethod()> Public Sub LocalStorageDB_TestData_Add_FindById_GetObj()
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

		Dim sp = New SearchParams
		sp.FindCriterias = {New FindCriteria("id", FindCondition.eqaul, data.ID)}
		Dim ids = _localStorage.FindObj(Of TestData)(sp)

		Assert.AreNotEqual(ids, Nothing)
		Assert.AreEqual(ids.Count, 1)
		Assert.AreEqual(ids.First, data.ID)

		Dim ob = _localStorage.GetObj(Of TestData)(ids.First)

		Assert.AreNotEqual(ob, Nothing)
		Assert.AreEqual(ob.Cat, data.Cat)
		Assert.AreEqual(ob.Dog, data.Dog)
		Assert.AreEqual(ob.Image.Width, data.Image.Width)
		Assert.AreEqual(ob.Int.SomeData, Nothing)
		Assert.AreEqual(ob.Int.SomeBytes, Nothing)
		Assert.AreEqual(ob.Int.First, data.Int.First)
		Assert.AreEqual(ob.Int.Second, data.Int.Second)

		Dim obWithoutBlob = _localStorage.GetObj(Of TestData)(ids.First, False)

		Assert.AreNotEqual(obWithoutBlob, Nothing)
		Assert.AreEqual(obWithoutBlob.Cat, data.Cat)
		Assert.AreEqual(obWithoutBlob.Dog, data.Dog)
		Assert.AreEqual(obWithoutBlob.Image, Nothing)
		Assert.AreEqual(obWithoutBlob.Int.SomeData, Nothing)
		Assert.AreEqual(obWithoutBlob.Int.SomeBytes, Nothing)
		Assert.AreEqual(obWithoutBlob.Int.First, data.Int.First)
		Assert.AreEqual(obWithoutBlob.Int.Second, data.Int.Second)

	End Sub

	<TestMethod()> Public Sub LocalStorageDB_TestDataInternal_Add_FindById_GetObj()
		Dim data1 = New TestDataInternal
		data1.First = "234234234234"
		_localStorage.AddObj(data1)

		Dim data2 = New TestDataInternal
		data2.First = "111111"
		_localStorage.AddObj(data2)

		Dim sp = New SearchParams
		sp.FindCriterias = {New FindCriteria("id", FindCondition.eqaul, data1.ID)}
		Dim ids = _localStorage.FindObj(Of TestDataInternal)(sp)
		Assert.AreNotEqual(ids, Nothing)
		Assert.AreEqual(ids.Count, 1)
		Assert.AreEqual(ids.First, data1.ID)

		Dim ob = _localStorage.GetObj(Of TestDataInternal)(ids.First)

		Assert.AreNotEqual(ob, Nothing)
		Assert.AreEqual(ob.First, data1.First)
		Assert.AreEqual(ob.ID, data1.ID)
		Assert.AreEqual(ob.Second, data1.Second)
		Assert.AreEqual(ob.SomeBytes, data1.SomeBytes)
		Assert.AreEqual(ob.SomeData, data1.SomeData)

	End Sub

	<TestMethod()> Public Sub LocalStorageDB_FindBadObjById()
		Dim sp = New SearchParams
		sp.FindCriterias = {New FindCriteria("id", FindCondition.eqaul, "{C12FADA0-190F-4A11-B12E-CBA1C7DF0D03}")}
		Dim ids = _localStorage.FindObj(Of TestDataInternal)(sp)
		Assert.AreNotEqual(ids, Nothing)
		Assert.AreEqual(ids.Count, 0)
	End Sub

	<TestMethod()> Public Sub LocalStorageDB_GetBadObjById()
		Dim obj = _localStorage.GetObj(Of TestDataInternal)("{E2F5E178-8809-46FA-ACBC-A9F6D45A44F3}")
		Assert.AreEqual(obj, Nothing)
	End Sub

	<TestMethod()> Public Sub LocalStorageDB_ContainsBadObjById()
		Dim contains = _localStorage.Contains(Of TestDataInternal)("{FE61FF34-CA73-4E8D-9515-5C8D47859B73}")
		Assert.AreEqual(contains, False)
	End Sub

	<TestMethod()> Public Sub LocalStorageDB_ContainsGoodObjById()
		Dim data = New TestDataInternal
		data.First = "2222"
		_localStorage.AddObj(data)
		Dim contains = _localStorage.Contains(Of TestDataInternal)(data.ID)
		Assert.AreEqual(contains, True)
	End Sub

	<TestMethod()> Public Sub LocalStorageDB_ParamSort()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		Dim tmp = _localStorage.FindObj(Of TestData)(Nothing)
		Dim sp = New SearchParams()
		Dim sort = New SortParam("Timestamp", SortMode.Ascending)
		sp = New SearchParams(sortParam:=sort)
		Dim ids1 = _localStorage.FindObj(Of TestData)(sp)

		sort = New SortParam("Timestamp", SortMode.Descending)
		sp = New SearchParams(sortParam:=sort)
		Dim ids2 = _localStorage.FindObj(Of TestData)(sp)
		_localStorage.RemoveAllObj(GetType(TestData))
		Assert.AreEqual(tmp.Count, 2)
		Assert.AreEqual(tmp(0), ids1(0))
		Assert.AreEqual(tmp(0), ids2(1))
	End Sub

	<TestMethod()> Public Sub LocalStorageDB_GetDataInfo()
		Dim tempS = New ObjDataInfoGenerator()
		Dim pp = tempS.GetObjDataInfo(_data1)
		Dim f = pp.GetOneFileForWeb
		Dim ob_ttt = tempS.GetObject(ObjDataInfo.GetFromOneFile(f))
		Dim obttt = CType(ob_ttt, TestData)
		Assert.AreEqual(_data1.ID, obttt.ID)
	End Sub

	<TestMethod()> Public Sub LocalStorageDB_SelectOptions()
		_localStorage.RemoveAllObj(GetType(TestData))
		_localStorage.AddObj(_data1)
		_localStorage.AddObj(_data2)
		_localStorage.AddObj(_data3)
		_localStorage.AddObj(_data4)
		_localStorage.AddObj(_data5)
		_localStorage.AddObj(_data6)
		Dim crit = {New FindCriteria("Timestamp", FindCondition.less, DateTime.Now)}
		Dim sp = New SearchParams(crit)
		Dim selectOpt = New SelectOptions(10)
		sp = New SearchParams(selectOptions:=selectOpt)
		Dim ids1 = _localStorage.FindObj(Of TestData)(sp)

		sp = New SearchParams(selectOptions:=selectOpt)
		Dim ids2 = _localStorage.FindObj(Of TestData)(sp)

		selectOpt = New SelectOptions(2)
		Dim sort = New SortParam("Timestamp", SortMode.Descending)
		sp = New SearchParams(selectOptions:=selectOpt, sortParam:=sort)
		Dim ids3 = _localStorage.FindObj(Of TestData)(sp)

		Assert.AreEqual(ids1.Count, 6)
		Assert.AreEqual(ids2.Count, 6)
		Assert.AreEqual(ids3.Count, 2)
	End Sub

	<TestMethod()> Public Sub LocalStorageDB_RemoveObj()
		Dim crit = {New FindCriteria("Timestamp", FindCondition.notEqual, DateTime.Now)}
		Dim sp = New SearchParams(crit)
		Dim ids1 = _localStorage.FindObj(Of TestData)(sp)
		Dim count1 = _localStorage.FindObjCount(GetType(TestData))

		For Each objId In ids1
			_localStorage.RemoveObj(Of TestData)(objId)
		Next

		Dim count2 = _localStorage.FindObjCount(GetType(TestData))
		Assert.AreEqual(count1, Convert.ToInt64(6))
		Assert.AreEqual(count2, Convert.ToInt64(0))
	End Sub


End Class