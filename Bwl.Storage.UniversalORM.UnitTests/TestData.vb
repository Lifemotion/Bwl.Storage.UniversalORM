﻿Imports Newtonsoft.Json
Imports Bwl.Storage.UniversalORM
Imports System.Drawing

Public Class TestData
    Implements ObjBase

    <Indexing>
    Public Property Cat As String

    <Indexing>
    Public Property Dog As String

    Public Property Kitten As Integer

    <Indexing>
    Public Property Timestamp As DateTime = DateTime.Now

    <Indexing> <BlobContainer>
    Public Property Int As New TestDataInternal

    Public Property BigData As Byte()

    Public Property ID As String Implements ObjBase.ID

    <Blob> <JsonIgnore>
    Public Property Image As Bitmap
End Class

Public Class TestDataInternal
    Implements ObjBase


    Public Property First As String
    <Indexing> Public Property Second As Integer

    <JsonIgnore>
    Public Property SomeData As String

    <JsonIgnore>
    Public Property SomeBytes As Byte()


    Public Property ID As String Implements ObjBase.ID
End Class
