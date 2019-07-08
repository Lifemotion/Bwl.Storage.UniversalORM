Imports Npgsql

Public Class PgStorageManager
    Implements IObjStorageManager

    Private _connStringBld As NpgsqlConnectionStringBuilder
    Private _dbName As String

    Public Sub New(connStringBld As NpgsqlConnectionStringBuilder)
        _connStringBld = connStringBld
        _dbName = connStringBld.Database
    End Sub

    Public Sub New(databaseName As String)
        _connStringBld = New NpgsqlConnectionStringBuilder()

        _connStringBld.Host = "localhost"
        _connStringBld.Database = databaseName
        _connStringBld.Username = "postgres"
        _connStringBld.Password = "password"
        _connStringBld.Timeout = 1

        _dbName = _connStringBld.Database
    End Sub

    Public Property ConnectionStringBuilder As NpgsqlConnectionStringBuilder
        Get
            Return _connStringBld
        End Get
        Set(value As NpgsqlConnectionStringBuilder)
            _connStringBld = value
        End Set
    End Property

    Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
        Return CreateStorage(name, GetType(T))
    End Function

    Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
        Return New PgStorage(ConnectionStringBuilder, type, _dbName)
    End Function
End Class
