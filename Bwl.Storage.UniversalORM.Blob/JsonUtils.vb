Imports Newtonsoft.Json

Public Class JsonUtils
	Public Shared Function LoadFromJsonString(Of T)(json As String) As T
		Dim result = JsonConvert.DeserializeObject(Of T)(json)
		Return result
	End Function

	Public Shared Function ToJson(obj As Object) As String
		Dim stringData = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented)
		Return stringData
	End Function
End Class