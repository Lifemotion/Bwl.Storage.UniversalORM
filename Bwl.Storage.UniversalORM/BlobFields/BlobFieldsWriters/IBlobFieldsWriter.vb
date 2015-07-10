''' <summary>
''' Модуль для сохранения/загрузки потоков в реальное хранилище (файл, БД и т.д.)
''' </summary>
''' <remarks></remarks>
Public Interface IBlobFieldsWriter
    Sub Save(objBlobInfo As BlobFieldsSet)
    Function Load(parentObjId As String) As BlobFieldsSet
    Sub Remove(parentObjId As String)
End Interface
