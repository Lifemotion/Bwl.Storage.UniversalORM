Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Drawing
Imports System.Threading
Imports Bwl.Storage.UniversalORM.Files
Imports Bwl.Storage.UniversalORM

<TestClass()> Public Class LocalStorageTest_File
	Inherits LocalStorageBaseTest

	Protected Overrides Function CreateLocalStorage() As ILocalStorage
		Dim path = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\Data")
		Dim manager As New FileStorageManager(path)
		Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
		Dim blobFileSaver = New Blob.FileBlobSaver(blobSaverDir)
		Dim localStorage = New Bwl.Storage.UniversalORM.LocalStorage.LocalStorage(manager, blobFileSaver)
		Return localStorage
	End Function

	<TestMethod()>
	Public Sub FileStorage_RemoveAll()
		RemoveAll()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_AddObj()
		AddObj()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_RemoveObj()
		RemoveObj()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_GetObj()
		GetObj()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_GetObjects()
		GetObjects()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_Criteria()
		FindObj_Criteria()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_SelectOption()
		FindObj_SelectOption()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_1()
		FindObj_timestamp_1()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_2()
		FindObj_timestamp_2()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_3()
		FindObj_timestamp_3()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_4()
		FindObj_timestamp_4()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_timestamp_and_string()
		FindObj_timestamp_and_string()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObj_SortParam()
		FindObj_SortParam()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_Contains()
		Contains()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_ContainsBadObjById()
		ContainsBadObjById()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindObjBetween()
		FindObjBetween()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_TestData_Add_GetObj()
		TestData_Add_GetObj()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_GetBadObjById()
		GetBadObjById()
	End Sub

	<TestMethod()>
	Public Sub FileStorage_FindByBadField()
		FindByBadField()
	End Sub

End Class