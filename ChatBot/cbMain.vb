Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Data
Imports Conversive.Verbot5
Imports System.IO

Public Class cbMain
    Inherits System.Windows.Forms.Form
    Private verbot As Verbot5Engine
    Private state As State
    Private stCKBFileFilter As String = "Verbot Knowledge Bases (*.vkb)|*.vkb"
    Private stFormName As String = "Verbot SDK Windows App Sample"
    Public kb As KnowledgeBase = New KnowledgeBase()
    Dim kbi As KnowledgeBaseItem = New KnowledgeBaseItem()
    Public Sub New()
        '
        ' Required for Windows Form Designer support
        '
        InitializeComponent()

        Me.verbot = New Verbot5Engine()
        Me.state = New State()

        Me.openFileDialog1.Filter = Me.stCKBFileFilter
        Me.Text = Me.stFormName
    End Sub

    Private Sub loadMenuItem_Click(sender As Object, e As System.EventArgs) Handles loadMenuItem.Click
        With Me.openFileDialog1
            If .ShowDialog() = DialogResult.OK Then
                Dim xToolbox As XMLToolbox = New XMLToolbox(GetType(KnowledgeBase))
                kb = xToolbox.LoadXML(.FileName)
                kbi.Filename = Path.GetFileName(.FileName)
                kbi.Fullpath = Path.GetDirectoryName(.FileName) + "\"
                verbot.AddKnowledgeBase(kb, kbi)
                state.CurrentKBs.Clear()
                state.CurrentKBs.Add(.FileName)
            End If
        End With
    End Sub

    Private Sub exitMenuItem_Click(sender As Object, e As System.EventArgs) Handles exitMenuItem.Click
        Me.Close()
    End Sub


    Private Sub inputTextBox_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles inputTextBox.KeyPress
        If e.KeyChar = ControlChars.Cr Then
            e.Handled = True
            Me.getReply()
        End If
    End Sub

    Private Sub getReplyButton_Click(sender As Object, e As System.EventArgs) Handles getReplyButton.Click
        Me.getReply()
    End Sub

    Private Sub getReply()
        Dim stInput As String = Me.inputTextBox.Text.Trim()
        Me.inputTextBox.Text = ""
        Dim reply As Reply = Me.verbot.GetReply(stInput, Me.state)
        If reply IsNot Nothing Then
            Me.outputTextBox.Text = reply.Text
            Me.parseEmbeddedOutputCommands(reply.AgentText)
            Me.runProgram(reply.Cmd)
        Else
            Me.outputTextBox.Text = "No reply found."
        End If
    End Sub

    Private Sub parseEmbeddedOutputCommands(text As String)
        Dim startCommand As String = "<"
        Dim endCommand As String = ">"

        Dim start As Integer = text.IndexOf(startCommand)
        Dim [end] As Integer = -1

        While start <> -1
            [end] = text.IndexOf(endCommand, start)
            If [end] <> -1 Then
                Dim command As String = text.Substring(start + 1, [end] - start - 1).Trim()
                If command <> "" Then
                    Me.runEmbeddedOutputCommand(command)
                End If
            End If
            start = text.IndexOf(startCommand, start + 1)
        End While
    End Sub
    'parseEmbeddedOutputCommands(string text)
    Private Sub runEmbeddedOutputCommand(command As String)
        Dim spaceIndex As Integer = command.IndexOf(" ")

        Dim [function] As String
        Dim args As String
        If spaceIndex = -1 Then
            [function] = command.ToLower()
            args = ""
        Else
            [function] = command.Substring(0, spaceIndex).ToLower()
            args = command.Substring(spaceIndex + 1)
        End If

        Try
            Select Case [function]
                Case "quit", "exit"

                    Me.Close()
                    Exit Select
                Case "run"
                    Me.runProgram(args)
                    Exit Select
                Case Else
                    Exit Select
                    'switch
            End Select
        Catch
        End Try
    End Sub
    'runOutputEmbeddedCommand(string command)
    Private Sub runProgram(command As String)
        Try
            Dim pieces As String() = Me.splitOnFirstUnquotedSpace(command)

            Dim proc As New System.Diagnostics.Process()
            proc.StartInfo.FileName = pieces(0).Trim()
            proc.StartInfo.Arguments = pieces(1)
            proc.StartInfo.CreateNoWindow = True
            proc.StartInfo.UseShellExecute = True
            proc.Start()
        Catch
        End Try
    End Sub
    'runProgram(string filename, string args)
    Public Function splitOnFirstUnquotedSpace(text As String) As String()
        Dim pieces As String() = New String(1) {}
        Dim index As Integer = -1
        Dim isQuoted As Boolean = False
        'find the first unquoted space
        For i As Integer = 0 To text.Length - 1
            If text(i) = """"c Then
                isQuoted = Not isQuoted
            ElseIf text(i) = " "c AndAlso Not isQuoted Then
                index = i
                Exit For
            End If
        Next

        'break up the string
        If index <> -1 Then
            pieces(0) = text.Substring(0, index)
            pieces(1) = text.Substring(index + 1)
        Else
            pieces(0) = text
            pieces(1) = ""
        End If

        Return pieces
    End Function
    'splitOnFirstUnquotedSpace(string text)

End Class
