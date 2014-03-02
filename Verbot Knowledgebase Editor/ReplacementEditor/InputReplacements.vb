Imports System.Windows.Forms
Imports Conversive.Verbot5

Public Class InputReplacements
    Public IP As InputReplacement = New InputReplacement

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Public Sub New(ByRef IPs As InputReplacement)
        IP = IPs
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        IP.TextToFind = TextBox1.Text
        IP.TextToInput = TextBox2.Text

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub InputReplacements_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        TextBox1.Text = IP.TextToFind
        TextBox2.Text = IP.TextToInput
    End Sub
End Class
