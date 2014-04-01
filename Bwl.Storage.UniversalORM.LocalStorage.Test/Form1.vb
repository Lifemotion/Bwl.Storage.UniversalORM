Imports System.Data.SqlClient
Imports System.IO

Public Class Form1

	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		Dim conStrBld = New SqlConnectionStringBuilder()
		conStrBld.InitialCatalog = "TestDB1"
		conStrBld.UserID = "sa"
		conStrBld.Password = "123"
		conStrBld.DataSource = "(local)"
		conStrBld.IntegratedSecurity = True
		conStrBld.ConnectTimeout = 1

		Dim fileSaverDir = Path.Combine(Application.StartupPath, "FileData")

		'Dim storageManager = New AdoDb.MSSQLSRVStorageManager(conStrBld)
		Dim storageManager = New Files.FileStorageManager(fileSaverDir)

		Dim blobSaverDir = Path.Combine(Application.StartupPath, "BlobData")

		Dim blobFileSaver = New Blob.FileBlobSaver(blobSaverDir)

		Dim localstorage = New LocalStorage(storageManager, blobFileSaver)

		Dim data1 = New TestData()
		data1.Cat = "111111"
		data1.Dog = "222222"
		data1.Image = New Bitmap(33, 44)
		data1.Int.SomeBytes = {1, 1, 1, 2, 2, 3, 3, 3, 44, 55, 23}
		data1.Int.First = "44444444"
		data1.Int.Second = "555555555"
		data1.Int.SomeData = "999999999999"

		Dim id = localstorage.Save(Of TestData)(data1)
		Dim data3 = New TestData()
		data3.Cat = "111211"
		data3.Dog = "222222"
		data3.Image = New Bitmap(33, 44)
		data3.Int.SomeBytes = {1, 1, 1, 2, 2, 3, 3, 3, 44, 55, 23}
		data3.Int.First = "44444444"
		data3.Int.Second = "555555555"
		data3.Int.SomeData = "999999999999"

		localstorage.Save(Of TestData)(data3)

		Dim data2 = localstorage.Load(Of TestData)(id)

		Dim ids = localstorage.FindObj(Of TestData)({New FindCriteria("Timestamp", FindCondition.less, DateTime.Now)})

		ids = localstorage.FindObj(Of TestData)({New FindCriteria("Timestamp", FindCondition.greater, DateTime.Now)})

		For Each objId In ids
			localstorage.Remove(Of TestData)(objId)
		Next

		ids = localstorage.FindObj(Of TestData)()
		For Each objId In ids
			localstorage.Remove(Of TestData)(objId)
		Next

		Dim objDataGen = New ObjDataInfoGenerator
		Dim di1 = objDataGen.GetObjDataInfo(data1)
		Dim files = di1.GetFilesForWeb

		Dim di2 = ObjDataInfo.GetFromFiles(files)
		Dim data5 = objDataGen.GetObject(di2)
	End Sub
End Class
