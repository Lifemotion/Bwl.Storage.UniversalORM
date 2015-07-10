Imports FirebirdSql.Data.FirebirdClient
Imports Bwl.Storage.UniversalORM.Firebird
Imports System.IO

Public Class FireBirdForm
    Inherits Bwl.Framework.FormAppBase

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim settings = New LocalSettings_Firebird(AppBase.RootStorage)


        '     Try
        '------------------
        Dim _localStorage As ILocalStorage
        Dim fileSaverDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileData")
        Dim storageManager = New Firebird.FbStorageManager(settings.ConnectionStringBuilder_Embeded)
        Dim blobSaverDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BlobData")
        Dim blobFileSaver = New FileBlobSaver(blobSaverDir)
        _localStorage = New Bwl.Storage.UniversalORM.LocalStorage(storageManager, blobFileSaver)

        Dim data = New FireBirdTestDataInternal
        data.First = "2222"
        data.Second = 2222
        _localStorage.AddObj(data)
        Dim contains = _localStorage.Contains(data.ID, GetType(FireBirdTestDataInternal))
        '--
        Dim spt = New SearchParams
        spt.FindCriterias = {New FindCriteria("Second", FindCondition.eqaul, 123)}
        Dim ids = _localStorage.FindObj(Of FireBirdTestDataInternal)(spt)

        '--
        Dim _data1 As New FireBirdTestData
        _data1.Cat = "cat11"
        _data1.ID = Guid.NewGuid.ToString("B")
        _data1.Image = New Bitmap(100, 100)

        Dim _data2 As New FireBirdTestData
        _data2.Cat = "cat22"
        _data2.ID = Guid.NewGuid.ToString("B")
        _data2.Image = New Bitmap(30, 47)

        _localStorage.RemoveAllObj(GetType(FireBirdTestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        Dim ftmp = _localStorage.FindObj(Of FireBirdTestData)(Nothing)
        Dim sps = New SearchParams()
        Dim sort = New SortParam("Timestamp", SortMode.Ascending)
        sps = New SearchParams(sortParam:=sort)
        Dim ids1 = _localStorage.FindObj(Of FireBirdTestData)(sps)

        sort = New SortParam("Timestamp", SortMode.Descending)
        sps = New SearchParams(sortParam:=sort)
        Dim ids2 = _localStorage.FindObj(Of FireBirdTestData)(sps)
        _localStorage.RemoveAllObj(GetType(FireBirdTestData))
        '--
        Dim _data3 As New FireBirdTestData
        _data3.Cat = "cat33"
        _data3.ID = Guid.NewGuid.ToString("B")
        _data3.Image = New Bitmap(67, 80)

        Dim _data4 As New FireBirdTestData
        _data4.Cat = "cat44"
        _data4.ID = Guid.NewGuid.ToString("B")
        _data4.Image = New Bitmap(36, 33)

        Dim _data5 As New FireBirdTestData
        _data5.Cat = "cat55"
        _data5.ID = Guid.NewGuid.ToString("B")
        _data5.Image = New Bitmap(100, 100)

        Dim _data6 As New FireBirdTestData
        _data6.Cat = "cat66"
        _data6.ID = Guid.NewGuid.ToString("B")
        _data6.Image = New Bitmap(30, 47)

        _localStorage.RemoveAllObj(GetType(FireBirdTestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)
        _localStorage.AddObj(_data4)
        _localStorage.AddObj(_data5)
        _localStorage.AddObj(_data6)
        Dim crit = {New FindCriteria("Timestamp", FindCondition.less, DateTime.Now)}
        Dim spo = New SearchParams(crit)
        Dim selectOpt = New SelectOptions(10)
        spo = New SearchParams(selectOptions:=selectOpt)
        Dim ids1o = _localStorage.FindObj(Of FireBirdTestData)(spo)

        selectOpt = New SelectOptions(6)
        spo = New SearchParams(selectOptions:=selectOpt)
        Dim ids2o = _localStorage.FindObj(Of FireBirdTestData)(spo)

        selectOpt = New SelectOptions(2)
        Dim sorto = New SortParam("Timestamp", SortMode.Descending)
        spo = New SearchParams(selectOptions:=selectOpt, sortParam:=sorto)
        Dim ids3o = _localStorage.FindObj(Of FireBirdTestData)(spo)
        '---
        _localStorage.RemoveAllObj(GetType(FireBirdTestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        Dim tmpsp = _localStorage.FindObj(Of FireBirdTestData)(Nothing)
        Dim spsp = New SearchParams()
        sort = New SortParam("Cat", SortMode.Ascending)
        spsp = New SearchParams(sortParam:=sort)
        ids1 = _localStorage.FindObj(Of FireBirdTestData)(spsp)

        sort = New SortParam("Cat", SortMode.Descending)
        spsp = New SearchParams(sortParam:=sort)
        ids2 = _localStorage.FindObj(Of FireBirdTestData)(spsp)
        _localStorage.RemoveAllObj(GetType(FireBirdTestData))

        '----------------------
        'Dim tmpStorage As New FBStorage(conStrBld, GetType(TestData), "TestDB")

        'Dim manager As FbStorageManager = Nothing
        'Try
        '	manager = New FbStorageManager(conStrBld)
        'Catch ex As Exception
        '	MessageBox.Show("Error: " + ex.Message)
        'End Try
        'Dim storage As CommonObjStorage

        Dim testData1 = New FireBirdTestData
        testData1.Cat = "CAT111111"
        testData1.Dog = 22222222
        testData1.ID = Guid.NewGuid.ToString("B")
        testData1.Int = New FireBirdTestDataInternal
        testData1.Int.First = "11111111111111"
        testData1.Int.Second = "2222222"
        testData1.Int.SomeData = "bad data"

        Dim testData2 = New FireBirdTestData
        testData2.Cat = "CAT2"
        testData2.Dog = 22222222
        testData2.ID = Guid.NewGuid.ToString("B")
        testData2.Int = New FireBirdTestDataInternal
        testData2.Int.First = "11111111111111"
        testData2.Int.Second = "4444"
        testData2.Int.SomeData = "bad data"


        _localStorage.RemoveAllObj(GetType(FireBirdTestData))

        Dim td1 = New FireBirdTestData
        td1.Cat = "td1"
        td1.Dog = 111
        td1.ID = Guid.NewGuid.ToString("B")
        td1.Int = New FireBirdTestDataInternal
        td1.Int.First = "1111"
        td1.Int.Second = 1112
        td1.Int.SomeData = "bad data"
        td1.Int.ID = "{000}"

        Dim td2 = New FireBirdTestData
        td2.Cat = "td2"
        td2.Dog = 111
        td2.ID = Guid.NewGuid.ToString("B")
        td2.Int = New FireBirdTestDataInternal
        td2.Int.First = "2221"
        td2.Int.Second = 2222
        td2.Int.SomeData = "bad data"
        td2.Int.ID = "{111}"

        Dim td3 = New FireBirdTestData
        td3.Cat = "td3"
        td3.Dog = 111
        td3.ID = Guid.NewGuid.ToString("B")
        td3.Int = New FireBirdTestDataInternal
        td3.Int.First = "3331"
        td3.Int.Second = 3332
        td3.Int.SomeData = "bad data"
        td3.Int.ID = "{222}"

        Dim td4 = New FireBirdTestData
        td4.Cat = "td4"
        td4.Dog = 111
        td4.ID = Guid.NewGuid.ToString("B")
        td4.Int = New FireBirdTestDataInternal
        td4.Int.First = "4441"
        td4.Int.Second = 4442
        td4.Int.SomeData = "bad data"
        td4.Int.ID = "{333}"

        Dim td5 = New FireBirdTestData
        td5.Cat = "td5"
        td5.Dog = 111
        td5.ID = Guid.NewGuid.ToString("B")
        td5.Int = New FireBirdTestDataInternal
        td5.Int.First = "5551"
        td5.Int.Second = 5552
        td5.Int.SomeData = "bad data"
        td5.Int.ID = "{444}"
        Dim massAdd As FireBirdTestData()
        massAdd = {td1, td2, td3, td4, td5}
        _localStorage.AddObjects(massAdd)
        Dim spadd As New SearchParams({New FindCriteria("id", FindCondition.notEqual, "td"), New FindCriteria("id", FindCondition.notEqual, "ta-daaa")})
        spadd.SelectOptions = New SelectOptions(0, 5)
        Dim F = _localStorage.FindObj(Of FireBirdTestData)(spadd)

        Dim ms = _localStorage.FindObj(Of FireBirdTestData)(Nothing)
        Dim polo = _localStorage.GetObjects(ms, GetType(FireBirdTestData))

        _localStorage.AddObj(testData1)
        _localStorage.AddObj(testData2)

        Text = testData1.ID + testData1.Cat
        testData1.Cat = "meow_meow"
        _localStorage.UpdateObj(testData1)

        Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.likeEqaul, "%2"), New FindCriteria("Int.Second", FindCondition.eqaul, "4444")})
        sp.SelectOptions = New SelectOptions(4)
        Dim sortP As New SortParam("Cat", SortMode.Descending)
        Dim FOcount = _localStorage.FindObjCount(GetType(FireBirdTestData), sp)

        Dim constains = _localStorage.Contains(Of FireBirdTestData)("{0fda64fb-3d8b-485e-b50e-8e0b106b8916}")

        Dim mass As String()
        mass = {"{00000000-0000-0000-0000-000000000000}",
                "{11111111-1111-1111-1111-111111111111}",
                "{22222222-2222-2222-2222-222222222222}",
                "{33333333-3333-3333-3333-333333333333}",
                "{44444444-4444-4444-4444-444444444444}"}

        Dim objects = _localStorage.GetObjects(Of FireBirdTestData)(mass, True, sortP)

        Dim cat2Id = _localStorage.FindObj(Of FireBirdTestData)(New SearchParams(Nothing, sortP))

        Dim cat2 = _localStorage.GetObj(Of FireBirdTestData)(cat2Id.First)

        Dim newData = _localStorage.GetObj(Of FireBirdTestData)(testData1.ID)

        Dim allCats = _localStorage.FindObj(Nothing)
        Dim tmp = "tmp"
        '    Catch ex As Exception
        'MessageBox.Show(ex.Message)
        '     End Try
    End Sub
End Class
