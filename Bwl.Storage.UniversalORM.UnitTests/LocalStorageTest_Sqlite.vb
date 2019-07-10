Imports Bwl.Framework
Imports Bwl.Storage.UniversalORM
Imports Bwl.Storage.UniversalORM.Sqlite

<TestClass()>
Public Class LocalStorageTest_Sqlite
    Inherits LocalStorageBaseTest

    Protected Overrides Function CreateLocalStorage() As ILocalStorage
        Dim app = New Bwl.Framework.AppBase()
        Dim settings = New LocalSettings_Sqlite(app.RootStorage, app.DataFolder)
        Dim manager = New SqliteStorageManager(settings.ConnectionStringBuilder)

        Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
        Dim blobFileSaver = New FileBlobFieldsWriter(blobSaverDir)
        Dim localStorage = New Bwl.Storage.UniversalORM.LocalStorage(manager, blobFileSaver)
        Return localStorage
    End Function

End Class