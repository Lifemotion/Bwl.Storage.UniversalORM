Imports Bwl.Storage.UniversalORM
Imports NUnit.Framework
Imports Bwl.Framework
Imports Bwl.Storage.UniversalORM.SkiaSharp

<TestFixture>
Public Class LocalStorageTest_Firebird_Service
    Inherits LocalStorageBaseTest

    Protected Overrides Function CreateLocalStorage() As ILocalStorage
        DbType = DatabaseTestType.Firebird
        Dim app = New AppBase()
        Dim settings = New LocalSettings_Firebird(app.RootStorage)
        Dim manager = New FbStorageManager(settings.ConnectionStringBuilder_Service)

        Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
        Dim blobFileSaver = New FileBlobFieldsWriter(blobSaverDir)
        Dim localStorage = New LocalStorage(manager, blobFileSaver)
        localStorage.AddBinaryConverter(New SKBitmapBinaryConverter)
        Return localStorage
    End Function

End Class