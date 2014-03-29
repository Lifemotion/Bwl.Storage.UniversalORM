Imports Bwl.Storage.UniversalORM.Blob
Imports System.Text

Public Class ObjDataInfo
	Public Property ObjInfo As ObjInfo
	Public Property ObjBlobInfo As ObjBlobInfo

	Public Function GetFilesForWeb() As IEnumerable(Of Byte())
		Dim files = New List(Of Byte())

		Dim webObjInoJson = JsonUtils.ToJson(ObjInfo)
		files.Add(Encoding.UTF8.GetBytes(webObjInoJson))

		Dim ObjBlobInfoJson = JsonUtils.ToJson(ObjBlobInfo)
		files.Add(Encoding.UTF8.GetBytes(ObjBlobInfoJson))

		For Each b In ObjBlobInfo.BlobsInfo
			files.Add(b.Data)
		Next

		Return files
	End Function

	Public Shared Function GetFromFiles(files As IEnumerable(Of Byte())) As ObjDataInfo
		Dim str1 = Encoding.UTF8.GetString(files(0))
		Dim objDataInfo = New ObjDataInfo
		objDataInfo.ObjInfo = JsonConverter.Deserialize(Of ObjInfo)(str1)

		Dim str2 = Encoding.UTF8.GetString(files(1))
		objDataInfo.ObjBlobInfo = JsonConverter.Deserialize(Of ObjBlobInfo)(str2)

		Dim i = 2
		For Each bi In objDataInfo.ObjBlobInfo.BlobsInfo
			bi.Data = files(i)
			i += 1
		Next
		Return objDataInfo
	End Function

End Class
