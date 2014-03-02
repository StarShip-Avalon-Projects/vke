Imports System.Windows.Forms
Imports Conversive.Verbot5

Public Class OutputWindow
    Public _CurrentOutput As Output
    Sub New()
        InitializeComponent()
        _CurrentOutput = New Output
        _CurrentOutput.Id = Main.CurrentRule.GetNewOutputId
    End Sub

    'Edit Rule
    Sub New(ByRef CurrentOutput As Output)
        InitializeComponent()
        _CurrentOutput = CurrentOutput
    End Sub
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        _CurrentOutput.Text = TextBox1.Text
        _CurrentOutput.Condition = TextBox2.Text
        _CurrentOutput.Cmd = TextBox3.Text
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub OutputWindow_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        TextBox1.Text = _CurrentOutput.Text
        TextBox2.Text = _CurrentOutput.Condition
        TextBox3.Text = _CurrentOutput.Cmd
    End Sub
End Class
