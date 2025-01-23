Imports Bwl.Storage.UniversalORM
Imports Bwl.Framework
Imports NUnit.Framework
Imports Bwl.Storage.UniversalORM.SkiaSharp
Imports Bwl.Storage.UniversalORM.MSSQL

<TestFixture>
Public Class LocalStorageTest_SqlSrv
    Inherits LocalStorageBaseTest

    Protected Overrides Function CreateLocalStorage() As ILocalStorage
        DbType = DatabaseTestType.SQLServer
        Dim app = New AppBase()
        Dim settings = New LocalSettings_SqlSrv(app.RootStorage)
        Dim manager = New MSSQLSRVStorageManager(settings.SqlConnectionStringBuilder)
        Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
        Dim blobFileSaver = New FileBlobFieldsWriter(blobSaverDir)
        Dim localStorage = New LocalStorage(manager, blobFileSaver)
        localStorage.AddBinaryConverter(New SKBitmapBinaryConverter)
        Return localStorage
    End Function

End Class