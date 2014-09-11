Imports FirebirdSql.Data.FirebirdClient
Imports Bwl.Storage.UniversalORM
Imports Bwl.Storage.UniversalORM.Firebird
Imports System.Data.SqlClient
Imports Bwl.Storage.UniversalORM.AdoDb

Public Class FormBigDataTest
	Private _conStrBld As FbConnectionStringBuilder
	Private _manager As FbStorageManager
	Private _storage As CommonObjStorage

	Private _conStrBuidMSSQLSRV As SqlConnectionStringBuilder
	Private _managerMSSQLSRV As MSSQLSRVStorageManager
	Private _storageMSSQLSRV As CommonObjStorage

	Private _bigDataList As List(Of BigData)

	Private _stopWatch As Stopwatch
	Private _timespan As TimeSpan

	Private Sub InitFirebird()
		_conStrBld = New FbConnectionStringBuilder()
		_conStrBld.Database = "D:\CleverFlow\FirebirdBigDataTest\FirebirdBigDataTest\data\BigData.fdb"
		_conStrBld.UserID = "sysdba"
		_conStrBld.Password = "masterkey"
		_conStrBld.Dialect = 3
		_conStrBld.ServerType = FbServerType.Default
		_conStrBld.ConnectionTimeout = 1
		_conStrBld.ClientLibrary = "D:\CleverFlow\FirebirdBigDataTest\refs\fbe32\fbembed.dll"
		_manager = New FbStorageManager(_conStrBld)
		_storage = _manager.CreateStorage(Of BigData)("BigData")
		_bigDataList = New List(Of BigData)()
		_stopWatch = New Stopwatch()
		_timespan = New TimeSpan()
	End Sub

	Private Sub InitMSSQLSRV()
		_conStrBuidMSSQLSRV = New SqlConnectionStringBuilder()
		_conStrBuidMSSQLSRV.InitialCatalog = "BigData1"
		_conStrBuidMSSQLSRV.UserID = "DrFenazepam-ПК\DrFenazepam" '"sa"
		_conStrBuidMSSQLSRV.Password = "" '"123"
		_conStrBuidMSSQLSRV.DataSource = "DRFENAZEPAM-ПК\SQLEXPRESS" '"(local)"
		_conStrBuidMSSQLSRV.IntegratedSecurity = True
		_conStrBuidMSSQLSRV.ConnectTimeout = 1
		_managerMSSQLSRV = New MSSQLSRVStorageManager(_conStrBuidMSSQLSRV)
		_managerMSSQLSRV.SetDbForType(GetType(BigData), "BDNAME")
		_storageMSSQLSRV = _managerMSSQLSRV.CreateStorage(Of BigData)("BigData")
		_stopWatch = New Stopwatch()
		_timespan = New TimeSpan()
	End Sub

	Private Sub GenerateBigData(count As Long)
		Dim rand = New Random()
		Dim Float As Double = 0.0
		Dim ID As String = ""
		Dim iFloat As Double = 0.0
		Dim iInteger As Integer = 0
		Dim Image_width As Integer = 0
		Dim Image_height As Integer = 0
		Dim iText As String = ""
		Dim iTimestamp As New DateTime()
		Dim Text As String = ""
		Dim Time As New DateTime()
		Try
			For i = 0 To count - 1
				Dim data As New BigData
				Float = rand.NextDouble() * rand.Next(1, count + 100) + 0.0000000001
				ID = Guid.NewGuid().ToString("B")
				Threading.Thread.Sleep(10)
				iFloat = rand.NextDouble() * rand.Next(1, count + 100) + 0.0000000001
				Threading.Thread.Sleep(10)
				iInteger = rand.Next(count * 50)
				Threading.Thread.Sleep(10)
				Image_width = rand.Next(count + 300) + 1
				Image_height = rand.Next(Math.Round((count + 100) / 2)) + 1
				iText = Guid.NewGuid().ToString("N")
				iTimestamp = DateTime.Now()
				Text = Guid.NewGuid().ToString("N")
				Time = DateTime.Today()

				data.Float = Float
				data.ID = ID
				data.iFloat = iFloat
				data.iInteger = iInteger
				data.Image = New Bitmap(Image_width, Image_height)
				data.iText = iText
				data.iTimestamp = iTimestamp
				data.Text = Text
				data.Time = Time

				Dim tdata As New Data
				tdata.IDData = Guid.NewGuid().ToString("B")
				tdata.iIntData = rand.Next(count * 77)
				tdata.SomeBytesData = {0, 0, 0, 0, 0, 0, 0}


				Threading.Thread.Sleep(10)
				rand.NextBytes(tdata.SomeBytesData)

				tdata.TextData = Guid.NewGuid().ToString("N")
				data.iData = tdata
				_bigDataList.Add(data)
			Next
		Catch exc As Exception
			Dim msg = String.Format("Float:{0}" +
									+"ID:{1}" +
									+"iFloat:{2}" +
									+"iInteger:{3}" +
									+"width:{4}" +
									+"height:{5}" +
									+"iText:{6}" +
									+"iTimestamp:{7}" +
									+"Text:{8}" +
									+"Time:{9}", Float, ID, iFloat, iInteger, Image_width, Image_height, iText, iTimestamp, Text, Time)
			MessageBox.Show("Ошибка генерации BigData: " + exc.Message + "\n" + msg)
		End Try
	End Sub

	Private Sub SaveListObjects()
		For Each obj In _bigDataList
			_storage.AddObj(obj)
		Next
	End Sub

	Private Sub GenerateAndSaveObj(count As Integer)
		Dim rand = New Random()
		Dim Float As Double = 0.0
		Dim ID As String = ""
		Dim iFloat As Double = 0.0
		Dim iInteger As Integer = 0
		Dim Image_width As Integer = 0
		Dim Image_height As Integer = 0
		Dim iText As String = ""
		Dim iTimestamp As New DateTime()
		Dim Text As String = ""
		Dim Time As New DateTime()
		Try
			For i = 0 To count - 1
				Dim data As New BigData
				Dim rnd = rand.Next(1, 10000)
				Float = rand.NextDouble() * rand.Next(1, rnd + 100) + 0.0000000001
				ID = Guid.NewGuid().ToString("B")

				iFloat = rand.NextDouble() * rand.Next(1, rnd + 100) + 0.0000000001

				iInteger = rand.Next(rnd * 50)

				Image_width = rand.Next(rnd + 300) + 1
				Image_height = rand.Next(Math.Round((rnd + 100) / 2)) + 1
				iText = Guid.NewGuid().ToString("N")
				iTimestamp = DateTime.Now()
				Text = Guid.NewGuid().ToString("N")
				Time = DateTime.Today()

				data.Float = Float
				data.ID = ID
				data.iFloat = iFloat
				data.iInteger = iInteger
				data.Image = New Bitmap(Image_width, Image_height)
				data.iText = iText
				data.iTimestamp = iTimestamp
				data.Text = Text
				data.Time = Time

				Dim tdata As New Data
				tdata.IDData = Guid.NewGuid().ToString("B")
				tdata.iIntData = rand.Next(count * 77)
				tdata.SomeBytesData = {0, 0, 0, 0, 0, 0, 0}

				rand.NextBytes(tdata.SomeBytesData)

				tdata.TextData = Guid.NewGuid().ToString("N")
				data.iData = tdata
				_stopWatch.Start()
				_storage.AddObj(data)
				_stopWatch.Stop()
			Next
		Catch exc As Exception
			Dim msg = String.Format("Float:{0}" +
									+"ID:{1}" +
									+"iFloat:{2}" +
									+"iInteger:{3}" +
									+"width:{4}" +
									+"height:{5}" +
									+"iText:{6}" +
									+"iTimestamp:{7}" +
									+"Text:{8}" +
									+"Time:{9}", Float, ID, iFloat, iInteger, Image_width, Image_height, iText, iTimestamp, Text, Time)
			MessageBox.Show("Ошибка генерации BigData: " + exc.Message + "\n" + msg)
		End Try
	End Sub

	Private Sub GenerateAndSaveObjMSSQLSRV(count As Integer)
		Dim rand = New Random()
		Dim Float As Double = 0.0
		Dim ID As String = ""
		Dim iFloat As Double = 0.0
		Dim iInteger As Integer = 0
		Dim Image_width As Integer = 0
		Dim Image_height As Integer = 0
		Dim iText As String = ""
		Dim iTimestamp As New DateTime()
		Dim Text As String = ""
		Dim Time As New DateTime()
		Try
			For i = 0 To count - 1
				Dim data As New BigData
				Dim rnd = rand.Next(1, 10000)
				Float = rand.NextDouble() * rand.Next(1, rnd + 100) + 0.0000000001
				ID = Guid.NewGuid().ToString("B")

				iFloat = rand.NextDouble() * rand.Next(1, rnd + 100) + 0.0000000001

				iInteger = rand.Next(rnd * 50)

				Image_width = rand.Next(rnd + 300) + 1
				Image_height = rand.Next(Math.Round((rnd + 100) / 2)) + 1
				iText = Guid.NewGuid().ToString("N")
				iTimestamp = DateTime.Now()
				Text = Guid.NewGuid().ToString("N")
				Time = DateTime.Today()

				data.Float = Float
				data.ID = ID
				data.iFloat = iFloat
				data.iInteger = iInteger
				data.Image = New Bitmap(Image_width, Image_height)
				data.iText = iText
				data.iTimestamp = iTimestamp
				data.Text = Text
				data.Time = Time

				Dim tdata As New Data
				tdata.IDData = Guid.NewGuid().ToString("B")
				tdata.iIntData = rand.Next(count * 77)
				tdata.SomeBytesData = {0, 0, 0, 0, 0, 0, 0}

				rand.NextBytes(tdata.SomeBytesData)

				tdata.TextData = Guid.NewGuid().ToString("N")
				data.iData = tdata
				_stopWatch.Start()
				_storageMSSQLSRV.AddObj(data)
				_stopWatch.Stop()
			Next
		Catch exc As Exception
			Dim msg = String.Format("Float:{0}" +
									+"ID:{1}" +
									+"iFloat:{2}" +
									+"iInteger:{3}" +
									+"width:{4}" +
									+"height:{5}" +
									+"iText:{6}" +
									+"iTimestamp:{7}" +
									+"Text:{8}" +
									+"Time:{9}", Float, ID, iFloat, iInteger, Image_width, Image_height, iText, iTimestamp, Text, Time)
			MessageBox.Show("Ошибка генерации BigData: " + exc.Message + "\n" + msg)
		End Try
	End Sub


	Private Sub ContainsDB(count As Integer, id As String)
		For i = 0 To count
			_stopWatch.Start()
			_storage.Contains(id)
			_stopWatch.Stop()
		Next
	End Sub

	Private Sub ContainsMSSQLSRV(count As Integer, id As String)
		For i = 0 To count
			_stopWatch.Start()
			_storageMSSQLSRV.Contains(id)
			_stopWatch.Stop()
		Next
	End Sub

	Private Sub UpdateObj(count As Integer, obj As BigData)
		For i = 0 To count
			_storage.UpdateObj(obj)
		Next
	End Sub

	Private Sub UpdateObjMSSQLSRV(count As Integer, obj As BigData)
		For i = 0 To count
			_storageMSSQLSRV.UpdateObj(obj)
		Next
	End Sub

	Private Sub FindObjCount(count As Integer, sp As SearchParams)
		For i = 0 To count - 1
			_storage.FindObjCount(sp)
		Next
	End Sub

	Private Sub FindObjCountMSSQLSRV(count As Integer, sp As SearchParams)
		For i = 0 To count - 1
			Dim cnt = _storageMSSQLSRV.FindObjCount(sp)
		Next
	End Sub

	Private Sub FindObj(count As Integer, sp As SearchParams)
		For i = 0 To count - 1
			Dim p = _storage.FindObj(sp)
		Next
	End Sub

	Private Sub FindObjMSSQLSRV(count As Integer, sp As SearchParams)
		For i = 0 To count - 1
			Dim p = _storageMSSQLSRV.FindObj(sp)
		Next
	End Sub

	Private Sub GetObj(count As Integer, id As String)
		For i = 0 To count - 1
			Dim p = _storage.GetObj(id)
		Next
	End Sub

	Private Sub GetObjMSSQLSRV(count As Integer, id As String)
		For i = 0 To count - 1
			Dim p = _storageMSSQLSRV.GetObj(id)
		Next
	End Sub

	Private Sub GetObjects(count As Integer, ids As String())
		For i = 0 To count - 1
			Dim p = _storage.GetObjects(ids)
		Next
	End Sub

	Private Sub GetObjectsMSSQLSRV(count As Integer, ids As String())
		For i = 0 To count - 1
			Dim p = _storageMSSQLSRV.GetObjects(ids)
		Next
	End Sub

	Private Sub FormBigDataTest_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		'InitFirebird()
		InitMSSQLSRV()
		Dim count = 1000
		Dim ts As TimeSpan
		Dim stopWatch As New Stopwatch()
		Dim sp As New SearchParams({New FindCriteria("iText", FindCondition.notEqual, "txtxtx")})
		sp.SelectOptions = New SelectOptions(1000)


		'Dim obj1 As BigData = _storage.GetObj("{000062bd-290e-4799-a6b6-862a6fc46377}")
		'Dim obj2 As BigData = _storage.GetObj("{c40927d5-528e-4df5-8760-f59e63de81c9}")
		'Dim obj3 As BigData = _storage.GetObj("{ce11ec5f-f0fd-4c76-bef2-21c6e86c71d1}")

		'Dim obj1 As BigData = _storageMSSQLSRV.GetObj("{00010e5f-fb4b-453c-b584-d7dd5760d35b}")
		'Dim obj2 As BigData = _storageMSSQLSRV.GetObj("{c3a9972b-f38f-4939-b4f5-fda815410f0e}")
		'Dim obj3 As BigData = _storageMSSQLSRV.GetObj("{ffff44fa-1b4b-487d-8276-df3b5ec5d7e0}")
		Dim objnot As New BigData()
		objnot.ID = Guid.NewGuid.ToString("B")
		'Dim objectsID As String() = _storage.FindObj(sp)
		'Dim objectsIDMSSQLSRV As String() = _storageMSSQLSRV.FindObj(sp)
		stopWatch.Start()
		Try
			'GenerateAndSaveObjMSSQLSRV(count)
			'GenerateAndSaveObj(count)
			'запись №67200 = "{c40927d5-528e-4df5-8760-f59e63de81c9}"
			'запись #1 = "{000062bd-290e-4799-a6b6-862a6fc46377}"
			' запись №87999 = "{ce11ec5f-f0fd-4c76-bef2-21c6e86c71d1}"
			' iText последней записи = "b0b083a2cf004f4ab742b17c63af0431"
			'Dim resultContains = _storage.Contains("{000062bd-290e-4799-a6b6-862a6fc4637i}")
			'FindObj(100, Nothing)
			'FindObj(1, sp)
			'Dim resultFindSort = _storage.FindObj(sp)
			'ContainsDB(1000, "{c40927d5-528e-4df5-8760-f59e63de81c9}")
			'Dim reGetObj = _storage.GetObj("{000062bd-290e-4799-a6b6-862a6fc46377}")
			'FindObjCount(1000, sp)
			'GetObjects(10, objectsID)
			'GetObj(1000, "{c40927d5-528e-4df5-8760-f59e63de81c9}")
			'UpdateObj(1000, objnot)
			'ContainsMSSQLSRV(1000, obj2.ID)
			'FindObjMSSQLSRV(1, Nothing)
			'FindObjMSSQLSRV(100, sp)
			'FindObjCountMSSQLSRV(1000, sp)
			GetObjMSSQLSRV(1000, objnot.ID)
			'GetObjectsMSSQLSRV(1000, {objnot.ID})
			'UpdateObjMSSQLSRV(1, objnot)
		Catch exc As Exception
			MessageBox.Show(exc.Message)
		End Try
		stopWatch.Stop()

		ts = stopWatch.Elapsed
		_timespan = _stopWatch.Elapsed
		Dim elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
										ts.Hours,
										ts.Minutes,
										ts.Seconds,
										ts.Milliseconds / 10)

		Dim AddelapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
								_timespan.Hours,
								_timespan.Minutes,
								_timespan.Seconds,
								_timespan.Milliseconds / 10)
		If MessageBox.Show(String.Format("Время добаления {0} записей = {1} \n Время выполнения алгоритма: {2}", count, AddelapsedTime, elapsedTime)) = Windows.Forms.DialogResult.OK Then
			End
		End If

		Dim polo = _bigDataList
	End Sub

End Class
