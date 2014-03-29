Imports System.IO

''' <summary>
''' Модуль для сохранения/загрузки потоков в реальное хранилище (файл, БД и т.д.)
''' </summary>
''' <remarks></remarks>
Public Interface IBlobSaver
	Sub Save(objBlobInfo As ObjBlobInfo)
	Function Load(parentObjId As String) As ObjBlobInfo
	Sub Remove(parentObjId As String)
End Interface
