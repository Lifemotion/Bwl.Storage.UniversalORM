Imports System.Data.SQLite

Public Class SqliteStorageManager
    Implements IObjStorageManager

    Public Property ConnectionStringBuilder As SQLiteConnectionStringBuilder

    Public Sub New(connStringBld As SQLiteConnectionStringBuilder)
        _ConnectionStringBuilder = connStringBld
    End Sub


    Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage Implements IObjStorageManager.CreateStorage
        Return CreateStorage(name, GetType(T))
    End Function

    Public Function CreateStorage(name As String, type As Type) As IObjStorage Implements IObjStorageManager.CreateStorage
        Return New SqliteStorage(ConnectionStringBuilder, type)
    End Function
End Class
