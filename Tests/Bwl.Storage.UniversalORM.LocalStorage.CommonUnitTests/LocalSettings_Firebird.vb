Imports System.Data.SqlClient
Imports Bwl.Framework
Imports FirebirdSql.Data.FirebirdClient

Public Class LocalSettings_Firebird
	Private ReadOnly _settings As SettingsStorage
	Private _connStrBld_embed As FbConnectionStringBuilder
	Private _connStrBld_service As FbConnectionStringBuilder

	Public Sub New(settings As SettingsStorage)
		_settings = settings

		'!!!!!!!!!!!!!!!!!!!!!
		'  параметры подключения тут не изменять
		'  их надо изменять в конфиг файле
		'!!!!!!!!!!!!!!!!!!!!!

		CreateFbEmbedBld()
		CreateFbServiceBld()
	End Sub

	Private Sub CreateFbEmbedBld()
		Dim dbStorage_embed = _settings.CreateChildStorage("DB_FB_embed")

		Dim userSetting = dbStorage_embed.CreateStringSetting("DBUserSetting", "sysdba")
		Dim passSetting = dbStorage_embed.CreateStringSetting("DBPassSetting", "masterkey")
		Dim dialectSetting = dbStorage_embed.CreateIntegerSetting("DialectSetting", 3)

		Dim DBPathDef_embed = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\data\TestDB_EMBEDDED.fdb")
		Dim databaseSetting_embed = dbStorage_embed.CreateStringSetting("DatabaseSetting_Embed", DBPathDef_embed)

		Dim dbTimeout = dbStorage_embed.CreateIntegerSetting("dbTimeout", 1)
		Dim clientDllPathDef = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fbe32\fbembed.dll")
		Dim clientDllPathSetting = dbStorage_embed.CreateStringSetting("clientDllPathSetting", clientDllPathDef)

		_connStrBld_embed = New FbConnectionStringBuilder()
		_connStrBld_embed.Database = databaseSetting_embed.Value
		_connStrBld_embed.ServerType = FbServerType.Embedded
        _connStrBld_embed.UserID = userSetting.Value
        _connStrBld_embed.Password = passSetting.Value
        _connStrBld_embed.Dialect = dialectSetting.Value
        _connStrBld_embed.ConnectionTimeout = dbTimeout.Value
        _connStrBld_embed.ClientLibrary = clientDllPathSetting.Value
    End Sub

    Private Sub CreateFbServiceBld()
        Dim dbStorage_service = _settings.CreateChildStorage("DB_FB_service")

        Dim userSetting = dbStorage_service.CreateStringSetting("DBUserSetting", "sysdba")
        Dim passSetting = dbStorage_service.CreateStringSetting("DBPassSetting", "masterkey")
        Dim dialectSetting = dbStorage_service.CreateIntegerSetting("DialectSetting", 3)

        Dim dataSourceSetting = dbStorage_service.CreateStringSetting("DataSourceSetting", "localhost")

        Dim DBPathDef_service = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\data\TestDB_service.fdb")
        Dim databaseSetting_service = dbStorage_service.CreateStringSetting("DatabaseSetting_Service", DBPathDef_service)

        Dim dbTimeout = dbStorage_service.CreateIntegerSetting("dbTimeout", 1)

        _connStrBld_service = New FbConnectionStringBuilder()
        _connStrBld_service.Database = databaseSetting_service.Value
        _connStrBld_service.UserID = userSetting.Value
        _connStrBld_service.Password = passSetting.Value
        _connStrBld_service.Dialect = dialectSetting.Value
        _connStrBld_service.ConnectionTimeout = dbTimeout.Value
        _connStrBld_service.DataSource = dataSourceSetting.Value
    End Sub

	Public ReadOnly Property ConnectionStringBuilder_Service As FbConnectionStringBuilder
		Get
			Return _connStrBld_service
		End Get
	End Property

	Public ReadOnly Property ConnectionStringBuilder_Embeded As FbConnectionStringBuilder
		Get
			Return _connStrBld_embed
		End Get
	End Property

End Class
