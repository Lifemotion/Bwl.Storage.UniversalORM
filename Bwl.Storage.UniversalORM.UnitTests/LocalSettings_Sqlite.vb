Imports Bwl.Framework
Imports System.Data.SQLite
Imports Newtonsoft.Json.Linq

Public Class LocalSettings_Sqlite
    Private ReadOnly _settings As SettingsStorage
    Private _connStrBld As SQLiteConnectionStringBuilder

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

            ' Dim hostSetting = dbStorage.CreateStringSetting("DBHostSetting", "localhost") База строго локальная
            ' Dim portSetting = dbStorage.CreateIntegerSetting("DBPortSetting", 5433)
            Dim dbPathSetting = dbStorage.CreateStringSetting("DBDatabaseName", IO.Path.GetFullPath(IO.Path.Combine(datapath, "sqlite.db")))
            ' Dim userSetting = dbStorage.CreateStringSetting("DBUserSetting", "postgres") Пользователь не нужен, достаточно пароля
            Dim passSetting = dbStorage.CreateStringSetting("DBPassSetting", "password")
            ' Dim dbTimeout = dbStorage.CreateIntegerSetting("dbTimeout", 10) Локальная база же

            _connStrBld = New SQLiteConnectionStringBuilder()
            _connStrBld.DataSource = dbPathSetting
            _connStrBld.Version = 3 ' строго задаётся последняя версия
            _connStrBld.Password = passSetting.Value ' пароль
            _connStrBld.BinaryGUID = False ' UniversalOrm рассчитан на текстовые GUIDы
            _connStrBld.DateTimeFormat = SQLiteDateFormats.Ticks ' Время по умолчанию храним в тиках

            '_connStrBld.Host = hostSetting.Value
            '_connStrBld.Port = portSetting.Value
            '_connStrBld.Database = dbPathSetting.Value
            '_connStrBld.Username = userSetting.Value
            '_connStrBld.Password = passSetting.Value
            ' _connStrBld.Timeout = dbTimeout.Value
        Catch ex As Exception
            Console.WriteLine(ex.ToString())
        End Try
    End Sub

    Public ReadOnly Property ConnectionStringBuilder As SQLiteConnectionStringBuilder
        Get
            Return _connStrBld
        End Get
    End Property

End Class
