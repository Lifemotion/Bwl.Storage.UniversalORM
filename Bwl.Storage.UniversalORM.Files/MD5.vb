Public Class MD5
    Private Shared _md5 As System.Security.Cryptography.MD5 = System.Security.Cryptography.MD5.Create
    Public Shared Function GetHash(str As String) As String
        Dim bytes = Utils.Enc.GetBytes(str)
        Dim hash = System.Convert.ToBase64String(_md5.ComputeHash(bytes)).Replace("/", "-")
        Return hash
    End Function
End Class
