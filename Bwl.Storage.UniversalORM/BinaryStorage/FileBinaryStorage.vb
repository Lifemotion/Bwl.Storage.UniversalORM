Imports System.IO

Public Class FileBinaryStorage
    Implements IBinaryStorage

    Private _rootDir As String

    Public Sub New(rootDir As String)
        _rootDir = rootDir
    End Sub

    Public ReadOnly Property RootDir As String
        Get
            Return _rootDir
        End Get
    End Property

    Public Function GetPath(blobId As String) As String
        If blobId Is Nothing Then Throw New ArgumentNullException("id")
        Dim subDir = blobId
        If subDir.Length > 4 Then subDir = blobId.Substring(1, 3)
        Dim dir = Path.Combine(Path.Combine(_rootDir, subDir), blobId)
        Return dir
    End Function

    Private Function GetObjectFilename(id As String) As String
        Dim dir = GetPath(id)
        If Directory.Exists(dir) = False Then Throw New Exception("IBinaryObject with id not exists: " + id)
        Dim files = Directory.GetFiles(dir)
        If files.Length = 0 Then Throw New Exception("GetObjectFilename: blob dir without files: " + dir)
        If files.Length > 1 Then Throw New Exception("GetObjectFilename: blob dir with >1 file: " + dir)
        Return Path.Combine(dir, files(0))
    End Function

    Public Function Load(id As String) As Byte() Implements IBinaryStorage.Load
        Dim bytes = File.ReadAllBytes(GetObjectFilename(id))
        Return bytes
    End Function

    Public Sub Load(id As String, binaryObject As IBinaryObject) Implements IBinaryStorage.Load
        If binaryObject Is Nothing Then Throw New Exception("Load: IBinaryObject is nothing")
        Dim bytes = GetObjectFilename(id)
        binaryObject.Name = Path.GetFileName(bytes)
        binaryObject.Bytes = File.ReadAllBytes(bytes)
    End Sub

    Public Sub Save(binaryObject As IBinaryObject) Implements IBinaryStorage.Save
        Save(binaryObject.ID, binaryObject.Name, binaryObject.Bytes)
    End Sub

    Public Sub Save(id As String, name As String, data() As Byte) Implements IBinaryStorage.Save
        Dim dir = GetPath(id)
        If data Is Nothing Then data = {}
        If name Is Nothing OrElse name.Length = 0 Then name = "bytes"
        If Directory.Exists(dir) Then Throw New Exception("Save: IBinaryObject with id already exists: " + id)
        Directory.CreateDirectory(dir)
        IO.File.WriteAllBytes(Path.Combine(dir, name), data)
    End Sub

    Public Function Exists(id As String) As Boolean Implements IBinaryStorage.Exists
        Dim dir = GetPath(id)
        Return Directory.Exists(dir)
    End Function

    Public Sub Delete(id As String) Implements IBinaryStorage.Delete
        Dim dir = GetPath(id)
        If Exists(id) Then Directory.Delete(dir, True)
    End Sub
End Class
