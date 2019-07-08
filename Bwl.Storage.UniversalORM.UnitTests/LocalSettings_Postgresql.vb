Imports System.Data.SqlClient
Imports Bwl.Framework
Imports Npgsql

Public Class LocalSettings_Postgresql
    Private ReadOnly _settings As SettingsStorage
    Private _connStrBld As NpgsqlConnectionStringBuilder

    Public Sub New(settings As SettingsStorage)
        _settings = settings

        '!!!!!!!!!!!!!!!!!!!!!
        '  параметры подключения тут не изменять
        '  их надо изменять в конфиг файле
        '!!!!!!!!!!!!!!!!!!!!!

        CreateFbEmbedBld()
    End Sub

    Private Sub CreateFbEmbedBld()
        Try

            Dim dbStorage = _settings.CreateChildStorage("DB_Postgres")

            Dim hostSetting = dbStorage.CreateStringSetting("DBHostSetting", "localhost")
            Dim portSetting = dbStorage.CreateIntegerSetting("DBPortSetting", 5433)
            Dim dbSetting = dbStorage.CreateStringSetting("DBDatabaseName", "OrlanWeb")
            Dim userSetting = dbStorage.CreateStringSetting("DBUserSetting", "postgres")
            Dim passSetting = dbStorage.CreateStringSetting("DBPassSetting", "password")
            Dim dbTimeout = dbStorage.CreateIntegerSetting("dbTimeout", 10)

            _connStrBld = New NpgsqlConnectionStringBuilder()
            _connStrBld.Host = hostSetting.Value
            _connStrBld.Port = portSetting.Value
            _connStrBld.Database = dbSetting.Value
            _connStrBld.Username = userSetting.Value
            _connStrBld.Password = passSetting.Value
            _connStrBld.Timeout = dbTimeout.Value
        Catch ex As Exception
            Console.WriteLine(ex.ToString())
        End Try
    End Sub

    Public ReadOnly Property ConnectionStringBuilder As NpgsqlConnectionStringBuilder
        Get
            Return _connStrBld
        End Get
    End Property

End Class
