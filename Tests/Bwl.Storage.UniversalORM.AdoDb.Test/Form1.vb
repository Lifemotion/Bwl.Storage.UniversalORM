Imports System.Data.SqlClient

Public Class Form1

	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		Dim conStrBld = New SqlConnectionStringBuilder()
		conStrBld.InitialCatalog = "TestDB"
		conStrBld.UserID = "DrFenazepam-ПК\DrFenazepam" ' "sa"
		conStrBld.Password = ""	'"123"
		conStrBld.DataSource = "DRFENAZEPAM-ПК\SQLEXPRESS" '"(local)"
		conStrBld.IntegratedSecurity = True
		conStrBld.ConnectTimeout = 1

		Dim manager = New MSSQLSRVStorageManager(conStrBld)
		Dim storage As CommonObjStorage

		Dim testData1 = New TestData
		testData1.Cat = "CAT1111111"
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

		Dim testdata3 = New TestData2()
		testdata3.F1 = "11111"
		testdata3.F2 = 11111
		testdata3.ID = Guid.NewGuid.ToString("B")

		Try
			storage = manager.CreateStorage(Of TestData)("TestDataStorage")
			storage.AddObj(testData1)
			storage.AddObj(testData2)
			storage.AddObj(testdata3)

			Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.likeEqaul, "%2"), New FindCriteria("Int.Second", FindCondition.eqaul, "4444")})

			Dim cat2Id = storage.FindObj(sp)

			Dim cat2 = storage.GetObj(cat2Id.First)
			Dim gettdi = storage.GetObj(testdata3.ID)
			'Dim newData = storage.GetObj(testData1.ID)

			'Dim allCats = storage.FindObj(Nothing)
			'For Each ff In allCats
			'	storage.RemoveObj(ff)
			'Next

		Catch ex As Exception
			Dim t = 0
		End Try



	End Sub
End Class
