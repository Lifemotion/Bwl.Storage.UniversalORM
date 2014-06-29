Public Class CfJsonConverter
	Public Shared Function Serialize(obj As Object) As String
		Dim stringData = ""
		If obj IsNot Nothing Then
			stringData = JsonConvert.SerializeObject(obj, Formatting.Indented)
		End If
		Return stringData
	End Function

	Public Shared Function Deserialize(Of T)(data As String) As T
		Dim obj As T = Nothing
		If Not String.IsNullOrWhiteSpace(data) Then
			obj = JsonConvert.DeserializeObject(Of T)(data)
		End If
		Return obj
	End Function

	Public Shared Function Deserialize(data As String, type As Type) As Object
		Dim obj = Nothing
		If Not String.IsNullOrWhiteSpace(data) Then
			obj = JsonConvert.DeserializeObject(data, type)
		End If
		Return obj
	End Function
End Class
