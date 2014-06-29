Imports Bwl.Storage.UniversalORM.Blob
Imports System.Text

Public Class ObjDataInfo
	Public Property ObjInfo As ObjInfo
	Public Property ObjBlobInfo As ObjBlobInfo

	Public Function GetFilesForWeb() As IEnumerable(Of Byte())
		Dim files = New List(Of Byte())

		Dim webObjInoJson = CfJsonConverter.Serialize(ObjInfo)
		files.Add(Encoding.UTF8.GetBytes(webObjInoJson))

		Dim ObjBlobInfoJson = CfJsonConverter.Serialize(ObjBlobInfo)
		files.Add(Encoding.UTF8.GetBytes(ObjBlobInfoJson))

		For Each b In ObjBlobInfo.BlobsInfo
			files.Add(b.Data)
		Next

		Return files
	End Function

	Public Shared Function GetFromFiles(files As IEnumerable(Of Byte())) As ObjDataInfo
		Dim str1 = Encoding.UTF8.GetString(files(0))
		Dim objDataInfo = New ObjDataInfo
		objDataInfo.ObjInfo = CfJsonConverter.Deserialize(Of ObjInfo)(str1)

		Dim str2 = Encoding.UTF8.GetString(files(1))
		objDataInfo.ObjBlobInfo = CfJsonConverter.Deserialize(Of ObjBlobInfo)(str2)

		Dim i = 2
		For Each bi In objDataInfo.ObjBlobInfo.BlobsInfo
			bi.Data = files(i)
			i += 1
		Next
		Return objDataInfo
	End Function

	Public Function GetOneFileForWeb() As Byte()
		Dim files = GetFilesForWeb()
		Dim res = New List(Of Byte)
		Dim filesCount = Convert.ToByte(files.Count)
		res.Add(filesCount)
		For Each f In files
			Dim fLen = BitConverter.GetBytes(Convert.ToInt32(f.Length))
			res.AddRange(fLen)
			res.AddRange(f)
		Next
		Return res.ToArray
	End Function

	Public Shared Function GetFromOneFile(oneFile As Byte()) As ObjDataInfo
		Dim files = New List(Of Byte())
		Dim pos = 0
		Dim filesCount = oneFile.First
		pos = 1

		For i = 0 To filesCount - 1
			Dim fLenB(4) As Byte
			Array.ConstrainedCopy(oneFile, pos, fLenB, 0, 4)
			pos += 4
			Dim fLen = BitConverter.ToInt32(fLenB, 0)
			Dim f(fLen - 1) As Byte
			Array.ConstrainedCopy(oneFile, pos, f, 0, fLen)
			pos += fLen

			files.Add(f)
		Next

		Return GetFromFiles(files)
	End Function

End Class
