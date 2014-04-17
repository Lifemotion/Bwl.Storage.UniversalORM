Public Class MD5
    Private Shared _md5 As System.Security.Cryptography.MD5 = System.Security.Cryptography.MD5.Create
    Public Shared Function GetHash(str As String) As String
		Dim bytes = System.Text.Encoding.UTF8.GetBytes(str)
        Dim hash = System.Convert.ToBase64String(_md5.ComputeHash(bytes)).Replace("/", "-")
        Return hash
	End Function

	'Public Shared Function GetHash(bytes As Byte()) As Byte()
	'	Return _md5.ComputeHash(bytes)
	'End Function

	Public Shared Function GetHashEx(str As String) As Byte()
		Dim bytes = System.Text.Encoding.UTF8.GetBytes(str)
		Return _md5.ComputeHash(bytes)
	End Function
End Class
