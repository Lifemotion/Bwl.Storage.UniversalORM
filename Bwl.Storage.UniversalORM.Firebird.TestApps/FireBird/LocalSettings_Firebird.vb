Imports System.Data.SqlClient
Imports Bwl.Framework
Imports FirebirdSql.Data.FirebirdClient

Public Class LocalSettings_Firebird
	Private ReadOnly _settings As SettingsStorage
	Private ReadOnly _connStrBld_embed As FbConnectionStringBuilder
	Private ReadOnly _connStrBld_service As FbConnectionStringBuilder

	Public Sub New(settings As SettingsStorage)
		_settings = settings

		'!!!!!!!!!!!!!!!!!!!!!
		'  параметры подключения тут не изменять
		'  их надо изменять в конфиг файле
		'!!!!!!!!!!!!!!!!!!!!!

        Dim pathSep = IO.Path.DirectorySeparatorChar

		Dim dbStorage = _settings.CreateChildStorage("DB_FB")

		Dim userSetting = dbStorage.CreateStringSetting("DBUserSetting", "sysdba")
		Dim passSetting = dbStorage.CreateStringSetting("DBPassSetting", "masterkey")
		Dim dialectSetting = dbStorage.CreateIntegerSetting("DialectSetting", 3)

        Dim DBPathDef_embed = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".." + pathSep + "data" + pathSep + "TestDB_EMBEDDED.fdb")
		Dim databaseSetting_embed = dbStorage.CreateStringSetting("DatabaseSetting_Embed", DBPathDef_embed)

        Dim DBPathDef_service = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".." + pathSep + "data" + pathSep + "TestDB_service.fdb")
		Dim databaseSetting_service = dbStorage.CreateStringSetting("DatabaseSetting_Service", DBPathDef_service)

		Dim dbTimeout = dbStorage.CreateIntegerSetting("dbTimeout", 1)
        Dim clientDllPathDef = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fbe32" + pathSep + "fbembed.dll")
		Dim clientDllPathSetting = dbStorage.CreateStringSetting("clientDllPathSetting", clientDllPathDef)

		_connStrBld_embed = New FbConnectionStringBuilder()
		_connStrBld_embed.Database = databaseSetting_embed.Value
		_connStrBld_embed.ServerType = FbServerType.Embedded
		_connStrBld_embed.UserID = userSetting.Value
		_connStrBld_embed.Password = passSetting.Value
		_connStrBld_embed.Dialect = dialectSetting.Value
		_connStrBld_embed.ConnectionTimeout = dbTimeout.Value
		_connStrBld_embed.ClientLibrary = clientDllPathSetting.Value

		_connStrBld_service = New FbConnectionStringBuilder()
		_connStrBld_service.Database = databaseSetting_service.Value
		_connStrBld_service.UserID = userSetting.Value
		_connStrBld_service.Password = passSetting.Value
		_connStrBld_service.Dialect = dialectSetting.Value
		_connStrBld_service.ConnectionTimeout = dbTimeout.Value
		_connStrBld_service.ClientLibrary = clientDllPathSetting.Value
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
