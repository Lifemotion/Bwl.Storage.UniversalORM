Imports Bwl.Storage.UniversalORM
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class TempStorage
    Inherits Bwl.Storage.UniversalORM.CommonObjStorage

    Implements Bwl.Storage.UniversalORM.IObjStorageManager
    Implements Bwl.Storage.UniversalORM.IBlobFieldsWriter

    Public Property ObjDataInfo As ObjDataInfo

    Public Sub New(type As Type)
        MyBase.New(type)
    End Sub

    Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
        Return Me
    End Function

    Public Function Load(parentObjId As String) As BlobFieldsSet Implements IBlobFieldsWriter.Load
        Return ObjDataInfo.ObjBlobInfo
    End Function

    Public Sub Remove(parentObjId As String) Implements IBlobFieldsWriter.Remove

    End Sub

    Public Sub Save(objBlobInfo As BlobFieldsSet) Implements IBlobFieldsWriter.Save
        ObjDataInfo.ObjBlobInfo = objBlobInfo
    End Sub

    Public Overrides Sub AddObj(obj As ObjBase)
        ObjDataInfo = New ObjDataInfo
        ObjDataInfo.ObjInfo = New ObjInfo
        ObjDataInfo.ObjInfo.ObjType = obj.GetType
        ObjDataInfo.ObjInfo.Obj = Bwl.Storage.UniversalORM.CfJsonConverter.Serialize(obj)
    End Sub

    Public Overrides Function FindObj(searchParams As SearchParams) As String()
        Return Nothing
    End Function

    Public Overrides Function StrToObj(jsonObj As String, typeName As String) As ObjBase
        Dim res As ObjBase = Nothing
        If (typeName = "-") Or String.IsNullOrWhiteSpace(typeName) Then
            typeName = SupportedType.AssemblyQualifiedName
        End If
        If (jsonObj IsNot Nothing) Then
            Dim json = jsonObj.ToString
            res = CfJsonConverter.Deserialize(json, Type.GetType(typeName.ToString))
        End If
        Return res
    End Function

    Public Overloads Overrides Function StrToObj(Of T As ObjBase)(jsonObj As String, typeName As String) As T
        Return StrToObj(jsonObj, typeName)
    End Function

    Public Overrides Function GetObj(id As String) As ObjBase
        Dim type = ObjDataInfo.ObjInfo.ObjType
        Dim obj = CfJsonConverter.Deserialize(ObjDataInfo.ObjInfo.Obj, type)
        Try
            If TypeOf obj Is ObjContainer Then
                Dim objCont = CType(obj, ObjContainer)
                Dim t = objCont.Type
                objCont.Obj = CfJsonConverter.Deserialize(objCont.Obj.ToString, t)
            End If
        Catch ex As Exception
        End Try
        Return obj
    End Function

    Public Overrides Function GetSomeFieldDistinct(fieldName As String) As IEnumerable(Of String)
        Return Nothing
    End Function

    Public Overrides Sub RemoveObj(id As String)

    End Sub

    Public Overrides Sub RemoveObjs(ids As String())

    End Sub

    Public Overrides Sub UpdateObj(obj As ObjBase)

    End Sub

    Public Overloads Overrides Function GetObjects(Of T As ObjBase)(searchParams As SearchParams) As IEnumerable(Of T)
        Return Nothing
    End Function

    Public Overrides Function GetObjects(searchParams As SearchParams) As IEnumerable(Of ObjBase)
        Return Nothing
    End Function
    Public Overloads Overrides Function GetObjects(Of T As ObjBase)(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of T)
        Return Nothing
    End Function

    Public Overrides Function GetObjects(objIds As String(), Optional sortParam As SortParam = Nothing) As IEnumerable(Of ObjBase)
        Return Nothing
    End Function

    Public Overrides Function Contains(id As String) As Boolean
        Return False
    End Function

    Public Overloads Overrides Function GetObj(Of T As ObjBase)(id As String) As T
        Return Nothing
    End Function


    Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
        Return Me
    End Function

    Public Overrides Sub AddObjects(obj() As ObjBase)
    End Sub

    Public Overrides Sub RemoveAllObjects()

    End Sub

    Public Overrides Function FindObjCount(searchParams As SearchParams) As Long
        Return 0
    End Function

    ' Следующие поля - только для SQL, в TempStorage они бесполезны
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
