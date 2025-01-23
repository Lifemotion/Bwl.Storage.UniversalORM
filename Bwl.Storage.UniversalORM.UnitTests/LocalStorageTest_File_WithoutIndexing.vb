Imports Bwl.Storage.UniversalORM
Imports NUnit.Framework
Imports Bwl.Storage.UniversalORM.SkiaSharp

<TestFixture>
Public Class LocalStorageTest_File_WithoutIndexing
    Inherits LocalStorageBaseTest

    Protected Overrides Function CreateLocalStorage() As ILocalStorage
        DbType = DatabaseTestType.File
        Dim path = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\Data")
        Dim manager As New FileStorageManager(path)
        manager.UseIndexing = False
        Dim blobSaverDir = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\BlobData")
        Dim blobFileSaver = New FileBlobFieldsWriter(blobSaverDir)
        Dim localStorage = New LocalStorage(manager, blobFileSaver)
        localStorage.AddBinaryConverter(New SKBitmapBinaryConverter)
        Return localStorage
    End Function
End Class