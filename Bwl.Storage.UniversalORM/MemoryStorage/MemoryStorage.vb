Imports System.Reflection
Imports System.IO

Public Class MemoryStorage
    Inherits CommonObjStorage

    Private ReadOnly _objects As New Dictionary(Of String, ObjBase)

    Friend Sub New(type As Type)
        MyBase.New(type)
    End Sub

    Public Overrides Sub AddObj(obj As ObjBase)
        _objects(obj.ID) = obj
    End Sub

    Public Overrides Sub UpdateObj(obj As ObjBase)
        _objects(obj.ID) = obj
    End Sub

    Public Overrides Sub RemoveObj(id As String)
        If Contains(id) Then
            _objects.Remove(id)
        End If
    End Sub

    Public Overrides Sub RemoveObjs(ids As String())
        For Each id As String In ids
            RemoveObj(id)
        Next
    End Sub

    Public Overrides Function FindObj(searchParams As SearchParams) As String()
        Return _objects.Keys.ToArray
    End Function

    Public Overrides Function StrToObj(jsonObj As String, typeName As String) As ObjBase
        Dim res As ObjBase = Nothing
        Return res
    End Function

    Public Overloads Overrides Function StrToObj(Of T As ObjBase)(jsonObj As String, typeName As String) As T
        Return StrToObj(jsonObj, typeName)
    End Function

    Public Overrides Function GetObj(id As String) As ObjBase
        Return _objects(id)
    End Function

    Public Overrides Function GetObj(Of T As ObjBase)(id As String) As T
        Return CType(GetObj(id), T)
    End Function


    Public Overrides Function Contains(id As String) As Boolean
        Return _objects.ContainsKey(id)
    End Function


    Public Overrides Sub AddObjects(objects() As ObjBase)
        For Each obj In objects
            AddObj(obj)
        Next
    End Sub

    Public Overrides Sub RemoveAllObjects()
        _objects.Clear()
    End Sub

    Public Overrides Function GetObjects(Of T As ObjBase)(sp As SearchParams) As IEnumerable(Of T)
        Return Nothing
    End Function
    Public Overrides Function GetObjects(sp As SearchParams) As IEnumerable(Of ObjBase)
        Return Nothing
    End Function

    Public Overrides Function GetObjects(Of T As ObjBase)(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of T)
        Return Nothing
    End Function
    Public Overrides Function GetObjects(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase)
        Return Nothing
    End Function

    Public Overrides Function FindObjCount(searchParams As SearchParams) As Long
        Return _objects.Count
    End Function

    Public Overrides Function GetSomeFieldDistinct(fieldName As String) As IEnumerable(Of String)
        Return Nothing
    End Function

    ' Следующие поля - только для SQL, в MemoryStorage они бесполезны
    <Obsolete("DO NOT use this method unless absolutely necessary", False)>
    Public Overrides Function ExecSqlGetObjects(sqlString As String) As List(Of List(Of Object))
        Throw New NotImplementedException()
    End Function
    <Obsolete("DO NOT use this method unless absolutely necessary", False)>
    Public Overrides Sub ExecSql(sqlString As String)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Function GetNullDataIds() As String()
        Throw New NotImplementedException()
    End Function

    Public Overrides Sub CleanNullData()
        Throw New NotImplementedException()
    End Sub
End Class
