Imports System.Data.SqlClient
Imports System.IO

Public Class Form1

	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		Dim conStrBld = New SqlConnectionStringBuilder()
		conStrBld.InitialCatalog = "TestDB1"
		conStrBld.UserID = "sa"
		conStrBld.Password = "123"
		conStrBld.DataSource = "(local)"
		conStrBld.IntegratedSecurity = False
		conStrBld.ConnectTimeout = 1

		Dim fileSaverDir = Path.Combine(Application.StartupPath, "FileData")

		Dim storageManager = New AdoDb.MSSQLSRVStorageManager(conStrBld)
		'Dim storageManager = New Files.FileStorageManager(fileSaverDir)

		Dim blobSaverDir = Path.Combine(Application.StartupPath, "BlobData")

		Dim blobFileSaver = New Blob.FileBlobSaver(blobSaverDir)

		'Dim localstorage = New LocalStorage(storageManager, New Blob.MemorySaver())
		Dim localstorage = New LocalStorage(storageManager, blobFileSaver)

		Dim data1 = New TestData()
		data1.Cat = "111111"
		data1.Dog = "222222"
		data1.Image = New Bitmap(33, 44)
		data1.Int.SomeBytes = {1, 1, 1, 2, 2, 3, 3, 3, 44, 55, 23}
		data1.Int.First = "44444444"
		data1.Int.Second = "555555555"
		data1.Int.SomeData = "999999999999"

		localstorage.AddObj(data1)
		Dim data3 = New TestData()
		data3.Cat = "111211"
		data3.Dog = "222222"
		data3.Image = New Bitmap(33, 44)
		data3.Int.SomeBytes = {1, 1, 1, 2, 2, 3, 3, 3, 44, 55, 23}
		data3.Int.First = "44444444"
		data3.Int.Second = "555555555"
		data3.Int.SomeData = "999999999999"

		localstorage.AddObj(data3)

		Dim data2 = localstorage.GetObj(Of TestData)(data1.ID)

		Dim crit = {New FindCriteria("Timestamp", FindCondition.less, DateTime.Now)}
		Dim sp = New SearchParams(crit)
		Dim ids = localstorage.FindObj(Of TestData)(sp)

		Dim objs11 = localstorage.GetObjects(Of TestData)(ids)
		Dim objs22 = localstorage.GetObjects(Of TestData)(ids, False)

		Dim res = localstorage.Contains(data1.ID, GetType(TestData))

		Dim sort = New SortParam("Timestamp", SortMode.Ascending)
		sp = New SearchParams(sortParam:=sort)
		Dim ids1 = localstorage.FindObj(Of TestData)(sp)

		sort = New SortParam("Timestamp", SortMode.Descending)
		sp = New SearchParams(sortParam:=sort)
		Dim ids2 = localstorage.FindObj(Of TestData)(sp)


		Dim tempS = New ObjDataInfoGenerator()
		Dim f = tempS.GetObjDataInfo(data1).GetOneFileForWeb
		Dim ob_ttt = tempS.GetObject(ObjDataInfo.GetFromOneFile(f))

		Dim selectOpt = New SelectOptions(10)
		sp = New SearchParams(selectOptions:=selectOpt)
		Dim ids3 = localstorage.FindObj(Of TestData)(sp)

		Dim objs111 = localstorage.GetObjects(Of TestData)(ids3)
		Dim objs222 = localstorage.GetObjects(Of TestData)(ids3, False)


		sp = New SearchParams(selectOptions:=selectOpt)
		Dim ids4 = localstorage.FindObj(Of TestData)(sp)

		Dim objs = localstorage.GetObjects(Of TestData)(ids4, False, New BetweenParam(3, 4))
		Dim objs2 = localstorage.GetObjects(Of TestData)(ids4, False)

		selectOpt = New SelectOptions(10)
		sort = New SortParam("Timestamp", SortMode.Descending)
		sp = New SearchParams(selectOptions:=selectOpt, sortParam:=sort)
		Dim ids7 = localstorage.FindObj(Of TestData)(sp)

		crit = {New FindCriteria("Timestamp", FindCondition.greater, DateTime.Now)}
		sp = New SearchParams(crit)
		ids = localstorage.FindObj(Of TestData)(sp)

		For Each objId In ids
			localstorage.RemoveObj(Of TestData)(objId)
		Next

		Dim count = localstorage.FindObjCount(GetType(TestData))

		ids = localstorage.FindObj(Of TestData)()
		For Each objId In ids
			localstorage.RemoveObj(Of TestData)(objId)
		Next

		Dim objDataGen = New ObjDataInfoGenerator
		Dim di1 = objDataGen.GetObjDataInfo(data1)
		Dim files = di1.GetFilesForWeb

		Dim di2 = ObjDataInfo.GetFromFiles(files)
		Dim data5 = objDataGen.GetObject(di2)
	End Sub
End Class
