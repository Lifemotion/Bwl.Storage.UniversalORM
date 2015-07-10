﻿Imports Bwl.Framework
Imports Bwl.Storage.UniversalORM
Imports Newtonsoft.Json

Public Class ExamplesApp
    Inherits FormAppBase

    Public Class TestDataClass
        Implements ObjBase
        Public Property ID As String = Guid.NewGuid.ToString("B") Implements ObjBase.ID
        <Indexing>
        Public Property Cat As String
        Public Property Dog As Integer
        <Indexing>
        Public Property Timestamp As DateTime
        <Indexing>
        Public Property NestedData As New TestDataNestedClass
        <Blob> <JsonIgnore>
        Public Property Image As Bitmap
    End Class

    Public Class TestDataNestedClass
        Implements ObjBase
        Public Property ID As String = Guid.NewGuid.ToString("B") Implements ObjBase.ID
        Public Property First As String
        <Indexing>
        Public Property Second As Integer
        <JsonIgnore>
        Public Property SomeData As String
        <JsonIgnore>
        Public Property SomeBytes As Byte()
    End Class

    Private Sub FirebirdStorageExample() Handles buttonFirebirdStorageExample.Click
        'путь к файлу базы данных
        Dim dbPath = IO.Path.Combine(AppBase.DataFolder, "fb_example_1.fdb")
        'создание фабрики (мендежера) хранилищ
        Dim fbStorageManager = New FbStorageManager(dbPath)
        'создание хранилища для типа данных TestDataClass
        Dim fbStorageForTestData = fbStorageManager.CreateStorage(Of TestDataClass)("TestDataClass")
        'создание двух объектов с разными данными
        Dim obj1 As New TestDataClass With {.Cat = "cat1", .Dog = 1, .Timestamp = Now, .NestedData = New TestDataNestedClass With {.First = "first1"}}
        Dim obj2 As New TestDataClass With {.Cat = "cat2", .Dog = 2, .Timestamp = Now.AddHours(-1), .NestedData = New TestDataNestedClass With {.First = "first2"}}
        'очистка хранилища
        fbStorageForTestData.RemoveAllObjects()
        'добавление объектов в хранилище
        fbStorageForTestData.AddObj(obj1)
        fbStorageForTestData.AddObj(obj2)
        'поиск идентификаторов объектов без ограничения
        Dim objSearchResults1a = fbStorageForTestData.FindObj(New SearchParams)
        'поиск идентификаторов объектов, где поле Cat равно значению cat1
        'поиск возможен только по полям Indexing
        Dim objSearchResults1b = fbStorageForTestData.FindObj(New SearchParams({New FindCriteria("Cat", FindCondition.eqaul, "cat1")}))
        'поиск идентификаторов объектов, где поле Cat содержит значению cat1
        Dim objSearchResults1c = fbStorageForTestData.FindObj(New SearchParams({New FindCriteria("Dog", FindCondition.likeEqaul, "cat")}))
        'извлечение объекта из хранилища по идентификатору
        Dim obj1restored As TestDataClass = fbStorageForTestData.GetObj(objSearchResults1a(0))
    End Sub


End Class

