﻿Public Interface IBlobStorage
	ReadOnly Property BlobStreamSavers As IEnumerable(Of IBlobStreamSaver)
	ReadOnly Property BlobSavers As IEnumerable(Of IBlobSaver)
	Function SaveBlobs(parentObject As Object, parentId As String) As Boolean
	Function LoadBlobs(parentObject As Object, parentId As String) As Boolean
	Sub Remove(Id As String)
End Interface
