﻿Imports System.IO

Public Class FileBlobFieldsWriter
    Implements IBlobFieldsWriter

    Private _rootDir As String
    Private ReadOnly _list As New Dictionary(Of String, BlobFieldsSet)()

    Public Sub New(rootDir As String)
        _rootDir = rootDir
    End Sub

    Public Property RootDir As String
        Get
            Return _rootDir
        End Get
        Set(value As String)
            _rootDir = value
        End Set
    End Property

    Public Function Load(parentObjId As String) As BlobFieldsSet Implements IBlobFieldsWriter.Load
        parentObjId = parentObjId.Replace(" ", "")
        Dim dir = GetPath(parentObjId, True, False)
        Dim objBlobInfo As BlobFieldsSet = Nothing
        If Directory.Exists(dir) Then
            Dim json = File.ReadAllText(Path.Combine(dir, parentObjId + ".json"))
            objBlobInfo = CfJsonConverter.Deserialize(Of BlobFieldsSet)(json)
            For Each blobInfo In objBlobInfo.BlobFields
                blobInfo.Data = File.ReadAllBytes(Path.Combine(dir, blobInfo.BlobId))
            Next
        End If
        Return ObjBlobInfo
    End Function

    Private Function GetPath(id As String, fullPath As Boolean, needCreate As Boolean) As String
        Dim subDir = id
        Dim root = _rootDir
        If subDir.Length > 10 Then
            subDir = id.Substring(1, 8)
            Dim dir = Path.Combine(Path.Combine(root, subDir), id)
            If Not Directory.Exists(dir) Then
                subDir = id.Substring(0, 4)
                dir = Path.Combine(Path.Combine(root, subDir), id)
                If Not Directory.Exists(dir) Then
                    subDir = id.Substring(1, 3)
                End If
            End If
        End If

        If (fullPath) Then
            Dim dir = Path.Combine(Path.Combine(root, subDir), id)
            If needCreate AndAlso (Not Directory.Exists(dir)) Then
                Directory.CreateDirectory(dir)
            End If
            Return dir
        Else
            root = Path.GetFileName(root)
            Dim dir = Path.Combine(Path.Combine(root, subDir), id)
            Return dir
        End If
    End Function

    Public Function GetBlobFilePath(id As String, blobName As String) As String
        Dim res = String.Empty
        id = id.Replace(" ", "")
        Dim dir = GetPath(id, True, False)
        Dim subDir = GetPath(id, False, False)
        If Directory.Exists(dir) Then
            Dim json = File.ReadAllText(Path.Combine(dir, id + ".json"))
            Dim objBlobInfo = CfJsonConverter.Deserialize(Of BlobFieldsSet)(json)
            Dim bi = objBlobInfo.BlobFields.FirstOrDefault(Function(b) b.FieldName = blobName)
            If (bi IsNot Nothing) Then
                res = Path.Combine(subDir, bi.BlobId)
            Else
                Throw New Exception("FileblobSaver.GetBlobFilePath _ не найдено поле " + blobName)
            End If
        End If
        Return res
    End Function

    Public Sub Save(objBlobInfo As BlobFieldsSet) Implements IBlobFieldsWriter.Save
        If objBlobInfo IsNot Nothing AndAlso objBlobInfo.BlobFields IsNot Nothing AndAlso objBlobInfo.BlobFields.Any Then
            Dim dir = GetPath(objBlobInfo.ParentObjId, True, True)
            Dim json = CfJsonConverter.Serialize(objBlobInfo)
            File.WriteAllText(Path.Combine(dir, objBlobInfo.ParentObjId + ".json"), json)

            For Each blobInfo In objBlobInfo.BlobFields
                Dim fname = Path.Combine(dir, blobInfo.BlobId)
                If (File.Exists(fname)) Then
                    File.Delete(fname)
                End If
                Dim fileStream = New FileStream(fname, FileMode.CreateNew)
                fileStream.Write(blobInfo.Data, 0, blobInfo.Data.Length)
                fileStream.Close()
                fileStream.Dispose()
            Next
        End If
    End Sub

    Sub Remove(parentObjId As String) Implements IBlobFieldsWriter.Remove
        parentObjId = parentObjId.Replace(" ", "")
        Try
            Dim dir = GetPath(parentObjId, True, False)
            If Directory.Exists(dir) Then
                Directory.Delete(dir, True)
            End If
            Dim parentDir = Directory.GetParent(dir).FullName
            If Directory.Exists(parentDir) Then
                If (Not Directory.GetFiles(parentDir).Any) And (Not Directory.GetDirectories(parentDir).Any) Then
                    Directory.Delete(parentDir)
                End If
            End If
        Catch ex As Exception
            '...
        End Try
    End Sub
End Class
