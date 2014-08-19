Imports System.Data.SqlClient
Imports System.Data

Public Class MSSQLSRVStorageManager
	Implements IObjStorageManager

	Private Shared _createMainTableSQL As String = My.Resources.CreateMainTableSQL
	Private _connStringBld As SqlConnectionStringBuilder
	Private _dbName As String
	Private _defaultDB As String = "DefaultDB"
	Private _dictDB As Dictionary(Of String, String)

	Public Sub New(connStringBld As SqlConnectionStringBuilder)
		_connStringBld = connStringBld
		_dbName = _connStringBld.InitialCatalog
		_dictDB = New Dictionary(Of String, String)()
	End Sub

	Public Property ConnectionStringBuilder As SqlConnectionStringBuilder
		Get
			Return _connStringBld
		End Get
		Set(value As SqlConnectionStringBuilder)
			_connStringBld = value
		End Set
	End Property

	Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
		Return CreateStorage(name, GetType(T))
	End Function

	Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
		'Dim tableName = String.Format("{0}_main", name)
		_dbName = GetDB(type)
		'ConnectionStringBuilder.InitialCatalog = _dbName
		Return New MSSQLSRVStorage(ConnectionStringBuilder, type, _dbName)
	End Function

	Public Sub AddType(t As Type, nameDB As String)
		_dictDB.Add(t.ToString(), nameDB)
	End Sub

	Private Function GetDB(t As Type) As String
		Try
			Dim db = _dictDB.Item(t.ToString)
			If db IsNot Nothing Then
				Return db
			End If
		Catch exc As Exception
			Return _defaultDB
		End Try
		Return _defaultDB
	End Function
End Class
