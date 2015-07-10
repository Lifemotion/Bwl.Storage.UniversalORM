Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Drawing
Imports System.Threading
Imports Bwl.Storage.UniversalORM

<TestClass()> Public Class LocalStorageTest_File_WithIndexing
	Inherits LocalStorageBaseTest

	Protected Overrides Function CreateLocalStorage() As ILocalStorage
		Dim path = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\Data")
		Dim manager As New FileStorageManager(path)
		manager.UseIndexing = True
		Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
        Dim blobFileSaver = New FileBlobFieldsWriter(blobSaverDir)
        Dim localStorage = New Bwl.Storage.UniversalORM.LocalStorage(manager, blobFileSaver)
		Return localStorage
	End Function
End Class