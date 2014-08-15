Imports FirebirdSql.Data.FirebirdClient

Public Class SqlReaderHelper

	Public Sub New(reader As FbDataReader, cmd As FbCommand, conn As FbConnection)
		Me.Cmd = cmd
		Me.Connection = conn
		Me.Reader = reader
	End Sub

	Public Property Reader As FbDataReader
	Private Property Cmd As FbCommand
	Private Property Connection As FbConnection

	Public Sub Close()
		Reader.Close()
		Cmd.Dispose()
		Connection.Close()
		Connection.Dispose()
	End Sub

	Public Function GetObjectList() As List(Of List(Of Object))
		Dim mainList = New List(Of List(Of Object))

		If (Reader.HasRows) Then
			While (Reader.Read)
				Dim list = New List(Of Object)
				For i As Integer = 0 To Reader.FieldCount - 1
					list.Add(Reader(i))
				Next
				mainList.Add(list)
			End While
		End If
		Return mainList
	End Function
End Class
