Imports Bwl.Framework
Imports Microsoft.Data.Sqlite

Public Class LocalSettings_Sqlite
    Private ReadOnly _settings As SettingsStorage
    Private _connStrBld As SqliteConnectionStringBuilder

    Public Sub New(settings As SettingsStorage, datapath As String)
        _settings = settings

        '!!!!!!!!!!!!!!!!!!!!!
        '  параметры подключения тут не изменять
        '  их надо изменять в конфиг файле
        '!!!!!!!!!!!!!!!!!!!!!

        CreateSqliteEmbedBld(datapath)
    End Sub

    Private Sub CreateSqliteEmbedBld(datapath As String)
        Try

            Dim dbStorage = _settings.CreateChildStorage("DB_Sqlite")
            Dim dbPathSetting = dbStorage.CreateStringSetting("DBDatabaseName", IO.Path.GetFullPath(IO.Path.Combine(datapath, "sqlite.db")))
            _connStrBld = New SqliteConnectionStringBuilder With {
                .DataSource = dbPathSetting
            }
            ' SQLite does not have password protection (but if needed it can be added with SQLCipher)
            'Dim passSetting = dbStorage.CreateStringSetting("DBPassSetting", "password")
            '_connStrBld.Password = passSetting.Value ' пароль

        Catch ex As Exception
            Console.WriteLine(ex.ToString())
        End Try
    End Sub

    Public ReadOnly Property ConnectionStringBuilder As SqliteConnectionStringBuilder
        Get
            Return _connStrBld
        End Get
    End Property

End Class
