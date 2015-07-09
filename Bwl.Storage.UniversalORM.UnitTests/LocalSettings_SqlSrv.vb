Imports System.Data.SqlClient
Imports Bwl.Framework

Public Class LocalSettings_SqlSrv
	Private ReadOnly _settings As SettingsStorage
	Private ReadOnly _connStrBld As SqlConnectionStringBuilder

	Public Sub New(settings As SettingsStorage)
		_settings = settings

		'!!!!!!!!!!!!!!!!!!!!!
		'  параметры подключения тут не изменять
		'  их надо изменять в конфиг файле
		'!!!!!!!!!!!!!!!!!!!!!

		Dim dbStorage = _settings.CreateChildStorage("DB_SQLSRV")

		Dim userSetting = dbStorage.CreateStringSetting("DBUserSetting", "sa")
		Dim passSetting = dbStorage.CreateStringSetting("DBPassSetting", "123")

		Dim dsSetting = dbStorage.CreateStringSetting("DBDataSourceSetting", ".\SQLEXPRESS")
		Dim dbNameSetting = dbStorage.CreateStringSetting("DBNameSetting", "ORMTestDB")
		Dim dbIntegratedSeqSetting = dbStorage.CreateBooleanSetting("dbIntegratedSeqSetting", False)
		Dim dbTimeout = dbStorage.CreateIntegerSetting("dbTimeout", 1)

		_connStrBld = New SqlConnectionStringBuilder()
		_connStrBld.InitialCatalog = dbNameSetting.Value
		_connStrBld.UserID = userSetting.Value
		_connStrBld.Password = passSetting.Value
		_connStrBld.DataSource = dsSetting.Value
		_connStrBld.IntegratedSecurity = dbIntegratedSeqSetting.Value
		_connStrBld.ConnectTimeout = dbTimeout.Value
	End Sub

	Public ReadOnly Property SqlConnectionStringBuilder As SqlConnectionStringBuilder
		Get
			Return _connStrBld
		End Get
	End Property

End Class
