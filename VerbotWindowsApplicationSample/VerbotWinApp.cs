/*
	Copyright 2004-2006 Conversive, Inc.
	http://www.conversive.com
	3806 Cross Creek Rd., Unit F
	Malibu, CA 90265
 
	This file is part of Verbot 4 Library: a natural language processing engine.

    Verbot 4 Library is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    Verbot 4 Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Verbot 4 Library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
	
	Verbot 4 Library may also be available under other licenses.
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Conversive.Verbot5;

namespace VerbotWindowsApplicationSample
{
	/// <summary>
	/// Sample Windows application that uses the Verbot 4 Library.
	/// </summary>
	public class VerbotWinApp : System.Windows.Forms.Form
	{
		Verbot5Engine verbot;
		State state;
		string stCKBFileFilter = "Compiled Verbot Knowledge Bases (*.ckb)|*.ckb";
		string stFormName = "Verbot SDK Windows App Sample";

		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem fileMenuItem;
		private System.Windows.Forms.MenuItem helpMenuItem;
		private System.Windows.Forms.MenuItem aboutMenuItem;
		private System.Windows.Forms.MenuItem loadMenuItem;
		private System.Windows.Forms.MenuItem separatorMenuItem1;
		private System.Windows.Forms.MenuItem exitMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.TextBox inputTextBox;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Splitter mainSplitter;
		private System.Windows.Forms.TextBox outputTextBox;
		private System.Windows.Forms.Button getReplyButton;

		private System.ComponentModel.Container components = null;

		public VerbotWinApp()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.verbot = new Verbot5Engine();
			this.state = new State();

			this.openFileDialog1.Filter = this.stCKBFileFilter;
			this.Text = this.stFormName;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.fileMenuItem = new System.Windows.Forms.MenuItem();
			this.loadMenuItem = new System.Windows.Forms.MenuItem();
			this.separatorMenuItem1 = new System.Windows.Forms.MenuItem();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			this.helpMenuItem = new System.Windows.Forms.MenuItem();
			this.aboutMenuItem = new System.Windows.Forms.MenuItem();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.inputTextBox = new System.Windows.Forms.TextBox();
			this.topPanel = new System.Windows.Forms.Panel();
			this.getReplyButton = new System.Windows.Forms.Button();
			this.mainSplitter = new System.Windows.Forms.Splitter();
			this.outputTextBox = new System.Windows.Forms.TextBox();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.fileMenuItem,
																					 this.helpMenuItem});
			// 
			// fileMenuItem
			// 
			this.fileMenuItem.Index = 0;
			this.fileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.loadMenuItem,
																						 this.separatorMenuItem1,
																						 this.exitMenuItem});
			this.fileMenuItem.Text = "File";
			// 
			// loadMenuItem
			// 
			this.loadMenuItem.Index = 0;
			this.loadMenuItem.Text = "Load...";
			this.loadMenuItem.Click += new System.EventHandler(this.loadMenuItem_Click);
			// 
			// separatorMenuItem1
			// 
			this.separatorMenuItem1.Index = 1;
			this.separatorMenuItem1.Text = "-";
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Index = 2;
			this.exitMenuItem.Text = "Exit";
			this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
			// 
			// helpMenuItem
			// 
			this.helpMenuItem.Index = 1;
			this.helpMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.aboutMenuItem});
			this.helpMenuItem.Text = "Help";
			// 
			// aboutMenuItem
			// 
			this.aboutMenuItem.Index = 0;
			this.aboutMenuItem.Text = "About";
			this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
			// 
			// inputTextBox
			// 
			this.inputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.inputTextBox.Location = new System.Drawing.Point(8, 8);
			this.inputTextBox.Name = "inputTextBox";
			this.inputTextBox.Size = new System.Drawing.Size(216, 20);
			this.inputTextBox.TabIndex = 0;
			this.inputTextBox.Text = "";
			this.inputTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.inputTextBox_KeyPress);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.getReplyButton);
			this.topPanel.Controls.Add(this.inputTextBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = new System.Drawing.Size(292, 40);
			this.topPanel.TabIndex = 1;
			// 
			// getReplyButton
			// 
			this.getReplyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.getReplyButton.Location = new System.Drawing.Point(232, 8);
			this.getReplyButton.Name = "getReplyButton";
			this.getReplyButton.Size = new System.Drawing.Size(48, 23);
			this.getReplyButton.TabIndex = 1;
			this.getReplyButton.Text = "Send";
			this.getReplyButton.Click += new System.EventHandler(this.getReplyButton_Click);
			// 
			// mainSplitter
			// 
			this.mainSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.mainSplitter.Location = new System.Drawing.Point(0, 40);
			this.mainSplitter.Name = "mainSplitter";
			this.mainSplitter.Size = new System.Drawing.Size(292, 3);
			this.mainSplitter.TabIndex = 2;
			this.mainSplitter.TabStop = false;
			// 
			// outputTextBox
			// 
			this.outputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outputTextBox.Location = new System.Drawing.Point(0, 43);
			this.outputTextBox.Multiline = true;
			this.outputTextBox.Name = "outputTextBox";
			this.outputTextBox.Size = new System.Drawing.Size(292, 230);
			this.outputTextBox.TabIndex = 3;
			this.outputTextBox.Text = "";
			// 
			// VerbotWinApp
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Controls.Add(this.outputTextBox);
			this.Controls.Add(this.mainSplitter);
			this.Controls.Add(this.topPanel);
			this.Menu = this.mainMenu;
			this.Name = "VerbotWinApp";
			this.topPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new VerbotWinApp());
		}

		private void loadMenuItem_Click(object sender, System.EventArgs e)
		{
			if(this.openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				this.verbot.AddCompiledKnowledgeBase(this.openFileDialog1.FileName);
				this.state.CurrentKBs.Clear();
				this.state.CurrentKBs.Add(this.openFileDialog1.FileName);
			}
		}

		private void exitMenuItem_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void aboutMenuItem_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show(this, this.stFormName + "\r\nVersion: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void inputTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(e.KeyChar == '\r')
			{
				e.Handled = true;
				this.getReply();
			}
		}

		private void getReplyButton_Click(object sender, System.EventArgs e)
		{
			this.getReply();
		}

		private void getReply()
		{
			string stInput = this.inputTextBox.Text.Trim();
			this.inputTextBox.Text = "";
			Reply reply = this.verbot.GetReply(stInput, this.state);
			if(reply != null)
			{
				this.outputTextBox.Text = reply.Text;
				this.parseEmbeddedOutputCommands(reply.AgentText);
				this.runProgram(reply.Cmd);
			}
			else
				this.outputTextBox.Text = "No reply found.";
		}

		private void parseEmbeddedOutputCommands(string text)
		{
			string startCommand = "<";
			string endCommand = ">";

			int start = text.IndexOf(startCommand);
			int end = -1;

			while(start != -1)
			{
				end = text.IndexOf(endCommand, start);
				if(end != -1)
				{
					string command = text.Substring(start + 1, end - start - 1).Trim();
					if(command != "")
					{
						this.runEmbeddedOutputCommand(command);
					}
				}
				start = text.IndexOf(startCommand, start+1);
			}			
		}//parseEmbeddedOutputCommands(string text)

		private void runEmbeddedOutputCommand(string command)
		{
			int spaceIndex = command.IndexOf(" ");
			
			string function;
			string args;
			if(spaceIndex == -1)
			{
				function = command.ToLower();
				args = "";
			}
			else
			{
				function = command.Substring(0, spaceIndex).ToLower();
				args = command.Substring(spaceIndex + 1);
			}

			try
			{
				switch(function)
				{
					case "quit":
					case "exit":

						this.Close();
						break;
					case "run":
						this.runProgram(args);
						break;
					default:
						break;
				}//switch
			}
			catch {}
		}//runOutputEmbeddedCommand(string command)

		private void runProgram(string command)
		{
			try
			{
				string[] pieces = this.splitOnFirstUnquotedSpace(command);

				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.StartInfo.FileName = pieces[0].Trim(); 
				proc.StartInfo.Arguments = pieces[1];
				proc.StartInfo.CreateNoWindow = true;
				proc.StartInfo.UseShellExecute = true;
				proc.Start();
			}
			catch{}
		}//runProgram(string filename, string args)
		
		public string[] splitOnFirstUnquotedSpace(string text)
		{
			string[] pieces = new string[2];
			int index = -1;
			bool isQuoted = false;
			//find the first unquoted space
			for(int i = 0; i < text.Length; i++)
			{
				if(text[i] == '"')
					isQuoted = !isQuoted;
				else if(text[i] == ' ' && !isQuoted)
				{
					index = i;
					break;
				}
			}

			//break up the string
			if(index != -1)
			{
				pieces[0] = text.Substring(0, index);
				pieces[1] = text.Substring(index + 1);
			}
			else
			{
				pieces[0] = text;
				pieces[1] = "";
			}

			return pieces;
		}//splitOnFirstUnquotedSpace(string text)

	}//class VerbotWinApp
}
