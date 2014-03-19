Public Class UORM
    Private manager As New FileStorageManager(Application.StartupPath + "\..\data")
    Private stor As FileObjStorage(Of TestData) = manager.CreateStorage(Of TestData)("TestDataStor")
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        stor.Add(New TestData)
        Dim f = stor.FindObj({})
        For Each ff In f
            stor.Remove(ff)
        Next
    End Sub
End Class
