Public Class JsonConverter
	Public Shared Function Serialize(obj As Object) As String
		Dim stringData = JsonConvert.SerializeObject(obj, Formatting.Indented)
		Return stringData
	End Function

    Public Shared Function Deserialize(Of T)(data As String) As T
        Dim obj As T = JsonConvert.DeserializeObject(Of T)(data)
        Return obj
	End Function

	Public Shared Function Deserialize(data As String, type As Type) As Object
		Return JsonConvert.DeserializeObject(data, type)
	End Function
End Class
