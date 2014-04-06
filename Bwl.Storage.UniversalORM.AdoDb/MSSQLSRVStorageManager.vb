Imports System.Data.SqlClient
Imports System.Data

Public Class MSSQLSRVStorageManager
	Implements IObjStorageManager

	Private Shared _createMainTableSQL As String = My.Resources.CreateMainTableSQL
	Private _connStringBld As SqlConnectionStringBuilder
	Private _dbName As String

	Public Sub New(connStringBld As SqlConnectionStringBuilder)
		_connStringBld = connStringBld
		_dbName = _connStringBld.InitialCatalog
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
		Dim tableName = String.Format("{0}_main", name)
		MSSQLSRVUtils.CreateDB(_connStringBld, _dbName)
		Return New MSSQLSRVStorage(ConnectionStringBuilder.ConnectionString, type)
	End Function
End Class
