Imports System.Data.SqlClient
Imports System.Data

Public Class MSSQLSRVStorageManager
	Implements IObjStorageManager

	Private ReadOnly _connStringBld As SqlConnectionStringBuilder
	Private ReadOnly _defaultDB As String
	Private ReadOnly _dictDB As Dictionary(Of String, String)

	Public Sub New(connStringBld As SqlConnectionStringBuilder)
		_connStringBld = connStringBld
		_defaultDB = _connStringBld.InitialCatalog
		_dictDB = New Dictionary(Of String, String)()
	End Sub

	Public ReadOnly Property ConnectionStringBuilder As SqlConnectionStringBuilder
		Get
			Return _connStringBld
		End Get
	End Property

	Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
		Return CreateStorage(name, GetType(T))
	End Function

	Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
		Dim dbName = GetDB(type)
		Return New MSSQLSRVStorage(ConnectionStringBuilder, type, dbName)
	End Function

	Public Function SetDbForType(t As Type, nameDB As String) As Boolean
		Dim res = True
		If t IsNot Nothing AndAlso Not String.IsNullOrEmpty(nameDB) Then
			SyncLock (_dictDB)
				_dictDB(t.ToString()) = nameDB
			End SyncLock
		Else
			res = False
		End If
		Return res
	End Function

	Public Function GetDB(t As Type) As String
		Dim dbName = _defaultDB
		If t IsNot Nothing Then
			SyncLock (_dictDB)
				If _dictDB.ContainsKey(t.ToString) Then
					dbName = _dictDB(t.ToString)
				End If
			End SyncLock
		End If
		Return dbName
	End Function
End Class
