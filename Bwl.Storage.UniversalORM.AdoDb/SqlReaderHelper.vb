Imports System.Data.SqlClient

Public Class SqlReaderHelper

	Public Sub New(reader As SqlDataReader, cmd As SqlCommand, conn As SqlConnection)
		Me.Cmd = cmd
		Me.Connection = conn
		Me.Reader = reader
	End Sub

	Public Property Reader As SqlDataReader
	Private Property Cmd As SqlCommand
	Private Property Connection As SqlConnection

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
