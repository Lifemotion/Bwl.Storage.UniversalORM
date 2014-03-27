Public Class FileStorageManager
    Implements IObjStorageManager

    Private _folder As String

    Public Sub New(folder As String)
        _folder = folder
        Utils.TestFolder(_folder)
    End Sub

	Public Function CreateStorage(Of T As ObjBase)(name As String) As IObjStorage(Of T) Implements IObjStorageManager.CreateStorage
		Dim path = _folder + Utils.Sep + name
		Utils.TestFolder(path)
		Dim stor As New FileObjStorage(Of T)(path)
		Return stor
	End Function

	Public Property FileStorageDir As String
		Get
			Return _folder
		End Get
		Set(value As String)
			_folder = value
		End Set
	End Property
End Class
