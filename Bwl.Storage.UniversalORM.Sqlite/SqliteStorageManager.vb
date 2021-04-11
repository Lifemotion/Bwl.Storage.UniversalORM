Imports System.Data.SQLite

Public Class SqliteStorageManager
    Implements IObjStorageManager

    Private _connStringBld As SQLiteConnectionStringBuilder
    Private _dbPath As String

    Public Sub New(connStringBld As SQLiteConnectionStringBuilder)
        _connStringBld = connStringBld
        _dbPath = connStringBld.DataSource
    End Sub

    Public Sub New(dbPath As String)
        _connStringBld = New SQLiteConnectionStringBuilder()

        _connStringBld.DataSource = dbPath
        _connStringBld.Version = 3
        _connStringBld.Password = "password"
        _connStringBld.BinaryGUID = False
        _connStringBld.DateTimeFormat = SQLiteDateFormats.Ticks

        ' Для ускорения работы - write-ahead и общий кэш
        _connStringBld.JournalMode = SQLiteJournalModeEnum.Wal
        _connStringBld.Add("cache", "shared")

        _dbPath = _connStringBld.DataSource
    End Sub

    Public Property ConnectionStringBuilder As SQLiteConnectionStringBuilder
        Get
            Return _connStringBld
        End Get
        Set(value As SQLiteConnectionStringBuilder)
            _connStringBld = value
        End Set
    End Property

    Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
        Return CreateStorage(name, GetType(T))
    End Function

    Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
        Return New SqliteStorage(ConnectionStringBuilder, type, _dbPath)
    End Function
End Class
