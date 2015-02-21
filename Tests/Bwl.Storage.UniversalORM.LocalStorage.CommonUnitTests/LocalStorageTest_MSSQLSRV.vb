Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Data.SqlClient
Imports System.IO
Imports Bwl.Storage.UniversalORM
Imports System.Drawing
Imports Bwl.Storage.UniversalORM.LocalStorage
Imports Bwl.Storage.UniversalORM.AdoDb
Imports Bwl.Framework

<TestClass()> Public Class LocalStorageTest_SqlSrv
    Inherits LocalStorageBaseTest

    Protected Overrides Function CreateLocalStorage() As ILocalStorage
        Dim app = New Bwl.Framework.AppBase(True)
        Dim settings = New LocalSettings_SqlSrv(app.RootStorage)
        Dim manager = New MSSQLSRVStorageManager(settings.SqlConnectionStringBuilder)
        Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
        Dim blobFileSaver = New Blob.FileBlobSaver(blobSaverDir)
        Dim localStorage = New Bwl.Storage.UniversalORM.LocalStorage.LocalStorage(manager, blobFileSaver)
        Return localStorage
    End Function

End Class