﻿Public Interface ILocalStorage
    Sub AddObj(obj As ObjBase, Optional type As Type = Nothing)

    ''' <summary>Добоавление объектов одинакового типа в хранилище.</summary>
    Sub AddObjects(obj As ObjBase(), Optional type As Type = Nothing)

    Sub UpdateObj(obj As ObjBase, Optional type As Type = Nothing)

    Sub RemoveObj(id As String, type As Type)
    Sub RemoveObj(Of T As ObjBase)(id As String)
    Sub RemoveObjs(ids As String(), type As Type)
    Sub RemoveObjs(Of T As ObjBase)(id As String())

    Sub RemoveAllObj(type As Type)

    Function StrToObj(Of T As ObjBase)(jsonObj As String, typeName As String) As T
    Function StrToObj(jsonObj As String, typeName As String, gotType As Type) As ObjBase

    Function GetObj(Of T As ObjBase)(id As String, Optional loadBlob As Boolean = True) As T
    Function GetObj(id As String, type As Type, Optional loadBlob As Boolean = True) As ObjBase

    Function GetObjects(type As Type, Optional loadBlob As Boolean = True, Optional searchParams As SearchParams = Nothing) As IEnumerable(Of ObjBase)
    Function GetObjects(Of T As ObjBase)(Optional loadBlob As Boolean = True, Optional searchParams As SearchParams = Nothing) As IEnumerable(Of T)

    Function GetObjects(objIds As String(), type As Type, Optional loadBlob As Boolean = True, Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase)
    Function GetObjects(Of T As ObjBase)(objIds As String(), Optional loadBlob As Boolean = True, Optional sortParam As SortParam = Nothing) As IEnumerable(Of T)

    Function FindObj(type As Type, Optional searchParams As SearchParams = Nothing) As String()
    Function FindObj(Of T As ObjBase)(Optional searchParams As SearchParams = Nothing) As String()

    Function FindObjCount(type As Type, Optional searchParams As SearchParams = Nothing) As Long

    Function Contains(id As String, type As Type) As Boolean
    Function Contains(Of T As ObjBase)(id As String) As Boolean

    Function GetSomeFieldDistinct(fieldName As String, type As Type) As IEnumerable(Of String)

    <Obsolete("DO NOT use this method unless absolutely necessary", False)>
    Function ExecSqlGetObjects(type As Type, sqlString As String) As List(Of List(Of Object))

    <Obsolete("DO NOT use this method unless absolutely necessary", False)>
    Sub ExecSql(type As Type, sqlString As String)
End Interface
