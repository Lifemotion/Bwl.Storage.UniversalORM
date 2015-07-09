Imports Newtonsoft.Json

Public Class AdoDbTestData
    Implements ObjBase
    <Indexing> Public Property Cat As String = "fffg"
    Public Property Dog As Integer = 4
    <Indexing> Public Property Int As New TestDataInternal
    Public Property ID As String = "fff" Implements ObjBase.ID
End Class

Public Class TestDataInternal
	Public Property First As String = "fffg"
	<Indexing> Public Property Second As Integer = 4

	<JsonIgnore>
	Public Property SomeData As String
End Class

Public Class TestData2
	Implements ObjBase
	<Indexing> Public Property F1 As String = "qwert"
	Public Property F2 As Integer = 111
	Public Property ID As String = "ididididdi" Implements ObjBase.ID
End Class