﻿Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Data.SqlClient
Imports System.IO
Imports Bwl.Storage.UniversalORM
Imports System.Drawing
Imports Bwl.Storage.UniversalORM.LocalStorage
Imports FirebirdSql.Data.FirebirdClient
Imports Bwl.Storage.UniversalORM.Firebird

<TestClass()> Public Class LocalStorageTest_Firebird_Service
	Inherits LocalStorageBaseTest

	Protected Overrides Function CreateLocalStorage() As ILocalStorage
		Dim app = New Bwl.Framework.AppBase(True)
		Dim settings = New LocalSettings_Firebird(app.RootStorage)
		Dim manager = New Firebird.FbStorageManager(settings.ConnectionStringBuilder_Service)

		Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
		Dim blobFileSaver = New Blob.FileBlobSaver(blobSaverDir)
		Dim localStorage = New Bwl.Storage.UniversalORM.LocalStorage.LocalStorage(manager, blobFileSaver)
		Return localStorage
	End Function

End Class