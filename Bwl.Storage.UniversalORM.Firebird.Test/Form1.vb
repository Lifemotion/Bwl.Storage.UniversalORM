'Imports System.Data.SqlClient
Imports FirebirdSql.Data.FirebirdClient
Imports Bwl.Storage.UniversalORM.Firebird
Imports System.IO
'Imports Bwl.Storage.UniversalORM
Public Class Form1

	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		Dim conStrBld = New FbConnectionStringBuilder()
		'conStrBld.InitialCatalog = "TestDB"
		conStrBld.Database = "D:\CleverFlow\Bwl.storage.UniversalORM\bwl.storage.universalorm\Bwl.Storage.UniversalORM.Firebird.Test\data\TestDB_Tst.fdb"
		conStrBld.UserID = "sysdba"
		conStrBld.Password = "masterkey"
		'conStrBld.UserID = "123"
		'conStrBld.Password = "123"
		conStrBld.Dialect = 3
		'conStrBld.DataSource = "localhost"
		conStrBld.ServerType = FbServerType.Embedded
		'conStrBld.Charset = FbCharset.Utf8.ToString()
		'conStrBld.IntegratedSecurity = True
		conStrBld.ConnectionTimeout = 1
		conStrBld.ClientLibrary = "D:\CleverFlow\Bwl.storage.UniversalORM\bwl.storage.universalorm\refs\fbe32\fbembed.dll"
		'conStrBld.ClientLibrary = ""
		'If Not IO.File.Exists(conStrBld.ClientLibrary) Then
		'	MessageBox.Show("DLL не найдена")
		'End If

		'Dim manager = New MSSQLSRVStorageManager(conStrBld)
		Try
			'------------------
			Dim _localStorage As ILocalStorage
			Dim fileSaverDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileData")
			Dim storageManager = New Firebird.FbStorageManager(conStrBld)
			Dim blobSaverDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BlobData")
			Dim blobFileSaver = New Blob.FileBlobSaver(blobSaverDir)
			_localStorage = New Bwl.Storage.UniversalORM.LocalStorage.LocalStorage(storageManager, blobFileSaver)

			Dim data = New TestDataInternal
			data.First = "2222"
			data.Second = 2222
			_localStorage.AddObj(data)
			Dim contains = _localStorage.Contains(data.ID, GetType(TestDataInternal))
			'--
			Dim spt = New SearchParams
			spt.FindCriterias = {New FindCriteria("Second", FindCondition.eqaul, 123)}
			Dim ids = _localStorage.FindObj(Of TestDataInternal)(spt)

			'--
			Dim _data1 As New TestData
			_data1.Cat = "cat11"
			_data1.ID = "111"
			_data1.Image = New Bitmap(100, 100)

			Dim _data2 As New TestData
			_data2.Cat = "cat22"
			_data2.ID = "222"
			_data2.Image = New Bitmap(30, 47)

			_localStorage.RemoveAllObj(GetType(TestData))
			_localStorage.AddObj(_data1)
			_localStorage.AddObj(_data2)
			Dim ftmp = _localStorage.FindObj(Nothing)
			Dim sps = New SearchParams()
			Dim sort = New SortParam("Timestamp", SortMode.Ascending)
			sps = New SearchParams(sortParam:=sort)
			Dim ids1 = _localStorage.FindObj(Of TestData)(sps)

			sort = New SortParam("Timestamp", SortMode.Descending)
			sps = New SearchParams(sortParam:=sort)
			Dim ids2 = _localStorage.FindObj(Of TestData)(sps)
			_localStorage.RemoveAllObj(GetType(TestData))
			'--
			Dim _data3 As New TestData
			_data3.Cat = "cat33"
			_data3.ID = "333"
			_data3.Image = New Bitmap(67, 80)

			Dim _data4 As New TestData
			_data4.Cat = "cat44"
			_data4.ID = "444"
			_data4.Image = New Bitmap(36, 33)

			Dim _data5 As New TestData
			_data5.Cat = "cat55"
			_data5.ID = "555"
			_data5.Image = New Bitmap(100, 100)

			Dim _data6 As New TestData
			_data6.Cat = "cat66"
			_data6.ID = "666"
			_data6.Image = New Bitmap(30, 47)

			_localStorage.RemoveAllObj(GetType(TestData))
			_localStorage.AddObj(_data1)
			_localStorage.AddObj(_data2)
			_localStorage.AddObj(_data3)
			_localStorage.AddObj(_data4)
			_localStorage.AddObj(_data5)
			_localStorage.AddObj(_data6)
			Dim crit = {New FindCriteria("Timestamp", FindCondition.less, DateTime.Now)}
			Dim spo = New SearchParams(crit)
			Dim selectOpt = New SelectOptions(10)
			spo = New SearchParams(selectOptions:=selectOpt)
			Dim ids1o = _localStorage.FindObj(Of TestData)(spo)

			selectOpt = New SelectOptions(6)
			spo = New SearchParams(selectOptions:=selectOpt)
			Dim ids2o = _localStorage.FindObj(Of TestData)(spo)

			selectOpt = New SelectOptions(2)
			Dim sorto = New SortParam("Timestamp", SortMode.Descending)
			spo = New SearchParams(selectOptions:=selectOpt, sortParam:=sorto)
			Dim ids3o = _localStorage.FindObj(Of TestData)(spo)
			'---
			_localStorage.RemoveAllObj(GetType(TestData))
			_localStorage.AddObj(_data1)
			_localStorage.AddObj(_data2)
			Dim tmpsp = _localStorage.FindObj(Nothing)
			Dim spsp = New SearchParams()
			sort = New SortParam("Cat", SortMode.Ascending)
			spsp = New SearchParams(sortParam:=sort)
			ids1 = _localStorage.FindObj(Of TestData)(spsp)

			sort = New SortParam("Cat", SortMode.Descending)
			spsp = New SearchParams(sortParam:=sort)
			ids2 = _localStorage.FindObj(Of TestData)(spsp)
			_localStorage.RemoveAllObj(GetType(TestData))

			'----------------------
			Dim tmpStorage As New FBStorage(conStrBld, GetType(TestData), "TestDB")

			Dim manager As FbStorageManager
			Try
				manager = New FbStorageManager(conStrBld)
			Catch ex As Exception
				MessageBox.Show("Error: " + ex.Message)
			End Try
			Dim storage As CommonObjStorage

			Dim testData1 = New TestData
			testData1.Cat = "CAT111111"
			testData1.Dog = 22222222
			testData1.ID = Guid.NewGuid.ToString("B")
			testData1.Int = New TestDataInternal
			testData1.Int.First = "11111111111111"
			testData1.Int.Second = "2222222"
			testData1.Int.SomeData = "bad data"

			Dim testData2 = New TestData
			testData2.Cat = "CAT2"
			testData2.Dog = 22222222
			testData2.ID = Guid.NewGuid.ToString("B")
			testData2.Int = New TestDataInternal
			testData2.Int.First = "11111111111111"
			testData2.Int.Second = "4444"
			testData2.Int.SomeData = "bad data"


			storage = manager.CreateStorage(Of TestData)("TestDataStorage")
			storage.RemoveAllObjects()

			Dim td1 = New TestData
			td1.Cat = "td1"
			td1.Dog = 111
			td1.ID = "{00000000-0000-0000-0000-000000000000}"
			td1.Int = New TestDataInternal
			td1.Int.First = "1111"
			td1.Int.Second = 1112
			td1.Int.SomeData = "bad data"
			td1.Int.ID = "{000}"

			Dim td2 = New TestData
			td2.Cat = "td2"
			td2.Dog = 111
			td2.ID = "{44444444-4444-4444-4444-444444444444}"
			td2.Int = New TestDataInternal
			td2.Int.First = "2221"
			td2.Int.Second = 2222
			td2.Int.SomeData = "bad data"
			td2.Int.ID = "{111}"

			Dim td3 = New TestData
			td3.Cat = "td3"
			td3.Dog = 111
			td3.ID = "{22222222-2222-2222-2222-222222222222}"
			td3.Int = New TestDataInternal
			td3.Int.First = "3331"
			td3.Int.Second = 3332
			td3.Int.SomeData = "bad data"
			td3.Int.ID = "{222}"

			Dim td4 = New TestData
			td4.Cat = "td4"
			td4.Dog = 111
			td4.ID = "{33333333-3333-3333-3333-333333333333}"
			td4.Int = New TestDataInternal
			td4.Int.First = "4441"
			td4.Int.Second = 4442
			td4.Int.SomeData = "bad data"
			td4.Int.ID = "{333}"

			Dim td5 = New TestData
			td5.Cat = "td5"
			td5.Dog = 111
			td5.ID = "{11111111-1111-1111-1111-111111111111}"
			td5.Int = New TestDataInternal
			td5.Int.First = "5551"
			td5.Int.Second = 5552
			td5.Int.SomeData = "bad data"
			td5.Int.ID = "{444}"
			Dim massAdd As TestData()
			massAdd = {td1, td2, td3, td4, td5}
			storage.AddObjects(massAdd)
			Dim spadd As New SearchParams({New FindCriteria("id", FindCondition.notEqual, "td"), New FindCriteria("id", FindCondition.notEqual, "ta-daaa")})
			spadd.SelectOptions = New SelectOptions(0, 5)
			Dim F = storage.FindObj(spadd)

			Dim ms = storage.FindObj(Nothing)
			Dim polo = storage.GetObjects(ms)

			storage.AddObj(testData1)
			storage.AddObj(testData2)

			Text = testData1.ID + testData1.Cat
			testData1.Cat = "meow_meow"
			storage.UpdateObj(testData1)

			Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.likeEqaul, "%2"), New FindCriteria("Int.Second", FindCondition.eqaul, "4444")})
			sp.SelectOptions = New SelectOptions(4)
			Dim sortP As New SortParam("Cat", SortMode.Descending)
			Dim FOcount = storage.FindObjCount(sp)

			Dim constains = storage.Contains("{0fda64fb-3d8b-485e-b50e-8e0b106b8916}")

			Dim mass As String()
			mass = {"{00000000-0000-0000-0000-000000000000}",
					"{11111111-1111-1111-1111-111111111111}",
					"{22222222-2222-2222-2222-222222222222}",
					"{33333333-3333-3333-3333-333333333333}",
					"{44444444-4444-4444-4444-444444444444}"}

			Dim objects = storage.GetObjects(mass, sortP)

			Dim cat2Id = storage.FindObj(New SearchParams(Nothing, sortP))

			Dim cat2 = storage.GetObj(cat2Id.First)

			Dim newData = storage.GetObj(testData1.ID)

			Dim allCats = storage.FindObj(Nothing)
			'For Each ff In allCats
			'	storage.RemoveObj(ff)
			'Next

			Dim tmp = "tmp"

		Catch ex As Exception
			MessageBox.Show(ex.Message)
		End Try
	End Sub
End Class
