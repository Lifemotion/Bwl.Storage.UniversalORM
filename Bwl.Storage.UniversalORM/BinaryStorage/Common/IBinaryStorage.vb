Public Interface IBinaryStorage
    Sub Save(id As String, name As String, bytes As Byte())
    Sub Save(binaryObject As IBinaryObject)
    Function Load(id As String) As Byte()
    Sub Load(id As String, binaryObject As IBinaryObject)
    Sub Delete(id As String)
    Function Exists(id As String) As Boolean
End Interface
