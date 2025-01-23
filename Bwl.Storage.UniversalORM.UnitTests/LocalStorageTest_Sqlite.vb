Imports Bwl.Framework
Imports Bwl.Storage.UniversalORM
Imports Bwl.Storage.UniversalORM.Sqlite
Imports Bwl.Storage.UniversalORM.SkiaSharp
Imports NUnit.Framework

<TestFixture>
Public Class LocalStorageTest_Sqlite
    Inherits LocalStorageBaseTest

    Protected Overrides Function CreateLocalStorage() As ILocalStorage
        DbType = DatabaseTestType.SQLite
        Dim app = New AppBase()
        Dim settings = New LocalSettings_Sqlite(app.RootStorage, app.DataFolder)
        Dim manager = New SqliteStorageManager(settings.ConnectionStringBuilder)

        Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
        Dim blobFileSaver = New FileBlobFieldsWriter(blobSaverDir)
        Dim localStorage = New LocalStorage(manager, blobFileSaver)
        localStorage.AddBinaryConverter(New SKBitmapBinaryConverter)
        Return localStorage
    End Function

End Class