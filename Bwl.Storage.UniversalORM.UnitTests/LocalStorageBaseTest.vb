Imports Bwl.Storage.UniversalORM
Imports System.Threading
Imports NUnit.Framework
Imports Bwl.Storage.UniversalORM.SkiaSharp
Imports SkiaSharp

Friend Enum DatabaseTestType
    Unspecified
    File
    SQLite
    SQLServer
    PostgreSQL
    Firebird
End Enum

<TestFixture>
Public MustInherit Class LocalStorageBaseTest
    Private _localStorage As ILocalStorage

    Friend DbType As DatabaseTestType = DatabaseTestType.Unspecified

    Protected ReadOnly Property LocalStorage As ILocalStorage
        Get
            Return _localStorage
        End Get
    End Property

    Private _data1 As TestData
    Private _data2 As TestData
    Private _data3 As TestData
    Private _data4 As TestData
    Private _data5 As TestData
    Private _data6 As TestData

    Private _dataInt1 As TestDataInternal
    Private _dataInt2 As TestDataInternal

    '<TestInitialize()>
    Public Sub New()

        _localStorage = CreateLocalStorage()
        _localStorage.AddBinaryConverter(New SKBitmapBinaryConverter)

        _data1 = New TestData With {
            .Cat = "happycat",
            .Dog = "happydog",
            .ID = Guid.NewGuid.ToString("B"),
            .Image = New SKBitmap(100, 100)
        }

        'чтобы было отличие во времени создания
        Thread.Sleep(10)

        _data2 = New TestData With {
            .Cat = "cat22",
            .ID = Guid.NewGuid.ToString("B"),
            .Image = New SKBitmap(30, 47)
        }

        'чтобы было отличие во времени создания
        Thread.Sleep(10)

        _data3 = New TestData With {
            .Cat = "happycat",
            .Dog = "happydog",
            .ID = Guid.NewGuid.ToString("B"),
            .Image = New SKBitmap(67, 80)
        }

        'чтобы было отличие во времени создания
        Thread.Sleep(10)

        _data4 = New TestData With {
            .Cat = "cat44",
            .ID = Guid.NewGuid.ToString("B"),
            .Image = New SKBitmap(36, 33)
        }

        'чтобы было отличие во времени создания
        Thread.Sleep(10)

        _data5 = New TestData With {
            .Cat = "cat55",
            .ID = Guid.NewGuid.ToString("B"),
            .Image = New SKBitmap(100, 100)
        }

        'чтобы было отличие во времени создания
        Thread.Sleep(10)

        _data6 = New TestData With {
            .Cat = "happycat",
            .ID = Guid.NewGuid.ToString("B"),
            .Image = New SKBitmap(30, 47)
        }

        'чтобы было отличие во времени создания
        Thread.Sleep(10)

        _dataInt1 = New TestDataInternal With {
            .First = "happycat111"
        }

        'чтобы было отличие во времени создания
        Thread.Sleep(10)

        _dataInt2 = New TestDataInternal With {
            .First = "3rlosdf8"
        }

        If Not SubInit() Then Throw New Exception()
    End Sub

    Protected MustOverride Function CreateLocalStorage() As ILocalStorage

    Protected Overridable Function SubInit() As Boolean
        Return True
    End Function

    <Test>
    Public Sub RemoveAll()
        _localStorage.RemoveAllObj(GetType(TestData))
        Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)
        Assert.That(Object.Equals(p1, 0L))

        _localStorage.RemoveAllObj(GetType(TestDataInternal))
        Dim p2 = _localStorage.FindObjCount(GetType(TestDataInternal), Nothing)

        Dim TestDataCount = _localStorage.FindObj(Of TestData)()
        Assert.That(Not Object.Equals(TestDataCount, Nothing))
        Assert.That(Object.Equals(TestDataCount.Count, 0))

        Dim TestDataInternalCount = _localStorage.FindObj(Of TestDataInternal)()
        Assert.That(Not Object.Equals(TestDataInternalCount, Nothing))
        Assert.That(Object.Equals(TestDataInternalCount.Count, 0))

        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)

        Dim p3 = _localStorage.FindObjCount(GetType(TestData), Nothing)
        Assert.That(Object.Equals(p3, 3L))

        Dim p4 = _localStorage.FindObjCount(GetType(TestDataInternal), Nothing)
        Assert.That(Object.Equals(p4, 0L))

        Dim p6 = _localStorage.FindObj(Of TestData)()
        Assert.That(Object.Equals(p6.Length, 3))

        _localStorage.RemoveAllObj(GetType(TestData))

        Dim p5 = _localStorage.FindObjCount(GetType(TestData), Nothing)
        Assert.That(Object.Equals(p5, 0L))
    End Sub

    <Test>
    Public Sub AddObj()
        _localStorage.RemoveAllObj(GetType(TestData))
        Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)
        _localStorage.AddObj(_data3)
        Dim p3 = _localStorage.FindObjCount(GetType(TestData), Nothing)

        Assert.That(Object.Equals(p1, 0L))
        Assert.That(Object.Equals(p2, 2L))
        Assert.That(Object.Equals(p3, 3L))
    End Sub

    <Test>
    Public Sub PlainSqlGetObjCount()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)

        If (DbType = DatabaseTestType.File) Then
            Dim results = _localStorage.FindObjCount(GetType(TestData))
            Assert.That(Object.Equals(results, 3L))
        Else
            Dim script As String ' Works differently in different environments
            Select Case DbType
                Case DatabaseTestType.PostgreSQL
                    script = "SELECT Count(*) FROM ""TestData"""
                Case DatabaseTestType.SQLServer
                    script = "SELECT Count(*) FROM [TestData]"
                Case Else
                    script = "SELECT Count(*) FROM TestData"
            End Select
            Dim sqlResults = _localStorage.ExecSqlGetObjects(GetType(TestData), script)
            Assert.That(sqlResults(0)(0) = 3L)
        End If
    End Sub

    <Test>
    Public Sub GetSomeFieldDistinct()
        _localStorage.RemoveAllObj(GetType(TestData))

        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)

        Dim distinctValues = _localStorage.GetSomeFieldDistinct("Cat", GetType(TestData))

        Assert.That(Not Object.Equals(distinctValues, Nothing))
        Assert.That(Object.Equals(distinctValues.Count, 2))
        Assert.That(distinctValues.Contains(_data1.Cat))
        Assert.That(distinctValues.Contains(_data2.Cat))
    End Sub

    <Test>
    Public Sub BigData_100KB()
        _localStorage.RemoveAllObj(GetType(TestData))

        Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)

        Dim d1 = New TestData With {
            .Cat = "happycat",
            .ID = Guid.NewGuid.ToString("B"),
            .Image = New SKBitmap(100, 100)
        }
        ReDim d1.BigData(100000)
        d1.BigData(268) = 44
        d1.BigData(43453) = 58

        _localStorage.AddObj(d1)
        Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)

        Dim d2 = _localStorage.GetObj(Of TestData)(d1.ID)

        Assert.That(Object.Equals(p1, 0L))
        Assert.That(Object.Equals(p2, 1L))

        Assert.That(Object.Equals(d1.ID, d2.ID))
        Assert.That(Object.Equals(d1.Cat, d2.Cat))
        Assert.That(Object.Equals(d1.Image.Width, d2.Image.Width))
        Assert.That(Object.Equals(d1.Image.Height, d2.Image.Height))
        Assert.That(Object.Equals(d1.BigData.Length, d2.BigData.Length))

        Assert.That(Object.Equals(d1.BigData(100), d2.BigData(100)))
        Assert.That(Object.Equals(d1.BigData(268), d2.BigData(268)))
        Assert.That(Object.Equals(d1.BigData(43453), d2.BigData(43453)))
    End Sub

    <Test>
    Public Sub UpdateIndex()
        _localStorage.RemoveAllObj(GetType(TestData))

        Dim d1 = New TestData With {
            .Cat = "happycat",
            .ID = Guid.NewGuid.ToString("B"),
            .Image = New SKBitmap(100, 100)
        }
        ReDim d1.BigData(100000)
        d1.BigData(268) = 44
        d1.BigData(43453) = 58

        _localStorage.AddObj(d1)
        d1.Cat = "hhhhhh"
        _localStorage.UpdateObj(d1)

        Dim ids = _localStorage.FindObj(Of TestData)(New SearchParams({New FindCriteria("Cat", FindCondition.equal, d1.Cat)}))

        Assert.That(Object.Equals(d1.ID, ids.First))

        Dim d2 = _localStorage.GetObj(Of TestData)(d1.ID)

        Assert.That(Object.Equals(d1.Cat, d2.Cat))

        d1.Cat = Nothing
        _localStorage.UpdateObj(d1)

        d2 = _localStorage.GetObj(Of TestData)(d1.ID)

        Assert.That(Object.Equals(d1.Cat, d2.Cat))


        Assert.That(Object.Equals(d1.ID, d2.ID))
        Assert.That(Object.Equals(d1.Cat, d2.Cat))
        Assert.That(Object.Equals(d1.Image.Width, d2.Image.Width))
        Assert.That(Object.Equals(d1.Image.Height, d2.Image.Height))
        Assert.That(Object.Equals(d1.BigData.Length, d2.BigData.Length))

        Assert.That(Object.Equals(d1.BigData(100), d2.BigData(100)))
        Assert.That(Object.Equals(d1.BigData(268), d2.BigData(268)))
        Assert.That(Object.Equals(d1.BigData(43453), d2.BigData(43453)))
    End Sub

    <Test>
    Public Sub BigData_inCycle_10()
        _localStorage.RemoveAllObj(GetType(TestData))
        For index = 1 To 10
            BigData_10MB(False, False)
        Next
    End Sub

    <Test>
    Public Sub BigData_10MB()
        BigData_10MB(True, True)
    End Sub

    Public Sub BigData_10MB(needRemove As Boolean, needControlObjCount As Boolean)
        If needRemove Then
            _localStorage.RemoveAllObj(GetType(TestData))
        End If

        Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)

        Dim d1 = New TestData With {
            .Cat = "happycat",
            .ID = Guid.NewGuid.ToString("B"),
            .Image = New SKBitmap(100, 100)
        }
        ReDim d1.BigData(10000000)
        d1.BigData(268) = 44
        d1.BigData(43453) = 58
        d1.BigData(143453) = 62
        d1.BigData(543453) = 155
        d1.BigData(1043453) = 210

        _localStorage.AddObj(d1)
        Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)

        Dim d2 = _localStorage.GetObj(Of TestData)(d1.ID)

        If needControlObjCount Then
            Assert.That(Object.Equals(p1, 0L))
            Assert.That(Object.Equals(p2, 1L))
        End If

        Assert.That(Object.Equals(d1.ID, d2.ID))
        Assert.That(Object.Equals(d1.Cat, d2.Cat))
        Assert.That(Object.Equals(d1.Image.Width, d2.Image.Width))
        Assert.That(Object.Equals(d1.Image.Height, d2.Image.Height))
        Assert.That(Object.Equals(d1.BigData.Length, d2.BigData.Length))

        Assert.That(Object.Equals(d1.BigData(100), d2.BigData(100)))
        Assert.That(Object.Equals(d1.BigData(268), d2.BigData(268)))
        Assert.That(Object.Equals(d1.BigData(43453), d2.BigData(43453)))
        Assert.That(Object.Equals(d1.BigData(143453), d2.BigData(143453)))
        Assert.That(Object.Equals(d1.BigData(543453), d2.BigData(543453)))
        Assert.That(Object.Equals(d1.BigData(1043453), d2.BigData(1043453)))

    End Sub

    <Test>
    Public Sub RemoveObj()
        _localStorage.RemoveAllObj(GetType(TestData))
        Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)
        _localStorage.AddObj(_data1)
        Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)
        _localStorage.RemoveObj(Of TestData)(_data1.ID)
        Dim p3 = _localStorage.FindObjCount(GetType(TestData), Nothing)

        Assert.That(Object.Equals(p1, 0L))
        Assert.That(Object.Equals(p2, 1L))
        Assert.That(Object.Equals(p3, 0L))
    End Sub

    <Test>
    Public Sub RemoveObjs()
        Dim objsToAdd = New TestData() {_data1, _data2, _data3, _data4, _data5, _data6}
        Dim objsToRemove = New String() {_data1.ID, _data4.ID, _data5.ID, _data6.ID}
        Dim objsShouldLeft = objsToAdd.Where(Function(f) Not objsToRemove.Contains(f.ID)).Select(Function(f) f.ID).ToArray()

        _localStorage.RemoveAllObj(GetType(TestData))
        Dim p1 = _localStorage.FindObjCount(GetType(TestData), Nothing)
        _localStorage.AddObjects(objsToAdd)
        Dim p2 = _localStorage.FindObjCount(GetType(TestData), Nothing)
        _localStorage.RemoveObjs(Of TestData)(objsToRemove)
        Dim objsLeft = _localStorage.FindObj(GetType(TestData), Nothing)
        Dim p3 As Long = objsLeft.Count()
        Dim p4 = (objsLeft.Count() = objsShouldLeft.Where(Function(f) objsShouldLeft.Contains(f)).Count())

        Assert.That(Object.Equals(p1, 0L))
        Assert.That(Object.Equals(p2, 6L))
        Assert.That(Object.Equals(p3, 2L))
        Assert.That(Object.Equals(p4, True))
    End Sub

    <Test>
    Public Sub GetObj()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)

        Dim p1 = _localStorage.GetObj(Of TestData)(_data1.ID)
        Dim p2 = _localStorage.GetObj(Of TestData)(_data2.ID)
        Dim p3 = _localStorage.GetObj(Of TestData)(_data3.ID)

        Assert.That(Object.Equals(p1.ID, _data1.ID))
        Assert.That(Object.Equals(p2.ID, _data2.ID))
        Assert.That(Object.Equals(p3.ID, _data3.ID))
    End Sub

    <Test>
    Public Sub GetObjects()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)

        Dim ids = _localStorage.FindObj(Of TestData)()
        Dim p1 = _localStorage.GetObjects(Of TestData)(ids)
        Assert.That(Object.Equals(p1.Count, 3))

        Dim idsNew = p1.Select(Function(val) val.ID)
        Assert.That(idsNew.Contains(_data1.ID))
        Assert.That(idsNew.Contains(_data2.ID))
        Assert.That(idsNew.Contains(_data3.ID))
    End Sub

    <Test>
    Public Sub GetObjectsDirect()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)

        Dim p1 = _localStorage.GetObjects(Of TestData)()
        Assert.That(Object.Equals(p1.Count, 3))

        Dim idsNew = p1.Select(Function(val) val.ID)
        Assert.That(idsNew.Contains(_data1.ID))
        Assert.That(idsNew.Contains(_data2.ID))
        Assert.That(idsNew.Contains(_data3.ID))
    End Sub

    ' Doesn't really test functionality and might work differently depending on database, so it's useless
    '<Test>
    'Public Sub GetObjectsDirectVsClassic()

    '    ' Cache
    '    GetObjects()
    '    GetObjectsDirect()

    '    ' Test
    '    Dim sw1 = Stopwatch.StartNew()
    '    GetObjects()
    '    sw1.Stop()

    '    Dim sw2 = Stopwatch.StartNew()
    '    GetObjectsDirect()
    '    sw2.Stop()

    '    Assert.That(sw2.ElapsedMilliseconds <= sw1.ElapsedMilliseconds, $"sw2 ms = {sw2.ElapsedMilliseconds}, sw1 ms = {sw1.ElapsedMilliseconds}")
    'End Sub

    <Test>
    Public Sub FindObjCount_SelectOptions()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)
        _localStorage.AddObj(_data4)
        _localStorage.AddObj(_data5)
        _localStorage.AddObj(_data6)

        Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.equal, "happycat")})
        Dim p1 = _localStorage.FindObjCount(GetType(TestData), sp)
        sp.SelectOptions = New SelectOptions(2)
        Dim p2 = _localStorage.FindObjCount(GetType(TestData), sp)
        sp.SelectOptions = New SelectOptions(1, 1)
        Dim p3 = _localStorage.FindObjCount(GetType(TestData), sp)
        Assert.That(p1 = 3)
        Assert.That(p2 = 2)
        Assert.That(p3 = 1)
    End Sub

    <Test>
    Public Sub FindObj_Criteria()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)
        _localStorage.AddObj(_data4)
        _localStorage.AddObj(_data5)
        _localStorage.AddObj(_data6)

        Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.equal, "happycat")})
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 3))
    End Sub

    <Test>
    Public Sub FindObj_LikeCriteria()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)
        _localStorage.AddObj(_data4)
        _localStorage.AddObj(_data5)
        _localStorage.AddObj(_data6)

        Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.likeEqual, "%happy%")})
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 3))
        sp = New SearchParams({New FindCriteria("Cat", FindCondition.likeEqual, "%%")})
        Dim p2 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p2.Count, 6))
    End Sub

    <Test>
    Public Sub FindObj_MultipleCriteria()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)
        _localStorage.AddObj(_data4)
        _localStorage.AddObj(_data5)
        _localStorage.AddObj(_data6)

        ' Стандартное использование
        Dim availableValues = CfJsonConverter.Serialize(New String() {"cat22", "cat44"})
        Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.multipleEqual, availableValues)})
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 2))
        ' То же, но лишь с одним значением
        sp.FindCriterias = New FindCriteria() {New FindCriteria("Cat", FindCondition.multipleEqual, CfJsonConverter.Serialize(New String() {"cat22"}))}
        p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 1))
        ' Multiple-условие + дополнительное условие поиска
        sp.FindCriterias = New FindCriteria() {New FindCriteria("Cat", FindCondition.multipleEqual, availableValues),
                                               New FindCriteria("Cat", FindCondition.likeEqual, "%22%")}
        p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 1))
        ' Условие multiple negative
        sp = New SearchParams({New FindCriteria("Cat", FindCondition.multipleNotEqual, availableValues)})
        p1 = _localStorage.FindObj(Of TestData)(sp)
        Dim p2Values = _localStorage.GetObjects(Of TestData)(p1)
        Assert.That(Object.Equals(p1.Count, 4))
    End Sub

    <Test>
    Public Sub FindObj_FindCriteriaCriteria()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)
        _localStorage.AddObj(_data4)
        _localStorage.AddObj(_data5)
        _localStorage.AddObj(_data6)

        ' Стандартное использование
        Dim findCriteriaArray = New List(Of FindCriteria) From {New FindCriteria("Cat", FindCondition.equal, "happycat"),
                                                           New FindCriteria("Dog", FindCondition.equal, "happydog")}.ToArray()
        Dim serializedFindCriteria = CfJsonConverter.Serialize(findCriteriaArray)

        Dim sp = New SearchParams({New FindCriteria("Cat", FindCondition.equal, "happycat"),
                                   New FindCriteria("Cat", FindCondition.findCriteria, serializedFindCriteria)})
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 2))

        Dim objcts = _localStorage.GetObjects(Of TestData)().ToArray()

        ' То же, но negative
        sp = New SearchParams({New FindCriteria("Cat", FindCondition.equal, "happycat"),
                               New FindCriteria("Cat", FindCondition.findCriteriaNegative, serializedFindCriteria)})
        p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 1))
    End Sub

    <Test>
    Public Sub FindObj_SelectOption()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data4)
        _localStorage.AddObj(_data5)

        Dim so = New SelectOptions(2)
        Dim sort = New SortParam("Timestamp", SortMode.Ascending)
        Dim sp = New SearchParams(selectOptions:=so, sortParam:=sort)
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)

        so = New SelectOptions(1, 2)
        sp = New SearchParams(selectOptions:=so, sortParam:=sort)
        Dim p2 = _localStorage.FindObj(Of TestData)(sp)

        Assert.That(Object.Equals(p1.Count, 2))
        Assert.That(Object.Equals(p1(0), _data2.ID))
        Assert.That(Object.Equals(p1(1), _data4.ID))
        Assert.That(Object.Equals(p2.Count, 2))
        Assert.That(Object.Equals(p2(0), _data4.ID))
        Assert.That(Object.Equals(p2(1), _data5.ID))
    End Sub

    <Test>
    Public Sub FindObj_timestamp_1()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)

        Dim dt = DateTime.Now

        _data2.Timestamp = dt
        _localStorage.AddObj(_data2)


        Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.equal, dt)})
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 1))
        Assert.That(Object.Equals(p1.First, _data2.ID))
    End Sub

    <Test>
    Public Sub FindObj_timestamp_2()
        _localStorage.RemoveAllObj(GetType(TestData))
        _data1.Timestamp = Now
        _localStorage.AddObj(_data1)
        _data2.Timestamp = Now
        _localStorage.AddObj(_data2)

        Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.greater, DateTime.MinValue)})
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 2))
    End Sub

    <Test>
    Public Sub FindObj_timestamp_3()
        _localStorage.RemoveAllObj(GetType(TestData))
        _data1.Timestamp = Now
        _localStorage.AddObj(_data1)
        _data2.Timestamp = Now
        _localStorage.AddObj(_data2)

        Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.less, DateTime.MinValue)})
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 0))
    End Sub

    <Test>
    Public Sub FindObj_timestamp_4()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)

        Dim dt = DateTime.Now

        _data2.Timestamp = dt
        _localStorage.AddObj(_data2)

        Dim sp = New SearchParams({New FindCriteria("Timestamp", FindCondition.less, DateTime.MaxValue)})
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 2))
    End Sub

    <Test>
    Public Sub FindObj_timestamp_and_string()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)

        Dim dt = DateTime.Now

        _data2.Timestamp = dt
        _data2.Cat = "tttttttttttttttttttttt"
        _localStorage.AddObj(_data2)

        Dim sp = New SearchParams({
                                  New FindCriteria("Timestamp", FindCondition.less, DateTime.MaxValue),
                                  New FindCriteria("Cat", FindCondition.equal, _data2.Cat)
                                  })
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)
        Assert.That(Object.Equals(p1.Count, 1))
        Assert.That(Object.Equals(p1.First, _data2.ID))
    End Sub

    <Test>
    Public Sub FindObj_SortParam()
        _localStorage.RemoveAllObj(GetType(TestData))

        _data1.Timestamp = DateTime.MinValue
        _data2.Timestamp = DateTime.Now
        _data3.Timestamp = DateTime.MaxValue

        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)

        Dim so = New SelectOptions(2)
        Dim sp = New SearchParams(selectOptions:=so)
        Dim p1 = _localStorage.FindObj(Of TestData)(sp)

        Dim sortp = New SortParam("Timestamp", SortMode.Ascending)
        sp.SortParam = sortp
        Dim p2 = _localStorage.FindObj(Of TestData)(sp)
        Dim obj_tmp1 = _localStorage.GetObjects(Of TestData)(p2, True, sortp)

        sortp = New SortParam("Timestamp", SortMode.Descending)
        sp.SortParam = sortp
        Dim p3 = _localStorage.FindObj(Of TestData)(sp)

        Assert.That(Object.Equals(p1.Count, 2))

        Assert.That(Object.Equals(p2(0), _data1.ID))
        Assert.That(Object.Equals(p2(1), _data2.ID))

        Assert.That(Object.Equals(p3(0), _data3.ID))
        Assert.That(Object.Equals(p3(1), _data2.ID))
    End Sub

    <Test>
    Public Sub Contains()
        _localStorage.RemoveAllObj(GetType(TestData))
        Dim p1 = _localStorage.Contains(Of TestData)(_data1.ID)
        _localStorage.AddObj(_data1)
        Dim p2 = _localStorage.Contains(Of TestData)(_data1.ID)

        Assert.That(Object.Equals(p1, False))
        Assert.That(Object.Equals(p2, True))
    End Sub

    <Test>
    Public Sub FindObjBetween()
        Dim td1 = New TestData With {
            .Cat = "td",
            .Kitten = 111,
            .ID = "{00000000-0000-0000-0000-000000000000}",
            .Int = New TestDataInternal
        }
        td1.Int.First = "1111"
        td1.Int.Second = 1112
        td1.Int.SomeData = "bad data"
        td1.Int.ID = "{000}"

        Dim td2 = New TestData With {
            .Cat = "td",
            .Kitten = 111,
            .ID = "{11111111-1111-1111-1111-111111111111}",
            .Int = New TestDataInternal
        }
        td2.Int.First = "2221"
        td2.Int.Second = 2222
        td2.Int.SomeData = "bad data"
        td2.Int.ID = "{111}"

        Dim td3 = New TestData With {
            .Cat = "td",
            .Kitten = 111,
            .ID = "{22222222-2222-2222-2222-222222222222}",
            .Int = New TestDataInternal
        }
        td3.Int.First = "3331"
        td3.Int.Second = 3332
        td3.Int.SomeData = "bad data"
        td3.Int.ID = "{222}"

        Dim td4 = New TestData With {
            .Cat = "td",
            .Kitten = 111,
            .ID = "{33333333-3333-3333-3333-333333333333}",
            .Int = New TestDataInternal
        }
        td4.Int.First = "4441"
        td4.Int.Second = 4442
        td4.Int.SomeData = "bad data"
        td4.Int.ID = "{333}"

        Dim td5 = New TestData With {
            .Cat = "td",
            .Kitten = 111,
            .ID = "{44444444-4444-4444-4444-444444444444}",
            .Int = New TestDataInternal
        }
        td5.Int.First = "5551"
        td5.Int.Second = 5552
        td5.Int.SomeData = "bad data"
        td5.Int.ID = "{444}"
        Dim massAdd As TestData()
        massAdd = {td1, td2, td3, td4, td5}

        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.RemoveAllObj(GetType(TestDataInternal))

        _localStorage.AddObjects(massAdd)

        Dim spadd As New SearchParams({New FindCriteria("Cat", FindCondition.equal, "td")})
        spadd.SelectOptions = New SelectOptions(0, 3)
        Dim F1 = _localStorage.FindObj(Of TestData)(spadd)
        Assert.That(Object.Equals(4, F1.Count))

        spadd.SelectOptions = New SelectOptions(0, 2)
        Dim F2 = _localStorage.FindObj(Of TestData)(spadd)
        Assert.That(Object.Equals(3, F2.Count))
    End Sub

    <Test>
    Public Sub TestData_Add_GetObj()
        _localStorage.RemoveAllObj(GetType(TestData))

        Dim data = New TestData With {
            .Cat = "111111",
            .Kitten = "222222",
            .Image = New SKBitmap(33, 44)
        }
        data.Int.SomeBytes = {1, 1, 1, 2, 2, 3, 3, 3, 44, 55, 23}
        data.Int.First = "44444444"
        data.Int.Second = "555555555"
        data.Int.SomeData = "999999999999"
        _localStorage.AddObj(data)

        Dim data2 = New TestData
        _localStorage.AddObj(data2)

        Dim ob = _localStorage.GetObj(Of TestData)(data.ID)

        Assert.That(Not Object.Equals(ob, Nothing))
        Assert.That(Object.Equals(ob.Cat, data.Cat))
        Assert.That(Object.Equals(ob.Kitten, data.Kitten))
        Assert.That(Object.Equals(ob.Image.Width, data.Image.Width))
        Assert.That(Object.Equals(ob.Int.SomeData, Nothing))
        Assert.That(Object.Equals(ob.Int.SomeBytes, Nothing))
        Assert.That(Object.Equals(ob.Int.First, data.Int.First))
        Assert.That(Object.Equals(ob.Int.Second, data.Int.Second))

        Dim obWithoutBlob = _localStorage.GetObj(Of TestData)(data.ID, False)

        Assert.That(Not Object.Equals(obWithoutBlob, Nothing))
        Assert.That(Object.Equals(obWithoutBlob.Cat, data.Cat))
        Assert.That(Object.Equals(obWithoutBlob.Kitten, data.Kitten))
        Assert.That(Object.Equals(obWithoutBlob.Image, Nothing))
        Assert.That(Object.Equals(obWithoutBlob.Int.SomeData, Nothing))
        Assert.That(Object.Equals(obWithoutBlob.Int.SomeBytes, Nothing))
        Assert.That(Object.Equals(obWithoutBlob.Int.First, data.Int.First))
        Assert.That(Object.Equals(obWithoutBlob.Int.Second, data.Int.Second))
    End Sub

    <Test>
    Public Sub GetBadObjById()
        _localStorage.RemoveAllObj(GetType(TestData))
        Dim id = Guid.NewGuid.ToString("B")
        Dim obj = _localStorage.GetObj(Of TestData)(id)
        Assert.That(Object.Equals(obj, Nothing))
    End Sub

    <Test>
    Public Sub FindByBadField()
        _localStorage.RemoveAllObj(GetType(TestData))
        _localStorage.AddObj(_data1)
        _localStorage.AddObj(_data2)
        _localStorage.AddObj(_data3)
        _localStorage.AddObj(_data4)
        _localStorage.AddObj(_data5)
        _localStorage.AddObj(_data6)

        Dim sp = New SearchParams
        sp.FindCriterias = {New FindCriteria("Fffiieiwefjolijef", FindCondition.equal, "edfsdf")}

        Dim exc As Exception = Nothing
        Try
            Dim ids = _localStorage.FindObj(Of TestData)(sp)
        Catch ex As Exception
            exc = ex
        End Try
        Assert.That(Not Object.Equals(exc, Nothing))
    End Sub

    <Test>
    Public Sub ContainsBadObjById()
        _localStorage.RemoveAllObj(GetType(TestData))
        Dim contains = _localStorage.Contains(Of TestData)("{FE61FF34-CA73-4E8D-9515-5C8D47859B73}")
        Assert.That(Object.Equals(contains, False))
    End Sub


    ''' <summary>
    ''' Надо вынести отдельно
    ''' </summary>
    ''' <remarks></remarks>
    <Test> Public Sub FirebirdLocalStorageDB_GetDataInfo()
        Dim tempS = New ObjDataInfoGenerator()
        Dim pp = tempS.GetObjDataInfo(_data1)
        Dim f = pp.GetOneFileForWeb
        Dim ob_ttt = tempS.GetObject(ObjDataInfo.GetFromOneFile(f))
        Dim obttt = CType(ob_ttt, TestData)
        Assert.That(Object.Equals(_data1.ID, obttt.ID))
    End Sub




#Region "10KB_2Thread"

    ' для файлового хранилища этот тест может не выполняться, т.к.
    ' в нем не организовано одновременное обращение к харнилищу (блокировки и прочее)


    Private _exc_10kb_2thread_1 As Exception
    Private _exc_10kb_2thread_2 As Exception
    Private _sema1 As Semaphore
    Private _sema2 As Semaphore
    Private _nBytes As Integer = 1000000

    Private _localStorage1 As ILocalStorage
    Private _localStorage2 As ILocalStorage

    <Test>
    Public Sub BigData_N_Bytes_2Thread()

        _localStorage1 = CreateLocalStorage()
        _localStorage1.AddBinaryConverter(New SKBitmapBinaryConverter)
        _localStorage2 = CreateLocalStorage()
        _localStorage2.AddBinaryConverter(New SKBitmapBinaryConverter)

        _localStorage1.RemoveAllObj(GetType(TestData))
        _localStorage2.RemoveAllObj(GetType(TestData))

        Thread.Sleep(1000)

        _exc_10kb_2thread_1 = Nothing
        _exc_10kb_2thread_2 = Nothing
        _sema1 = New Semaphore(0, 1)
        _sema2 = New Semaphore(0, 1)

        Dim t1 = New Thread(AddressOf f1_N_Bytes_1)
        Dim t2 = New Thread(AddressOf f1_N_Bytes_2)

        t1.Start()
        t2.Start()

        _sema1.WaitOne()
        _sema2.WaitOne()

        Assert.That(Object.Equals(_exc_10kb_2thread_1, Nothing))
        Assert.That(Object.Equals(_exc_10kb_2thread_2, Nothing))
    End Sub

    Private Sub f1_N_Bytes_1()
        Try
            For index = 1 To 10
                Dim p1 = _localStorage1.FindObjCount(GetType(TestData), Nothing)

                Dim d1 = New TestData With {
                    .Cat = "happycat",
                    .ID = Guid.NewGuid.ToString("B"),
                    .Image = New SKBitmap(100, 100)
                }
                ReDim d1.BigData(_nBytes)
                d1.BigData(268) = 44

                _localStorage1.AddObj(d1)
                Dim p2 = _localStorage1.FindObjCount(GetType(TestData), Nothing)

                Dim d2 = _localStorage1.GetObj(Of TestData)(d1.ID)

                Assert.That(Object.Equals(d1.ID, d2.ID))
                Assert.That(Object.Equals(d1.Cat, d2.Cat))
                Assert.That(Object.Equals(d1.Image.Width, d2.Image.Width))
                Assert.That(Object.Equals(d1.Image.Height, d2.Image.Height))
                Assert.That(Object.Equals(d1.BigData.Length, d2.BigData.Length))

                Assert.That(Object.Equals(d1.BigData(100), d2.BigData(100)))
                Assert.That(Object.Equals(d1.BigData(268), d2.BigData(268)))
            Next
        Catch ex As Exception
            _exc_10kb_2thread_1 = ex
        End Try
        _sema1.Release()
    End Sub

    Private Sub f1_N_Bytes_2()
        Try
            For index = 1 To 10


                Dim p1 = _localStorage2.FindObjCount(GetType(TestData), Nothing)

                Dim d1 = New TestData With {
                    .Cat = "happycat",
                    .ID = Guid.NewGuid.ToString("B"),
                    .Image = New SKBitmap(100, 100)
                }
                ReDim d1.BigData(_nBytes)
                d1.BigData(268) = 44

                _localStorage2.AddObj(d1)
                Dim p2 = _localStorage2.FindObjCount(GetType(TestData), Nothing)

                Dim d2 = _localStorage2.GetObj(Of TestData)(d1.ID)


                Assert.That(Object.Equals(d1.ID, d2.ID))
                Assert.That(Object.Equals(d1.Cat, d2.Cat))
                Assert.That(Object.Equals(d1.Image.Width, d2.Image.Width))
                Assert.That(Object.Equals(d1.Image.Height, d2.Image.Height))
                Assert.That(Object.Equals(d1.BigData.Length, d2.BigData.Length))

                Assert.That(Object.Equals(d1.BigData(100), d2.BigData(100)))
                Assert.That(Object.Equals(d1.BigData(268), d2.BigData(268)))
            Next
        Catch ex As Exception
            _exc_10kb_2thread_2 = ex
        End Try
        _sema2.Release()
    End Sub

#End Region





End Class
