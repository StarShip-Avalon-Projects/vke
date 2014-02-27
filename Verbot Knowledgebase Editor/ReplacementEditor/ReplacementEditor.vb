Imports System.Windows.Forms
Imports System.IO
Imports Conversive.Verbot5

Public Class ReplacementEditor
    Public FileName As String
    Public FilePath As String
    Public Resource As ReplacementProfile = New ReplacementProfile

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        If Not File.Exists(FilePath + "\" + FileName) Then
            With SaveFileDialog1
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    FileName = Path.GetFileName(.FileName)
                    FilePath = Path.GetDirectoryName(.FileName)
                Else
                    Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
                    Me.Close()
                    Exit Sub
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
        If File.Exists(FilePath + "\" + FileName) Then
            TextBox1.Text = FilePath + "\" + FileName
            Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(ReplacementProfile))
            Resource = xToolbox.LoadXML(FilePath + "\" + FileName)

            For i As Integer = 0 To Resource.InputReplacements.Count - 1
                Dim item As ListViewItem = ListView2.Items.Add(i.ToString)
                item.SubItems.Add(Resource.InputReplacements.Item(i).TextToFind)
                item.SubItems.Add(Resource.InputReplacements.Item(i).TextToInput)
            Next
            For i As Integer = 0 To Resource.Replacements.Count - 1
                Dim item As ListViewItem = ListView1.Items.Add(i.ToString)
                item.SubItems.Add(Resource.Replacements.Item(i).TextToFind)
                item.SubItems.Add(Resource.Replacements.Item(i).TextForOutput)
                item.SubItems.Add(Resource.Replacements.Item(i).TextForAgent)
            Next
        End If
    End Sub

    Private Sub Button7_Click(sender As System.Object, e As System.EventArgs) Handles Button7.Click
        With SaveFileDialog1
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                FileName = Path.GetFileName(.FileName)
                FilePath = Path.GetDirectoryName(.FileName)
                TextBox1.Text = .FileName

            End If
        End With
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Dim ip As New InputReplacements
        With ip
            If .ShowDialog() = Windows.Forms.DialogResult.OK Then
                Resource.InputReplacements.Add(.IP)
                Dim item As ListViewItem = ListView2.Items.Add((ListView2.Items.Count + 1).ToString)
                item.SubItems.Add(.IP.TextToFind)
                item.SubItems.Add(.IP.TextToInput)
            End If
        End With
    End Sub

    Private Sub Button6_Click(sender As System.Object, e As System.EventArgs) Handles Button6.Click
        Dim OP As New OutputReplacements
        With OP
            If .ShowDialog() = Windows.Forms.DialogResult.OK Then
                Resource.Replacements.Add(.OP)
                Dim item As ListViewItem = ListView1.Items.Add((ListView1.Items.Count + 1).ToString)
                item.SubItems.Add(.OP.TextToFind)
                item.SubItems.Add(.OP.TextForOutput)
                item.SubItems.Add(.OP.TextForAgent)
            End If
        End With
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click

        Dim ip As New InputReplacements(Resource.InputReplacements.Item(ListView2.FocusedItem.Index))
        With ip
            If .ShowDialog() = Windows.Forms.DialogResult.OK Then
                Resource.InputReplacements.Add(.IP)
                Dim item As ListViewItem = ListView2.Items.Add((ListView2.Items.Count + 1).ToString)
                item.SubItems.Add(.IP.TextToFind)
                item.SubItems.Add(.IP.TextToInput)
            End If
        End With

    End Sub

    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        Dim OP As New OutputReplacements(Resource.Replacements.Item(ListView2.FocusedItem.Index))
        With OP
            If .ShowDialog() = Windows.Forms.DialogResult.OK Then
                Resource.Replacements.Add(.OP)
                Dim item As ListViewItem = ListView1.Items.Add((ListView1.FocusedItem.Index).ToString)
                item.SubItems.Add(.OP.TextToFind)
                item.SubItems.Add(.OP.TextForOutput)
                item.SubItems.Add(.OP.TextForAgent)
            End If
        End With
    End Sub

    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        Resource.InputReplacements.RemoveAt(ListView2.FocusedItem.Index)
        ListView2.Items.RemoveAt(ListView2.FocusedItem.Index)
    End Sub

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        Resource.Replacements.RemoveAt(ListView1.FocusedItem.Index)
        ListView1.Items.RemoveAt(ListView1.FocusedItem.Index)
    End Sub
End Class
