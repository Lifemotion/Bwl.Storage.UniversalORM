Public Class MemoryBlobFieldsWriter
    Implements IBlobFieldsWriter

    Private ReadOnly _list As New Dictionary(Of String, BlobFieldsSet)()

    Public Function Load(parentObjId As String) As BlobFieldsSet Implements IBlobFieldsWriter.Load
        SyncLock (_list)
            If (_list.ContainsKey(parentObjId)) Then
                Return _list(parentObjId)
            End If
        End SyncLock
        Return Nothing
    End Function

    Public Sub Save(objBlobInfo As BlobFieldsSet) Implements IBlobFieldsWriter.Save
        If (objBlobInfo IsNot Nothing) AndAlso (Not String.IsNullOrEmpty(objBlobInfo.ParentObjId)) Then
            SyncLock (_list)
                _list(objBlobInfo.ParentObjId) = objBlobInfo
            End SyncLock
        End If
    End Sub

    Sub Remove(parentObjId As String) Implements IBlobFieldsWriter.Remove
        SyncLock (_list)
            If (_list.ContainsKey(parentObjId)) Then
                _list.Remove(parentObjId)
            End If
        End SyncLock
    End Sub
End Class