Imports FirebirdSql.Data.FirebirdClient
Imports System.Data

Public Class FbStorageManager
	Implements IObjStorageManager

	Private _connStringBld As FbConnectionStringBuilder
	Private _dbName As String

	Public Sub New(connStringBld As FbConnectionStringBuilder)
		_connStringBld = connStringBld
		_dbName = connStringBld.Database
    End Sub

    Public Sub New(databaseFilePath As String)
        _connStringBld = New FbConnectionStringBuilder()

        _connStringBld.Database = databaseFilePath
        _connStringBld.UserID = "sysdba"
        _connStringBld.Password = "masterkey"
        _connStringBld.Dialect = 3
        _connStringBld.ServerType = FbServerType.Embedded
        _connStringBld.ConnectionTimeout = 1
        _connStringBld.ClientLibrary = "fbe32\fbembed.dll"

        _dbName = _connStringBld.Database
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
		Return New FBStorage(ConnectionStringBuilder, type, _dbName)
	End Function
End Class
