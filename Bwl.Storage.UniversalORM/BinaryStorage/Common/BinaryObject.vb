Public Class BinaryObject
    Implements IBinaryObject

    Public Property ID As String Implements ObjBase.ID
    Public Property Name As String Implements IBinaryObject.Name
    Public Property Bytes As Byte() Implements IBinaryObject.Bytes

    Public Sub New(id As String, bytes As Byte(), name As String)
        _ID = id
        _Bytes = bytes
        _Name = name
    End Sub

    Public Sub New(bytes As Byte(), name As String)
        Me.New(Guid.NewGuid.ToString("B"), bytes, name)
    End Sub

    Public Sub New(bytes As Byte())
        Me.New(Guid.NewGuid.ToString("B"), bytes, "binary")
    End Sub

    Public Sub New(id As String, bytes As Byte())
        Me.New(id, bytes, "binary")
    End Sub

    Public Sub New()
        Me.New(Guid.NewGuid.ToString("B"), {}, "binary")
    End Sub

End Class
