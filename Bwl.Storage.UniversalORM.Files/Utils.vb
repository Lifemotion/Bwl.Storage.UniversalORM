Public Class Utils

    Private Shared _sep As String = IO.Path.DirectorySeparatorChar
    Public Shared ReadOnly Property Sep As String
        Get
            Return _sep
        End Get
    End Property

    Public Shared Sub TestFolder(path As String)
        If Not IO.Directory.Exists(path) Then IO.Directory.CreateDirectory(path)
        IO.File.WriteAllText(path + _sep + "testfile.fsm", "testfile")
    End Sub

    Public Shared ReadOnly Property Enc As System.Text.Encoding
        Get
            Return System.Text.Encoding.UTF8
        End Get
    End Property
End Class
