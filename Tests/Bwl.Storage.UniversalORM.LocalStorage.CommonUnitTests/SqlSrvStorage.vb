Imports System.Data.SqlClient
Imports Bwl.Framework
Imports System.IO
Imports Bwl.Storage.UniversalORM.Blob

Public Class SqlSrvStorage
	Private ReadOnly _localStorage As LocalStorage
	Private ReadOnly _conStrBld As SqlConnectionStringBuilder
	Private ReadOnly _settingsStorage As SettingsStorage
	Private ReadOnly _logger As Logger

	Public Sub New(settingsStorage As SettingsStorage, logger As Logger)

		'!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		'ВАЖНО: тут параметры подключения менять нельзя
		'их нужно настраивать в конфиг файле, который создается в папке с приложением после первого запуска
		' так у каждого разработчика будет свой конфиг со своими настройками
		'!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

		_settingsStorage = settingsStorage
		_logger = logger
		Dim dbStorage = settingsStorage.CreateChildStorage("DB")

		Dim userSetting = dbStorage.CreateStringSetting("DBUserSetting", "sa")
		Dim passSetting = dbStorage.CreateStringSetting("DBPassSetting", "123")

		Dim dsSetting = dbStorage.CreateStringSetting("DBDataSourceSetting", "(local)")
		Dim dbNameSetting = dbStorage.CreateStringSetting("DBNameSetting", "HitonDB_test")
		Dim dbIntegratedSeqSetting = dbStorage.CreateBooleanSetting("dbIntegratedSeqSetting", False)
		Dim dbTimeout = dbStorage.CreateIntegerSetting("dbTimeout", 1)

		Dim conStrBld = New SqlConnectionStringBuilder()
		conStrBld.InitialCatalog = dbNameSetting.Value
		conStrBld.UserID = userSetting.Value
		conStrBld.Password = passSetting.Value
		conStrBld.DataSource = dsSetting.Value
		conStrBld.IntegratedSecurity = dbIntegratedSeqSetting.Value
		conStrBld.ConnectTimeout = dbTimeout.Value

		Dim blobDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BlobData")
		Dim blobDirSetting = _settingsStorage.CreateStringSetting("BlobDir", blobDir, "Путь к папке с blob данными", "Требуется перезапуск программы")

		_localStorage = New LocalStorage(New AdoDb.MSSQLSRVStorageManager(conStrBld), New FileBlobSaver(blobDirSetting.Value))
		_conStrBld = conStrBld
	End Sub

	Public ReadOnly Property Storage As LocalStorage
		Get
			Return _localStorage
		End Get
	End Property

End Class