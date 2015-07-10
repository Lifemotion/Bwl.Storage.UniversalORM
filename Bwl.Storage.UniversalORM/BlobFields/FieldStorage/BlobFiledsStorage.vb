Imports System.IO

Public Class BlobFiledsStorage
    Implements IBlobFiledsStorage

    Private ReadOnly _blobStreamSavers As New List(Of IBinaryConverter)()
    Private ReadOnly _blobSavers As New List(Of IBlobFieldsWriter)()
    'Private ReadOnly _typesInfo As New Dictionary(Of Type, String())()

    Public Sub New()
        AddBinaryConverter(New BitmapBinaryConverter)
        AddBinaryConverter(New BytesBinaryConverter)
    End Sub

    Public Sub AddBlobWriter(writer As IBlobFieldsWriter)
        SyncLock (_blobSavers)
            If (writer IsNot Nothing) AndAlso (Not _blobSavers.Contains(writer)) Then
                _blobSavers.Add(writer)
            End If
        End SyncLock
    End Sub

    Public Sub RemoveBlobWriter(writer As IBlobFieldsWriter)
        SyncLock (_blobSavers)
            If (writer IsNot Nothing) AndAlso (_blobSavers.Contains(writer)) Then
                _blobSavers.Remove(writer)
            End If
        End SyncLock
    End Sub

    Public Sub AddBinaryConverter(converter As IBinaryConverter)
        SyncLock (_blobStreamSavers)
            If (converter IsNot Nothing) AndAlso (Not _blobStreamSavers.Contains(converter)) Then
                _blobStreamSavers.Add(converter)
            End If
        End SyncLock
    End Sub

    Public Sub RemoveBinaryConverter(converter As IBinaryConverter)
        SyncLock (_blobStreamSavers)
            If (converter IsNot Nothing) AndAlso (_blobStreamSavers.Contains(converter)) Then
                _blobStreamSavers.Remove(converter)
            End If
        End SyncLock
    End Sub

    Public ReadOnly Property BinaryConverters As IEnumerable(Of IBinaryConverter) Implements IBlobFiledsStorage.BinaryConverters
        Get
            Return _blobStreamSavers.ToArray
        End Get
    End Property

    Public ReadOnly Property BlobWriters As IEnumerable(Of IBlobFieldsWriter) Implements IBlobFiledsStorage.BlobWriters
        Get
            Return _blobSavers.ToArray
        End Get
    End Property

    Private Function AnalyzeObj(parentObject As Object) As Dictionary(Of String, Object)
        Dim type = parentObject.GetType

        Dim typeInfo As Dictionary(Of String, Object)
        'SyncLock (_typesInfo)
        typeInfo = ReflectionTools.GetBLOBMemberNames(type, parentObject)
        '_typesInfo.Add(type, typeInfo)
        'End SyncLock

        Return typeInfo
    End Function

    ''' <summary>
    ''' Загружает в BLOB поля объекта их значения. 
    ''' </summary>
    ''' <param name="parentObject"></param>
    ''' <param name="parentId"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LoadBlobs(parentObject As Object, parentId As String) As Boolean Implements IBlobFiledsStorage.LoadBlobs
        'Try
        If (parentObject IsNot Nothing) Then
            Dim objBlobInfo = Load(parentId)
            SetBlobsValue(objBlobInfo, parentObject)
            Return True
        End If
        'Catch ex As Exception

        'End Try
        Return False
    End Function

    Private Function Load(parentId As String) As BlobFieldsSet
        SyncLock (_blobSavers)
            For Each saver In _blobSavers
                Try
                    Dim objBlobInfo = saver.Load(parentId)
                    If (objBlobInfo IsNot Nothing) Then
                        Return objBlobInfo
                    End If
                Catch ex As Exception
                End Try
            Next
        End SyncLock
        Return Nothing
    End Function

    Private Sub SetBlobsValue(objBlobInfo As BlobFieldsSet, parentObject As Object)
        If objBlobInfo IsNot Nothing Then
            SyncLock (_blobStreamSavers)
                For Each blobInfo In objBlobInfo.BlobFields
                    Dim blobType = blobInfo.FieldType
                    Dim streamSaver = _blobStreamSavers.FirstOrDefault(Function(s) s.SupportedTypes.Contains(blobType))
                    If (streamSaver IsNot Nothing) Then
                        Dim blobValue = streamSaver.FromBinary(blobInfo.Data, blobType)
                        ReflectionTools.SetMemberValueBLOB(blobInfo.FieldName, parentObject, blobValue)
                    End If
                Next
            End SyncLock
        End If
    End Sub

    ''' <summary>
    ''' Сохраняет значения BLOB полей объекта в хранилище.
    ''' </summary>
    ''' <param name="parentObject"></param>
    ''' <param name="parentId"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SaveBlobs(parentObject As Object, parentId As String) As Boolean Implements IBlobFiledsStorage.SaveBlobs
        If (parentObject IsNot Nothing) Then
            Dim objBlobInfo = SaveToStream(parentObject, parentId)
            Save(objBlobInfo)
        End If
        Return True
    End Function

    Private Function SaveToStream(parentObject As Object, parentId As String) As BlobFieldsSet
        SyncLock (_blobStreamSavers)
            Dim objInfo = AnalyzeObj(parentObject)
            Dim objBlobInfo = New BlobFieldsSet
            objBlobInfo.ParentObjId = parentId
            objBlobInfo.BlobFields = New List(Of BlobField)

            For Each pair In objInfo
                Try
                    Dim blobValue = pair.Value
                    If (blobValue IsNot Nothing) Then
                        Dim blobType = blobValue.GetType
                        Dim streamSaver = _blobStreamSavers.FirstOrDefault(Function(s) s.SupportedTypes.Contains(blobType))

                        If (streamSaver IsNot Nothing) Then
                            Dim blobInfo = New BlobField
                            blobInfo.BlobId = Guid.NewGuid.ToString
                            blobInfo.FieldName = pair.Key
                            blobInfo.FieldType = blobType
                            blobInfo.Data = streamSaver.ToBinary(blobValue)
                            objBlobInfo.BlobFields.Add(blobInfo)
                        End If
                    End If
                Catch ex As Exception
                    '...
                End Try
            Next
            Return objBlobInfo
        End SyncLock
    End Function

    Private Sub Save(objBlobInfo As BlobFieldsSet)
        SyncLock (_blobSavers)
            For Each saver In _blobSavers
                saver.Save(objBlobInfo)
            Next
        End SyncLock
    End Sub

    Public Sub Remove(Id As String) Implements IBlobFiledsStorage.Remove
        SyncLock (_blobSavers)
            For Each saver In _blobSavers
                saver.Remove(Id)
            Next
        End SyncLock
    End Sub

    Public Function LoadBlobs(parentObjects() As Object, parentIds() As String) As Boolean Implements IBlobFiledsStorage.LoadBlobs
        For i = 0 To parentObjects.Length - 1
            LoadBlobs(parentObjects(i), parentIds(i))
        Next
        Return True
    End Function

    Public Function SaveBlobs(parentObjects() As Object, parentIds() As String) As Boolean Implements IBlobFiledsStorage.SaveBlobs
        For i = 0 To parentObjects.Length - 1
            SaveBlobs(parentObjects(i), parentIds(i))
        Next
        Return True
    End Function
End Class
