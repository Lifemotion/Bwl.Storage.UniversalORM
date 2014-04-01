''' <summary> Если нужно не вносить в сериализацию, то надо использовать соответствующие атрибуты, отменяющие сериализацию поля </summary>
<AttributeUsageAttribute(AttributeTargets.Field Or AttributeTargets.Property)>
Public Class BlobAttribute
	Inherits Attribute
End Class
