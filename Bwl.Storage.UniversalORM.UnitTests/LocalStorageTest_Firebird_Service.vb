Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Data.SqlClient
Imports System.IO
Imports Bwl.Storage.UniversalORM
Imports System.Drawing
Imports Bwl.Storage.UniversalORM.LocalStorage
Imports FirebirdSql.Data.FirebirdClient

<TestClass()> Public Class LocalStorageTest_Firebird_Service
	Inherits LocalStorageBaseTest

	Protected Overrides Function CreateLocalStorage() As ILocalStorage
        Dim app = New Bwl.Framework.AppBase()
        Dim settings = New LocalSettings_Firebird(app.RootStorage)
        Dim manager = New FbStorageManager(settings.ConnectionStringBuilder_Service)

		Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
        Dim blobFileSaver = New FileBlobFieldsWriter(blobSaverDir)
        Dim localStorage = New Bwl.Storage.UniversalORM.LocalStorage(manager, blobFileSaver)
		Return localStorage
	End Function

End Class