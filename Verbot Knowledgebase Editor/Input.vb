Imports System.Windows.Forms
Imports Conversive.Verbot5

Public Class InputWindow
    Public _CurrentInput As Input
    'New Rule
    Sub New()
        InitializeComponent()
        _CurrentInput = New Input
        _CurrentInput.Id = Main.CurrentRule.GetNewInputId
    End Sub

    'Edit Rule
    Sub New(ByRef CurrentInput As Input)
        InitializeComponent()
        _CurrentInput = CurrentInput
    End Sub



    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        _CurrentInput.Text = TextBox1.Text
        _CurrentInput.Condition = TextBox2.Text
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub Input_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        TextBox1.Text = _CurrentInput.Text
        TextBox2.Text = _CurrentInput.Condition
    End Sub
End Class
