Public Interface IBlobFiledsStorage
    ReadOnly Property BinaryConverters As IEnumerable(Of IBinaryConverter)
    ReadOnly Property BlobWriters As IEnumerable(Of IBlobFieldsWriter)
    Function SaveBlobs(parentObject As Object, parentId As String) As Boolean
    Function SaveBlobs(parentObjects As Object(), parentIds As String()) As Boolean
    Function LoadBlobs(parentObject As Object, parentId As String) As Boolean
    Function LoadBlobs(parentObjects As Object(), parentIds As String()) As Boolean
    Sub Remove(Id As String)
End Interface
