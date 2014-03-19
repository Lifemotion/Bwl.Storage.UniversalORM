Public Class JsonConverter
    Public Shared Function Serialize(obj As ObjBase) As String
        Dim stringData = JsonConvert.SerializeObject(obj, Formatting.Indented)
        Return stringData
    End Function

    Public Shared Function Deserialize(Of T)(data As String) As T
        Dim obj As T = JsonConvert.DeserializeObject(Of T)(data)
        Return obj
    End Function
End Class
