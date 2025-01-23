Imports Bwl.Storage.UniversalORM
Imports Bwl.Storage.UniversalORM.Postgresql
Imports NUnit.Framework
Imports Bwl.Framework
Imports Bwl.Storage.UniversalORM.SkiaSharp

<TestFixture>
Public Class LocalStorageTest_Postgresql
    Inherits LocalStorageBaseTest

    Protected Overrides Function CreateLocalStorage() As ILocalStorage
        DbType = DatabaseTestType.PostgreSQL
        Dim app = New AppBase()
        Dim settings = New LocalSettings_Postgresql(app.RootStorage)
        Dim manager = New PgStorageManager(settings.ConnectionStringBuilder)

        Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
        Dim blobFileSaver = New FileBlobFieldsWriter(blobSaverDir)
        Dim localStorage = New LocalStorage(manager, blobFileSaver)
        localStorage.AddBinaryConverter(New SKBitmapBinaryConverter)
        Return localStorage
    End Function

End Class