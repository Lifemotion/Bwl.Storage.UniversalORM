<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ExamplesApp
    Inherits Bwl.Framework.FormAppBase

    'Форма переопределяет dispose для очистки списка компонентов.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Является обязательной для конструктора форм Windows Forms
    Private components As System.ComponentModel.IContainer

    'Примечание: следующая процедура является обязательной для конструктора форм Windows Forms
    'Для ее изменения используйте конструктор форм Windows Form.  
    'Не изменяйте ее в редакторе исходного кода.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.buttonFirebirdStorageExample = New System.Windows.Forms.Button()
        Me.buttonFileBinaryStorageExample = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'buttonFirebirdStorageExample
        '
        Me.buttonFirebirdStorageExample.Location = New System.Drawing.Point(12, 27)
        Me.buttonFirebirdStorageExample.Name = "buttonFirebirdStorageExample"
        Me.buttonFirebirdStorageExample.Size = New System.Drawing.Size(163, 23)
        Me.buttonFirebirdStorageExample.TabIndex = 2
        Me.buttonFirebirdStorageExample.Text = "Firebird Storage Example"
        Me.buttonFirebirdStorageExample.UseVisualStyleBackColor = True
        '
        'buttonFileBinaryStorageExample
        '
        Me.buttonFileBinaryStorageExample.Location = New System.Drawing.Point(181, 27)
        Me.buttonFileBinaryStorageExample.Name = "buttonFileBinaryStorageExample"
        Me.buttonFileBinaryStorageExample.Size = New System.Drawing.Size(163, 23)
        Me.buttonFileBinaryStorageExample.TabIndex = 3
        Me.buttonFileBinaryStorageExample.Text = "File Binary Storage Example"
        Me.buttonFileBinaryStorageExample.UseVisualStyleBackColor = True
        '
        'ExamplesApp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(784, 561)
        Me.Controls.Add(Me.buttonFileBinaryStorageExample)
        Me.Controls.Add(Me.buttonFirebirdStorageExample)
        Me.Name = "ExamplesApp"
        Me.Text = "Bwl.UniversalORM Examples"
        Me.Controls.SetChildIndex(Me.logWriter, 0)
        Me.Controls.SetChildIndex(Me.buttonFirebirdStorageExample, 0)
        Me.Controls.SetChildIndex(Me.buttonFileBinaryStorageExample, 0)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents buttonFirebirdStorageExample As System.Windows.Forms.Button
    Friend WithEvents buttonFileBinaryStorageExample As System.Windows.Forms.Button

End Class
