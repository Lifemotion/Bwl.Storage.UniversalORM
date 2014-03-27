Imports System.Reflection

Public Class ReflectionTools

	Public Shared Function GetIndexingMemberNames(type As System.Type) As IndexInfo()
		Return GetIndexingMemberNamesRecursive("", type)
	End Function

	Private Shared Function GetIndexingMemberNamesRecursive(path As String, type As System.Type) As IndexInfo()
		Dim members = type.GetMembers()
		Dim results As New List(Of IndexInfo)
		For Each item In members

			Dim itemType As System.Type = Nothing
			If item.MemberType = Reflection.MemberTypes.Property Or item.MemberType = Reflection.MemberTypes.Field Then
				If item.MemberType = Reflection.MemberTypes.Property Then
					Dim itemProperty As PropertyInfo = DirectCast(item, Reflection.PropertyInfo)
					itemType = itemProperty.PropertyType
				End If
				If item.MemberType = Reflection.MemberTypes.Field Then
					Dim itemField As FieldInfo = DirectCast(item, Reflection.FieldInfo)
					itemType = itemField.FieldType
				End If

				Dim indexAttributes = item.GetCustomAttributes(GetType(Indexing), True)
				If (indexAttributes.Any) Then
					Dim indAttr = CType(indexAttributes.First, Indexing)
					Dim nestedResults = GetIndexingMemberNamesRecursive(path + item.Name + ".", itemType)

					If nestedResults.Length > 0 Then
						results.AddRange(nestedResults)
					Else
						Dim inInfo = New IndexInfo(path + item.Name, itemType, indAttr.Length)
						results.Add(inInfo)
					End If
				End If
			End If
		Next
		Return results.ToArray
	End Function


    Shared Function GetMemberValue(fieldPath As String, obj As Object) As Object
        Dim fieldParts = fieldPath.Split(".")

        If fieldParts.Length = 1 Then
            Dim members = obj.GetType.GetMember(fieldParts(0))
            If members.Length > 0 Then
                If members(0).MemberType = MemberTypes.Property Then
					Dim value = DirectCast(members(0), PropertyInfo).GetValue(obj, Nothing)
                    Return value
                End If
                If members(0).MemberType = MemberTypes.Field Then
                    Dim value = DirectCast(members(0), FieldInfo).GetValue(obj)
                    Return value
                End If
                Throw New Exception
            End If
            Throw New Exception("Member not found: " + fieldParts(0))
        Else
            Dim current = fieldParts(0)
            Dim members = obj.GetType.GetMember(current)

            Dim path = fieldParts(1)
            For i = 2 To fieldParts.Length - 1
                path += "." + fieldParts(i)
            Next

            For Each member In members
                If member.MemberType = MemberTypes.Property Then
					Dim value = DirectCast(member, PropertyInfo).GetValue(obj, Nothing)
                    If value Is Nothing Then Throw New Exception("Value is nothing not found: " + member.Name)
                    Dim nested = GetMemberValue(path, value)
                    Return nested
                End If
                If member.MemberType = MemberTypes.Field Then
                    Dim value = DirectCast(member, FieldInfo).GetValue(obj)
                    Dim nested = GetMemberValue(path, value)
                    Return nested
                End If
            Next
            Throw New Exception("Member not found: " + current)
        End If
    End Function

    Shared Function GetMember(fieldPath As String, obj As Object) As Object
        Dim fieldParts = fieldPath.Split(".")

        If fieldParts.Length = 1 Then
            Dim members = obj.GetType.GetMember(fieldParts(0))
            If members.Length > 0 Then Return members(0)
            Throw New Exception("Member not found: " + fieldParts(0))
        Else
            Dim current = fieldParts(0)
            Dim members = obj.GetType.GetMember(current)

            Dim path = fieldParts(1)
            For i = 2 To fieldParts.Length - 1
                path += "." + fieldParts(i)
            Next

            For Each member In members
                If member.MemberType = MemberTypes.Property Then
					Dim value = DirectCast(member, PropertyInfo).GetValue(obj, Nothing)
                    If value Is Nothing Then Throw New Exception("Value is nothing not found: " + member.Name)
                    Dim nested = GetMember(path, value)
                    Return nested
                End If
                If member.MemberType = MemberTypes.Field Then
                    Dim value = DirectCast(member, FieldInfo).GetValue(obj)
                    Dim nested = GetMember(path, value)
                    Return nested
                End If
            Next
            Throw New Exception("Member not found: " + current)
        End If
    End Function

End Class
