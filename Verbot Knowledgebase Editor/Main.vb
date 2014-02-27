Imports Conversive.Verbot5
Imports System.IO

Public Class Main
    Private verbot As Verbot5Engine = New Verbot5Engine()
    Public kb As KnowledgeBase = New KnowledgeBase()
    Public CurrentRule As Rule = New Rule
    Public CurrentResourceFile As ResourceFile = New ResourceFile
    Dim KB_Changed As Boolean = False

    Private FileName As String = ""
    Private Sub OpenToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles OpenToolStripMenuItem.Click

        With OpenFileDialog1
            ' Select Character ini file

            If .ShowDialog = DialogResult.OK Then
                Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(KnowledgeBase))
                kb = xToolbox.LoadXML(.FileName)
                FileName = .FileName
                ListBox1.Items.Clear()
                ListBox2.Items.Clear()
                ListBox3.Items.Clear()
                ListBox4.Items.Clear()
                For i As Integer = 0 To kb.Rules.Count - 1
                    If String.IsNullOrEmpty(kb.Rules.Item(i).Name) Then
                        ListBox3.Items.Add(kb.Rules.Item(i).Id)
                    Else
                        ListBox3.Items.Add(kb.Rules.Item(i).Name)
                    End If
                Next
                For i As Integer = 0 To kb.ResourceFiles.Count - 1
                    ListBox4.Items.Add(kb.ResourceFiles.Item(i))
                Next
            End If
        End With
    End Sub

    Private Sub ListBox3_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ListBox3.SelectedIndexChanged
        If ListBox3.SelectedIndex = -1 Then Exit Sub
        CurrentRule = kb.Rules.Item(ListBox3.SelectedIndex)
        ListBox1.Items.Clear()
        For i As Integer = 0 To CurrentRule.Inputs.Count - 1
            ListBox1.Items.Add(CurrentRule.Inputs.Item(i).Text)
        Next
        ListBox2.Items.Clear()
        For i As Integer = 0 To CurrentRule.Outputs.Count - 1
            ListBox2.Items.Add(CurrentRule.Outputs.Item(i).Text)
        Next
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Dim IPW As New InputWindow()
        With IPW
            .ShowDialog()
            If .DialogResult = Windows.Forms.DialogResult.OK Then
                CurrentRule.AddInput(IPW._CurrentInput.Text, IPW._CurrentInput.Condition)
                ListBox1.Items.Add(CurrentRule.Inputs.Item(CurrentRule.Inputs.Count - 1).Text)
            End If
        End With
    End Sub


    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click, ListBox1.DoubleClick
        If IsNothing(CurrentRule) Or ListBox1.SelectedIndex = -1 Then Exit Sub
        Dim IPW As New InputWindow(CurrentRule.Inputs.Item(ListBox1.SelectedIndex))
        With IPW
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                CurrentRule.Inputs.Item(ListBox1.SelectedIndex) = IPW._CurrentInput
                ListBox1.Items.RemoveAt(ListBox1.SelectedIndex)
                ListBox1.Items.Insert(ListBox1.SelectedIndex + 1, IPW._CurrentInput.Text)
            End If
        End With
    End Sub

    Private Sub SaveToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SaveToolStripMenuItem.Click

        Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(KnowledgeBase))
        xToolbox.SaveXML(kb, FileName)

    End Sub

    Private Sub SaveAsToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SaveAsToolStripMenuItem.Click
        With SaveFileDialog1
            ' Select Character ini file

            If .ShowDialog = DialogResult.OK Then
                Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(KnowledgeBase))
                FileName = .FileName
                xToolbox.SaveXML(kb, .FileName)
            End If
        End With
    End Sub

    Private Sub Button10_Click(sender As System.Object, e As System.EventArgs) Handles Button10.Click
        Dim vRule As Rule = New Rule()
        vRule.Id = kb.GetNewRuleId()
        vRule.Name = TextBox1.Text
        ListBox3.Items.Add(TextBox1.Text)
        TextBox1.Text = ""
        kb.Rules.Add(vRule)
    End Sub

    Private Sub Button6_Click(sender As System.Object, e As System.EventArgs) Handles Button6.Click
        Dim IPW As New OutputWindow()
        With IPW

            If .ShowDialog() = Windows.Forms.DialogResult.OK Then
                CurrentRule.AddOutput(IPW._CurrentOutput.Text, IPW._CurrentOutput.Condition, IPW._CurrentOutput.Cmd)
                ListBox2.Items.Add(CurrentRule.Outputs.Item(CurrentRule.Outputs.Count - 1).Text)
            End If
        End With
    End Sub


    Private Sub TextBox1_KeyPress(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles TextBox1.KeyDown
        Dim t As TextBox = CType(sender, TextBox)

        If e.KeyCode = Keys.Enter Then
            e.Handled = True
            e.SuppressKeyPress = True
            Dim vRule As Rule = New Rule()
            vRule.Id = kb.GetNewRuleId()
            vRule.Name = t.Text
            ListBox3.Items.Add(TextBox1.Text)
            t.Text = ""
            kb.Rules.Add(vRule)

        End If
    End Sub

    Private Sub Button7_Click(sender As System.Object, e As System.EventArgs) Handles Button7.Click
        If ListBox3.SelectedIndex = -1 Then Exit Sub
        CurrentRule.Name = TextBox1.Text
        ListBox3.Items.RemoveAt(ListBox3.SelectedIndex)
        ListBox3.Items.Insert(ListBox3.SelectedIndex + 1, TextBox1.Text)
    End Sub

    Private Sub Button9_Click(sender As System.Object, e As System.EventArgs) Handles Button9.Click
        If ListBox3.SelectedIndex = -1 Then Exit Sub
        CurrentRule = Nothing
        ListBox2.Items.Clear()
        ListBox1.Items.Clear()
        kb.Rules.RemoveAt(ListBox3.SelectedIndex)
        ListBox3.Items.RemoveAt(ListBox3.SelectedIndex)
    End Sub

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        If ListBox2.SelectedIndex = -1 Then Exit Sub
        CurrentRule.Outputs.RemoveAt(ListBox2.SelectedIndex)
        ListBox2.Items.RemoveAt(ListBox2.SelectedIndex)
    End Sub

    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        If ListBox1.SelectedIndex = -1 Then Exit Sub
        CurrentRule.Inputs.RemoveAt(ListBox1.SelectedIndex)
        ListBox1.Items.RemoveAt(ListBox1.SelectedIndex)
    End Sub

    Private Sub Button11_Click(sender As System.Object, e As System.EventArgs) Handles Button11.Click
        With OpenFileDialog2
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                Dim I As ResourceFile = New ResourceFile
                I.Filename = Path.GetFileName(.FileName)
                I.Filetype = ResourceFileType.ReplacementProfileFile
                kb.ResourceFiles.Add(I)
                ListBox4.Items.Add(Path.GetFileName(.FileName))
            End If
        End With
    End Sub



    Private Sub NewReplacementProfileToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles NewReplacementProfileToolStripMenuItem.Click
        With ReplacementEditor

            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                CurrentResourceFile = New ResourceFile

                CurrentResourceFile.Filename = .FileName
                CurrentResourceFile.Filetype = ResourceFileType.ReplacementProfileFile
                kb.ResourceFiles.Add(CurrentResourceFile)
                ListBox4.Items.Add(.FileName)
            End If
        End With
    End Sub

    Private Sub ListBox4_DoubleClick(sender As Object, e As System.EventArgs) Handles ListBox4.DoubleClick
        With ReplacementEditor
            .FilePath = Path.GetDirectoryName(FileName)
            .FileName = ListBox4.SelectedItem.ToString
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                CurrentResourceFile = kb.ResourceFiles.Item(ListBox4.SelectedIndex)
                CurrentResourceFile.Filename = Path.GetFileName(.FileName)
                Select Case Path.GetExtension(.FileName).ToLower
                    Case ".rpp"
                        CurrentResourceFile.Filetype = ResourceFileType.ReplacementProfileFile
                End Select


            End If
        End With
    End Sub

    Private Sub Button12_Click(sender As System.Object, e As System.EventArgs) Handles Button12.Click
        If ListBox4.SelectedIndex = -1 Then Exit Sub
        kb.ResourceFiles.RemoveAt(ListBox4.SelectedIndex)
        ListBox4.Items.RemoveAt(ListBox4.SelectedIndex)
    End Sub
End Class