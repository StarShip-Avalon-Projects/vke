<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class cbMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.outputTextBox = New System.Windows.Forms.TextBox()
        Me.getReplyButton = New System.Windows.Forms.Button()
        Me.inputTextBox = New System.Windows.Forms.TextBox()
        Me.openFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.mainMenu = New System.Windows.Forms.MainMenu(Me.components)
        Me.fileMenuItem = New System.Windows.Forms.MenuItem()
        Me.loadMenuItem = New System.Windows.Forms.MenuItem()
        Me.separatorMenuItem1 = New System.Windows.Forms.MenuItem()
        Me.exitMenuItem = New System.Windows.Forms.MenuItem()
        Me.helpMenuItem = New System.Windows.Forms.MenuItem()
        Me.aboutMenuItem = New System.Windows.Forms.MenuItem()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.SuspendLayout
        '
        'outputTextBox
        '
        Me.outputTextBox.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.outputTextBox.Location = New System.Drawing.Point(0, 38)
        Me.outputTextBox.Multiline = true
        Me.outputTextBox.Name = "outputTextBox"
        Me.outputTextBox.Size = New System.Drawing.Size(363, 247)
        Me.outputTextBox.TabIndex = 8
        '
        'getReplyButton
        '
        Me.getReplyButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.getReplyButton.Location = New System.Drawing.Point(304, 12)
        Me.getReplyButton.Name = "getReplyButton"
        Me.getReplyButton.Size = New System.Drawing.Size(48, 20)
        Me.getReplyButton.TabIndex = 7
        Me.getReplyButton.Text = "Send"
        '
        'inputTextBox
        '
        Me.inputTextBox.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.inputTextBox.Location = New System.Drawing.Point(48, 12)
        Me.inputTextBox.Name = "inputTextBox"
        Me.inputTextBox.Size = New System.Drawing.Size(250, 20)
        Me.inputTextBox.TabIndex = 6
        '
        'openFileDialog1
        '
        Me.openFileDialog1.DefaultExt = "vkb"
        Me.openFileDialog1.FileName = "Default"
        Me.openFileDialog1.Filter = "Knowedgebase files | *.vkb"
        '
        'mainMenu
        '
        Me.mainMenu.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.fileMenuItem, Me.helpMenuItem})
        '
        'fileMenuItem
        '
        Me.fileMenuItem.Index = 0
        Me.fileMenuItem.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.loadMenuItem, Me.separatorMenuItem1, Me.exitMenuItem})
        Me.fileMenuItem.Text = "File"
        '
        'loadMenuItem
        '
        Me.loadMenuItem.Index = 0
        Me.loadMenuItem.Text = "Load..."
        '
        'separatorMenuItem1
        '
        Me.separatorMenuItem1.Index = 1
        Me.separatorMenuItem1.Text = "-"
        '
        'exitMenuItem
        '
        Me.exitMenuItem.Index = 2
        Me.exitMenuItem.Text = "Exit"
        '
        'helpMenuItem
        '
        Me.helpMenuItem.Index = 1
        Me.helpMenuItem.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.aboutMenuItem})
        Me.helpMenuItem.Text = "Help"
        '
        'aboutMenuItem
        '
        Me.aboutMenuItem.Index = 0
        Me.aboutMenuItem.Text = "About"
        '
        'Label2
        '
        Me.Label2.AutoSize = true
        Me.Label2.Location = New System.Drawing.Point(8, 16)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(34, 13)
        Me.Label2.TabIndex = 9
        Me.Label2.Text = "Input:"
        '
        'cbMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(364, 297)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.outputTextBox)
        Me.Controls.Add(Me.getReplyButton)
        Me.Controls.Add(Me.inputTextBox)
        Me.Menu = Me.mainMenu
        Me.Name = "cbMain"
        Me.Text = "Form1"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Private WithEvents outputTextBox As System.Windows.Forms.TextBox
    Private WithEvents getReplyButton As System.Windows.Forms.Button
    Private WithEvents inputTextBox As System.Windows.Forms.TextBox
    Private WithEvents openFileDialog1 As System.Windows.Forms.OpenFileDialog
    Private WithEvents mainMenu As System.Windows.Forms.MainMenu
    Private WithEvents fileMenuItem As System.Windows.Forms.MenuItem
    Private WithEvents loadMenuItem As System.Windows.Forms.MenuItem
    Private WithEvents separatorMenuItem1 As System.Windows.Forms.MenuItem
    Private WithEvents exitMenuItem As System.Windows.Forms.MenuItem
    Private WithEvents helpMenuItem As System.Windows.Forms.MenuItem
    Private WithEvents aboutMenuItem As System.Windows.Forms.MenuItem
    Friend WithEvents Label2 As System.Windows.Forms.Label

End Class
