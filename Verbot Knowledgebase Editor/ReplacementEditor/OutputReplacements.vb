Imports System.Windows.Forms
Imports Conversive.Verbot5

Public Class OutputReplacements

    Public OP As Replacement

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        OP = New Replacement
    End Sub
    Public Sub New(ByRef OPs As Replacement)
        OP = OPs
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        OP.TextToFind = TxtBxTextToFind.Text
        OP.TextForOutput = TxtBxTextForOutput.Text
        OP.TextForAgent = TxtBxReplacementText.Text
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub OutputReplacements_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        TxtBxTextToFind.Text = OP.TextToFind
        TxtBxTextForOutput.Text = OP.TextForOutput
        TxtBxReplacementText.Text = OP.TextForAgent
    End Sub
End Class
