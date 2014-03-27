Imports System.Data.SqlClient
Imports System.Data

Public Class MSSQLSRVStorageManager
	Implements IObjStorageManager

	Private _connStringBld As SqlConnectionStringBuilder
	Private _dbName As String

	Public Sub New(connStringBld As SqlConnectionStringBuilder)
		_connStringBld = connStringBld
		_dbName = _connStringBld.InitialCatalog
	End Sub

	Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage(Of T) Implements IObjStorageManager.CreateStorage
		Dim tableName = String.Format("{0}_main", name)
		MSSQLSRVUtils.CreateDB(_connStringBld, _dbName)
		Return New MSSQLSRVStorage(Of T)(ConnectionStringBuilder.ConnectionString)
	End Function

	Public Property ConnectionStringBuilder As SqlConnectionStringBuilder
		Get
			Return _connStringBld
		End Get
		Set(value As SqlConnectionStringBuilder)
			_connStringBld = value
		End Set
	End Property

	Private Shared _createMainTableSQL As String = My.Resources.CreateMainTableSQL

End Class
