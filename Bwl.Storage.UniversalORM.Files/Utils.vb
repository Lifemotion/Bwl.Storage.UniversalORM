Imports System.Threading

Public Class Utils

	Private Shared _sep As Char = IO.Path.DirectorySeparatorChar

	Private ReadOnly _locker As Object = New Object

	Public Shared ReadOnly Property Sep As Char
		Get
			Return _sep
		End Get
	End Property

	Public Shared Function TestFolderFsm(path As String) As Boolean
		Dim res = False
		SyncLock (path)
			Try
				If Not IO.Directory.Exists(path) Then IO.Directory.CreateDirectory(path)
				IO.File.WriteAllText(path + _sep + "testfile.fsm", "testfile")
				res = True
			Catch ex1 As Exception
				Thread.Sleep(100)
				Try
					If Not IO.Directory.Exists(path) Then IO.Directory.CreateDirectory(path)
					IO.File.WriteAllText(path + _sep + "testfile.fsm", "testfile")
					res = True
				Catch ex2 As Exception
					res = False
				End Try
			End Try
		End SyncLock
		Return res
	End Function

	Public Shared ReadOnly Property Enc As System.Text.Encoding
		Get
			Return System.Text.Encoding.UTF8
		End Get
	End Property
End Class
