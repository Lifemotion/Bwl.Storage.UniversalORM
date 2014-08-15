Imports FirebirdSql.Data.FirebirdClient
Imports System.Data

Public Class FbStorageManager
	Implements IObjStorageManager

	Private Shared _createMainTableSQL As String = My.Resources.CreateMainTableSQL
	Private _connStringBld As FbConnectionStringBuilder
	Private _dbName As String

	Public Sub New(connStringBld As FbConnectionStringBuilder)
		_connStringBld = connStringBld
		'_dbName = _connStringBld.InitialCatalog
		_dbName = connStringBld.Database
	End Sub

	Public Property ConnectionStringBuilder As FbConnectionStringBuilder
		Get
			Return _connStringBld
		End Get
		Set(value As FbConnectionStringBuilder)
			_connStringBld = value
		End Set
	End Property

	Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
		Return CreateStorage(name, GetType(T))
	End Function

	Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
		'Dim tableName = String.Format("{0}_main", name)
		Return New FBStorage(ConnectionStringBuilder, type, _dbName)
	End Function
End Class
