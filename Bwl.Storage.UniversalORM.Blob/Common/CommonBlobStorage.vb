Imports System.IO

Public Class CommonBlobStorage
	Implements IBlobStorage

	Private ReadOnly _blobStreamSavers As New List(Of IBlobBinarySaver)()
	Private ReadOnly _blobSavers As New List(Of IBlobSaver)()
	Private ReadOnly _typesInfo As New Dictionary(Of Type, String())()

	Public Sub New()
		AddStreamSaver(New BitmapStreamSaver)
		AddStreamSaver(New BytesStreamSaver)
	End Sub

	Public Sub AddSaver(saver As IBlobSaver)
		SyncLock (_blobSavers)
			If (saver IsNot Nothing) AndAlso (Not _blobSavers.Contains(saver)) Then
				_blobSavers.Add(saver)
			End If
		End SyncLock
	End Sub

	Public Sub RemoveSaver(saver As IBlobBinarySaver)
		SyncLock (_blobSavers)
			If (saver IsNot Nothing) AndAlso (_blobSavers.Contains(saver)) Then
				_blobSavers.Remove(saver)
			End If
		End SyncLock
	End Sub

	Public Sub AddStreamSaver(streamSaver As IBlobBinarySaver)
		SyncLock (_blobStreamSavers)
			If (streamSaver IsNot Nothing) AndAlso (Not _blobStreamSavers.Contains(streamSaver)) Then
				_blobStreamSavers.Add(streamSaver)
			End If
		End SyncLock
	End Sub

	Public Sub RemoveStreamSaver(streamSaver As IBlobBinarySaver)
		SyncLock (_blobStreamSavers)
			If (streamSaver IsNot Nothing) AndAlso (_blobStreamSavers.Contains(streamSaver)) Then
				_blobStreamSavers.Remove(streamSaver)
			End If
		End SyncLock
	End Sub

	Public ReadOnly Property BlobStreamSavers As IEnumerable(Of IBlobBinarySaver) Implements IBlobStorage.BlobStreamSavers
		Get
			Return _blobStreamSavers.ToArray
		End Get
	End Property

	Public ReadOnly Property BlobSavers As IEnumerable(Of IBlobSaver) Implements IBlobStorage.BlobSavers
		Get
			Return _blobSavers.ToArray
		End Get
	End Property

	Private Function AnalyzeType(parentObject As Object) As String()
		Dim type = parentObject.GetType

		Dim typeInfo() As String
		SyncLock (_typesInfo)
			If (_typesInfo.ContainsKey(type)) Then
				typeInfo = _typesInfo(type)
			Else
				typeInfo = ReflectionTools.GetBLOBMemberNames(type)
				_typesInfo.Add(type, typeInfo)
			End If
		End SyncLock

		Return typeInfo
	End Function

	''' <summary>
	''' Загружает в BLOB поля объекта их значения. 
	''' </summary>
	''' <param name="parentObject"></param>
	''' <param name="parentId"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Function LoadBlobs(parentObject As Object, parentId As String) As Boolean Implements IBlobStorage.LoadBlobs
		Try
			If (parentObject IsNot Nothing) Then
				Dim objBlobInfo = Load(parentId)
				SetBlobsValue(objBlobInfo, parentObject)
				Return True
			End If
		Catch ex As Exception

		End Try
		Return False
	End Function

	Private Function Load(parentId As String) As ObjBlobInfo
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

	Private Sub SetBlobsValue(objBlobInfo As ObjBlobInfo, parentObject As Object)
		SyncLock (_blobStreamSavers)
			For Each blobInfo In objBlobInfo.BlobsInfo
				Dim blobType = blobInfo.FieldType
				Dim streamSaver = _blobStreamSavers.FirstOrDefault(Function(s) s.SupportedTypes.Contains(blobType))
				If (streamSaver IsNot Nothing) Then
					Dim blobValue = streamSaver.FromBinary(blobInfo.Data, blobType)
					ReflectionTools.SetMemberValue(blobInfo.FieldName, parentObject, blobValue)
				End If
			Next
		End SyncLock
	End Sub

	''' <summary>
	''' Сохраняет значения BLOB полей объекта в хранилище.
	''' </summary>
	''' <param name="parentObject"></param>
	''' <param name="parentId"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Function SaveBlobs(parentObject As Object, parentId As String) As Boolean Implements IBlobStorage.SaveBlobs
		If (parentObject IsNot Nothing) Then
			Dim objBlobInfo = SaveToStream(parentObject, parentId)
			Save(objBlobInfo)
		End If
		Return True
	End Function

	Private Function SaveToStream(parentObject As Object, parentId As String) As ObjBlobInfo
		SyncLock (_blobStreamSavers)
			Dim typeInfo = AnalyzeType(parentObject)
			Dim objBlobInfo = New ObjBlobInfo
			objBlobInfo.ParentObjId = parentId
			objBlobInfo.BlobsInfo = New List(Of BlobInfo)

			For Each fieldInfo In typeInfo
				Try
					Dim blobValue = ReflectionTools.GetMemberValue(fieldInfo, parentObject)
					If (blobValue IsNot Nothing) Then
						Dim blobType = blobValue.GetType
						Dim streamSaver = _blobStreamSavers.FirstOrDefault(Function(s) s.SupportedTypes.Contains(blobType))

						If (streamSaver IsNot Nothing) Then
							Dim blobInfo = New BlobInfo
							blobInfo.BlobId = Guid.NewGuid.ToString
							blobInfo.FieldName = fieldInfo
							blobInfo.FieldType = blobType
							blobInfo.Data = streamSaver.ToBinary(blobValue)
							objBlobInfo.BlobsInfo.Add(blobInfo)
						End If
					End If
				Catch ex As Exception
					'...
				End Try
			Next
			Return objBlobInfo
		End SyncLock
	End Function

	Private Sub Save(objBlobInfo As ObjBlobInfo)
		SyncLock (_blobSavers)
			For Each saver In _blobSavers
				saver.Save(objBlobInfo)
			Next
		End SyncLock
	End Sub

	Public Sub Remove(Id As String) Implements IBlobStorage.Remove
		SyncLock (_blobSavers)
			For Each saver In _blobSavers
				saver.Remove(Id)
			Next
		End SyncLock
	End Sub
End Class
