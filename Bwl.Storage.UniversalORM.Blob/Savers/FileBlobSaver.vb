Imports System.IO

Public Class FileBlobSaver
	Implements Blob.IBlobSaver

	Private _rootDir As String
	Private ReadOnly _list As New Dictionary(Of String, ObjBlobInfo)()

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

	Public Function Load(parentObjId As String) As ObjBlobInfo Implements IBlobSaver.Load
		Dim dir = GetPath(parentObjId)
		Dim json = File.ReadAllText(Path.Combine(dir, parentObjId + ".json"))
		Dim objBlobInfo = JsonUtils.LoadFromJsonString(Of ObjBlobInfo)(json)
		For Each blobInfo In objBlobInfo.BlobsInfo
			blobInfo.Data = File.ReadAllBytes(Path.Combine(dir, blobInfo.BlobId))
		Next
		Return objBlobInfo
	End Function

	Private Function GetPath(id As String)
		Dim subDir = id
		If subDir.Length > 10 Then
			subDir = id.Substring(1, 8)
		End If
		Dim dir = Path.Combine(Path.Combine(_rootDir, subDir), id)
		If (Not Directory.Exists(dir)) Then
			Directory.CreateDirectory(dir)
		End If
		Return dir
	End Function

	Public Sub Save(objBlobInfo As ObjBlobInfo) Implements IBlobSaver.Save
		Dim dir = GetPath(objBlobInfo.ParentObjId)
		Dim json = JsonUtils.ToJson(objBlobInfo)
		File.WriteAllText(Path.Combine(dir, objBlobInfo.ParentObjId + ".json"), json)

		For Each blobInfo In objBlobInfo.BlobsInfo
			Dim fname = Path.Combine(dir, blobInfo.BlobId)
			If (File.Exists(fname)) Then
				File.Delete(fname)
			End If
			Dim fileStream = New FileStream(fname, FileMode.CreateNew)
			fileStream.Write(blobInfo.Data, 0, blobInfo.Data.Length)
			fileStream.Close()
			fileStream.Dispose()
		Next
	End Sub

	Sub Remove(parentObjId As String) Implements IBlobSaver.Remove
		Dim dir = GetPath(parentObjId)
		If Directory.Exists(dir) Then
			Directory.Delete(dir, True)
		End If

		Try
			Dim parentDir = Directory.GetParent(dir).FullName
			If (Not Directory.GetFiles(parentDir).Any) And (Not Directory.GetDirectories(parentDir).Any) Then
				Directory.Delete(parentDir)
			End If
		Catch ex As Exception
			'...
		End Try
	End Sub
End Class
