Imports System.Windows.Forms
Imports System.IO
Imports Conversive.Verbot5

Public Class ReplacementEditor
    Public FileName As String
    Public Resource As ReplacementProfile = New ReplacementProfile

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        If Not File.Exists(FileName) Then
            With SaveFileDialog1
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    FileName = .FileName
                End If
            End With
        End If
        Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(ReplacementProfile))
        xToolbox.SaveXML(Resource, FileName)
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub ReplacementEditor_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        If File.Exists(FileName) Then
            TextBox1.Text = FileName
            Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(ReplacementProfile))
            Resource = xToolbox.LoadXML(FileName)

            For i As Integer = 0 To Resource.InputReplacements.Count - 1
                Dim item As ListViewItem = ListView2.Items.Add(i.ToString)
                item.SubItems.Add(Resource.InputReplacements.Item(i).TextToFind)
                item.SubItems.Add(item.SubItems.Add(Resource.InputReplacements.Item(i).TextToInput))
            Next
            For i As Integer = 0 To Resource.Replacements.Count - 1
                Dim item As ListViewItem = ListView1.Items.Add(i.ToString)
                item.SubItems.Add(Resource.Replacements.Item(i).TextToFind)
                item.SubItems.Add(item.SubItems.Add(Resource.Replacements.Item(i).TextForOutput))
                item.SubItems.Add(item.SubItems.Add(Resource.Replacements.Item(i).TextForAgent))
            Next
        End If
    End Sub
End Class
