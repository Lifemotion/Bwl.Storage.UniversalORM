Imports System.Reflection
Imports System.Linq

Public Class ReflectionTools

	Public Shared Function GetBLOBMemberNames(type As System.Type, obj As Object) As Dictionary(Of String, Object)
		Return GetBLOBMemberNamesRecursive("", obj)
	End Function

	Private Shared Function GetBLOBMemberNamesRecursive(path As String, obj As Object) As Dictionary(Of String, Object)
		Dim members = obj.GetType.GetMembers()
		Dim results As New Dictionary(Of String, Object)
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
				Dim value = GetMemberValue(item.Name, obj)
				If item.GetCustomAttributes(GetType(BlobContainerAttribute), True).Length > 0 Then
					If (itemType.GetInterface(GetType(IEnumerable).Name) IsNot Nothing) AndAlso (Not itemType.IsAbstract) Then
						'просматриваем массивы и списки
						If obj IsNot Nothing Then
							If value IsNot Nothing Then
								Dim list = CType(value, IEnumerable)
								Dim i = 0
								For Each listItem In list
									If listItem IsNot Nothing Then
										Dim listItemType = listItem.GetType
										Dim nestedResults = GetBLOBMemberNamesRecursive(path + item.Name + ";" + i.ToString + ".", listItem)
										If nestedResults.Count > 0 Then
											For Each pair In nestedResults
												results(pair.Key) = pair.Value
											Next
										End If
									End If
									i += 1
								Next
							End If
						End If
					Else
						'просматриваем объекты
						If value IsNot Nothing Then
							itemType = value.GetType
						End If
						Dim nestedResults = GetBLOBMemberNamesRecursive(path + item.Name + ".", value)
						If nestedResults.Count > 0 Then
							For Each pair In nestedResults
								results(pair.Key) = pair.Value
							Next
						End If
					End If
				End If


				If item.GetCustomAttributes(GetType(BlobAttribute), True).Length > 0 Then
					results(path + item.Name) = value
				End If
			End If
		Next
		Return results
	End Function


	Shared Sub SetMemberValue(fieldPath As String, obj As Object, value As Object)
		Dim fieldParts = fieldPath.Split("."c)

		If fieldParts.Length = 1 Then
			Dim members = obj.GetType.GetMember(fieldParts(0))
			If members.Length > 0 Then
				If members(0).MemberType = MemberTypes.Property Then
					DirectCast(members(0), PropertyInfo).SetValue(obj, value, Nothing)
				ElseIf members(0).MemberType = MemberTypes.Field Then
					DirectCast(members(0), FieldInfo).SetValue(obj, value)
				End If
			Else
				Throw New Exception("Member not found: " + fieldParts(0))
			End If
		Else
			Dim curNameMass = fieldParts(0).Split(";"c)
			Dim current = curNameMass(0)
			Dim members = obj.GetType.GetMember(current)

			Dim path = fieldParts(1)
			For i = 2 To fieldParts.Length - 1
				path += "." + fieldParts(i)
			Next

			If members.Any Then
				For Each member In members
					If member.MemberType = MemberTypes.Property Then
						obj = DirectCast(member, PropertyInfo).GetValue(obj, Nothing)
						If obj Is Nothing Then Throw New Exception("Value is nothing not found: " + member.Name)
						If curNameMass.Length = 2 Then
							Dim iEnum = CType(obj, IEnumerable(Of Object))
							obj = iEnum.ElementAt(Convert.ToInt32(curNameMass(1)))
						End If
						SetMemberValue(path, obj, value)
					End If
					If member.MemberType = MemberTypes.Field Then
						obj = DirectCast(member, FieldInfo).GetValue(obj)
						If obj Is Nothing Then Throw New Exception("Value is nothing not found: " + member.Name)
						SetMemberValue(path, obj, value)
					End If
				Next
			Else
				Throw New Exception("Member not found: " + current)
			End If
		End If
	End Sub

	Private Shared Function GetMemberValue(fieldPath As String, obj As Object) As Object
		Dim fieldParts = fieldPath.Split("."c)

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

End Class
