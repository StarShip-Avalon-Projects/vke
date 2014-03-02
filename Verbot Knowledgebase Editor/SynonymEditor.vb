
Imports Conversive.Verbot5
Imports System.IO
Public Class SynonymEditor
    Public FilePath As String
    Public Synonyms As SynonymGroup = New SynonymGroup

    Private s As Synonym

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Public Sub New(ByRef FileName As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        FilePath = FileName
    End Sub



    Private Sub Button8_Click(sender As System.Object, e As System.EventArgs) Handles Button8.Click
        If String.IsNullOrEmpty(TextBox3.Text) Then Exit Sub
        Synonyms.AddSynonym(TextBox3.Text)
        ListBox2.Items.Add(TextBox3.Text)
        TextBox3.Text = ""

    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        If ListBox2.SelectedIndex = -1 Then Exit Sub
        s = Synonyms.Synonyms.Item(ListBox2.SelectedIndex)
        s.AddPhrase(TextBox1.Text)
        ListBox1.Items.Add(TextBox1.Text)
        TextBox1.Text = ""
    End Sub

    Private Sub SaveToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SaveToolStripMenuItem.Click
        With SaveFileDialog1

            If .ShowDialog() = Windows.Forms.DialogResult.OK Then
                Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(SynonymGroup))
                xToolbox.SaveXML(Synonyms, .FileName)
                FilePath = .FileName
            End If
        End With
    End Sub

    Private Sub SynonymEditor_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        If File.Exists(FilePath) Then
            Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(SynonymGroup))
            Synonyms = xToolbox.LoadXML(FilePath)
            For I As Integer = 0 To Synonyms.Synonyms.Count - 1
                ListBox2.Items.Add(Synonyms.Synonyms.Item(I).Name)
            Next
        End If
    End Sub

    Private Sub ListBox2_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ListBox2.SelectedIndexChanged
        If ListBox2.SelectedIndex = -1 Then Exit Sub

        s = Synonyms.Synonyms.Item(ListBox2.SelectedIndex)
        ListBox1.Items.Clear()
        For i As Integer = 0 To s.Phrases.Count - 1
            ListBox1.Items.Add(s.Phrases.Item(i).Text)
        Next
    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles OpenToolStripMenuItem.Click
        With OpenFileDialog1

            If .ShowDialog() = Windows.Forms.DialogResult.OK Then

                FilePath = .FileName
                Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(SynonymGroup))
                Synonyms = xToolbox.LoadXML(.FileName)
                For I As Integer = 0 To Synonyms.Synonyms.Count - 1
                    ListBox2.Items.Add(Synonyms.Synonyms.Item(I).Name)
                Next
            End If
        End With
    End Sub
End Class