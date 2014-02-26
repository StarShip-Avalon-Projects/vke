/*
	Copyright 2004-2006 Conversive, Inc.
	http://www.conversive.com
	3806 Cross Creek Rd., Unit F
	Malibu, CA 90265
 
	This file is part of Verbot 5 Library: a natural language processing engine.

    Verbot 5 Library is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    Verbot 5 Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Verbot 5 Library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
	
	Verbot 5 Library may also be available under other licenses.
*/

using System;
using System.IO;
using System.Collections;
using System.Security.Permissions;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections.Generic;

namespace Conversive.Verbot5
{
	/// <summary>
	/// Core NLP engine code for Verbot 5 Library.
	/// </summary>
	public class Verbot5Engine
	{
		private Dictionary<string, CompiledKnowledgeBase> compiledKnowledgeBases;
		
		private XMLToolbox xmlToolbox;
		private ICryptoTransform encryptor;
		private ICryptoTransform decryptor;
        public Logger logger; 
        public delegate void KnowledgeBaseLoadError(Exception openException, string filePath);
        public event KnowledgeBaseLoadError OnKnowledgeBaseLoadError;

		public delegate void RuleCompiled(int completed, int total);
		public event RuleCompiled OnRuleCompiled;

		public delegate void RuleCompileFailed(string ruleId, string ruleName, string errorMessage);
		public event RuleCompileFailed OnRuleCompileFailed;

		public delegate void CompileWarning(string warningText, string lineText);
		public event CompileWarning OnCompileWarning;

		public delegate void CompileError(string errorText, string lineText);
		public event CompileError OnCompileError;

		public Verbot5Engine()
		{
			//This isn't strong encryption.  It's more for obfuscation.
			byte[] k = this.gk(32, "Copyright 2004 - Conversive, Inc.");
			byte[] v = this.gk(16, "Start the Dialog™");
			this.init(k, v);
            this.logger = new Logger("ReplyLog", "New log");
		}

		public Verbot5Engine(string encryptionKey, string encryptionVector)
		{
			byte[] k = this.gk(32, encryptionKey);
			byte[] v = this.gk(16, encryptionVector);
			this.init(k, v);
		}

		private void init(byte[] k, byte[] v)
		{
			this.compiledKnowledgeBases = new Dictionary<string,CompiledKnowledgeBase>();
			xmlToolbox = new XMLToolbox(typeof(KnowledgeBase));
			RijndaelManaged crypto = new RijndaelManaged();
			this.encryptor = crypto.CreateEncryptor(k, v);
			this.decryptor = crypto.CreateDecryptor(k, v);
		}

		public CompiledKnowledgeBase CompileKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
		{	
			return LoadKnowledgeBase(kb, knowledgeBaseItem);
		}

		public void SaveCompiledKnowledgeBase(CompiledKnowledgeBase ckb, string stPath)
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream fs = new FileStream(stPath, FileMode.Create);
			bf.Serialize(fs, ckb);
			fs.Close();
		}

		public void SaveEncryptedCompiledKnowledgeBase(CompiledKnowledgeBase ckb, string stPath)
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream fs = new FileStream(stPath, FileMode.Create);
			CryptoStream csEncrypt = new CryptoStream(fs, this.encryptor, CryptoStreamMode.Write);
			bf.Serialize(csEncrypt, ckb);
			csEncrypt.FlushFinalBlock();
			fs.Flush();
			fs.Close();
		}

		public CompiledKnowledgeBase LoadCompiledKnowledgeBase(string stPath)
		{
			BinaryFormatter bf = new BinaryFormatter();
			Stream fs = Stream.Null;
			CompiledKnowledgeBase ckb = null;
            try
            {
                fs = new FileStream(stPath, FileMode.Open, FileAccess.Read);
                ckb = (CompiledKnowledgeBase)bf.Deserialize(fs);
                ckb.AddConditionsAndCode();
            }
            catch (Exception eOpenOrDeserial)
            {
                bool bErrorHandled = false;
                string openOrSerserialError = eOpenOrDeserial.ToString();
                try//to open an encrypted CKB
                {
                    if (fs != FileStream.Null)
                        fs.Seek(0, SeekOrigin.Begin);
                    CryptoStream csDecrypt = new CryptoStream(fs, this.decryptor, CryptoStreamMode.Read);
                    ckb = (CompiledKnowledgeBase)bf.Deserialize(csDecrypt);
                    ckb.AddConditionsAndCode();
                    bErrorHandled = true;
                }
                catch (Exception e)
                {
                    string str = e.ToString();
                }
                if (!bErrorHandled)
                {
                    if (this.OnKnowledgeBaseLoadError != null)
                    {
                        this.OnKnowledgeBaseLoadError(eOpenOrDeserial, stPath);
                    }
                }
            }
            finally
            {
                if(fs != null && fs != FileStream.Null)
                    fs.Close();
            }
			return ckb;
		}//LoadCompiledKnowledgeBase(string stPath)

		public KnowledgeBase LoadKnowledgeBase(string stPath)
		{
			KnowledgeBase vkb = (KnowledgeBase)this.xmlToolbox.LoadXML(stPath);
			return vkb;
		}

		public CompiledKnowledgeBase LoadKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
		{
			CompiledKnowledgeBase ckb = new CompiledKnowledgeBase();
			ckb.Build = kb.Build;
			ckb.Name = knowledgeBaseItem.Fullpath + knowledgeBaseItem.Filename;
			ckb.OnRuleCompiled += new CompiledKnowledgeBase.RuleCompiled(this.compiledKnowledgeBase_OnRuleCompiled);
			ckb.OnRuleCompileFailed += new CompiledKnowledgeBase.RuleCompileFailed(this.compiledKnowledgeBase_OnRuleCompileFailed);
			ckb.OnCompileError += new Conversive.Verbot5.CompiledKnowledgeBase.CompileError(ckb_OnCompileError);
			ckb.OnCompileWarning += new Conversive.Verbot5.CompiledKnowledgeBase.CompileWarning(ckb_OnCompileWarning);
			ckb.LoadKnowledgeBase(kb, knowledgeBaseItem);
			return ckb;
		}

        public void AddOrReplaceCompiledKnowlegeBase(string stPath, CompiledKnowledgeBase ckb)
        {
            this.addOrReplaceCompiledKnowlegeBase(stPath, ckb);
        }

        private void addOrReplaceCompiledKnowlegeBase(string stPath, CompiledKnowledgeBase ckb)
        {
            if (ckb != null)
            {
                if (!compiledKnowledgeBases.ContainsKey(stPath))
                {
                    this.compiledKnowledgeBases.Add(stPath, ckb);
                }
                else
                {
                    this.compiledKnowledgeBases[stPath] = ckb;
                }
            }
        }

		public CompiledKnowledgeBase AddCompiledKnowledgeBase(string stPath)
		{
			CompiledKnowledgeBase ckb = LoadCompiledKnowledgeBase(stPath);
            this.addOrReplaceCompiledKnowlegeBase(stPath, ckb);
			return ckb;
		}

        public bool ContainsCompiledKnowledgeBase(string stPath)
        {
            bool bLoaded = false;
            if (this.compiledKnowledgeBases.ContainsKey(stPath))
            {
                bLoaded = true;
            }
            return bLoaded;
        }

		public void RemoveCompiledKnowledgeBase(string stPath)
		{
			this.compiledKnowledgeBases.Remove(stPath);
		}

		public void RemoveCompiledKnowledgeBase(CompiledKnowledgeBase ckb)
		{
			string stPath = ckb.KnowledgeBaseItem.Fullpath+ckb.KnowledgeBaseItem.Filename;
			this.RemoveCompiledKnowledgeBase(stPath);
		}

		public CompiledKnowledgeBase AddKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
		{
			CompiledKnowledgeBase ckb = this.LoadKnowledgeBase(kb, knowledgeBaseItem);
			string stPath = knowledgeBaseItem.Fullpath + knowledgeBaseItem.Filename;
            this.addOrReplaceCompiledKnowlegeBase(stPath, ckb);
			return ckb;
		}

		public void ReloadKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
		{
			CompiledKnowledgeBase ckb = this.LoadKnowledgeBase(kb, knowledgeBaseItem);
			string stPath = knowledgeBaseItem.Fullpath + knowledgeBaseItem.Filename;
            this.addOrReplaceCompiledKnowlegeBase(stPath, ckb);
		}

		private void compiledKnowledgeBase_OnRuleCompiled(int completed, int total)
		{
			if(this.OnRuleCompiled != null)
				this.OnRuleCompiled(completed, total);
		}//engine_OnRuleCompiled(int current, int total)

		private void compiledKnowledgeBase_OnRuleCompileFailed(string ruleId, string ruleName, string errorMessage)
		{
			if(this.OnRuleCompileFailed != null)
				this.OnRuleCompileFailed(ruleId, ruleName, errorMessage);
		}

		public Reply GetReply(string input, State state)
		{
            logger.LogMessage("GetReply");
			state.Vars["_input"] = input;
			state.Vars["_lastinput"] = state.Lastinput;
			state.Vars["_lastfired"] = state.Lastfired;
			state.Vars["_time"] = DateTime.Now.ToString("h:mm tt");
			state.Vars["_time24"] = DateTime.Now.ToString("HH:mm");
			state.Vars["_date"] = DateTime.Now.ToString("MMM. d, yyyy");
			state.Vars["_month"] = DateTime.Now.ToString("MMMM");
			state.Vars["_dayofmonth"] = DateTime.Now.ToString("d ").Trim();
			state.Vars["_year"] = DateTime.Now.ToString("yyyy");
			state.Vars["_dayofweek"] = DateTime.Now.ToString("dddd");
           
			if(input.Length == 0)
				input = "_blank";

			state.Lastinput = input;
            bool standardNoRuleFiredDefined = false;
            Reply noRuleFiredReply = new Reply("","","","",0.0, null);
			foreach(string stPath in state.CurrentKBs)
			{
                if(this.compiledKnowledgeBases.ContainsKey(stPath))
                {
                    logger.LogMessage("compiledKnowledgeBases(" + stPath + ")");
				    CompiledKnowledgeBase ckb = (CompiledKnowledgeBase)this.compiledKnowledgeBases[stPath];
                    string ckbOutput = ckb.CSToolbox.ExecuteOnBeforeRuleFired(state);
                    //update our input var incase the Before Rule Fired changes it
                    input = (string)state.Vars["_input"];
                    state.Lastinput = input;
                    Reply reply = ckb.GetReply(input, state.Lastfired, state.Vars, ckbOutput);
					if(reply != null)
					{
                        logger.LogMessage("reply != null");
                        ckbOutput = ckb.CSToolbox.ExecuteOnAfterRuleFired(state, reply);
                        string outputText = ckb.DoTextReplacements(ckbOutput);
                        string agentText = ckb.DoAgentTextReplacements(ckbOutput);
                        reply.Text += outputText;
                        reply.AgentText += agentText;
						state.Lastfired = reply.RuleId;
						state.Vars["_lastoutput"] = reply.Text;
						return reply;
					}
                    if (ckb.CSToolbox.StandardNoRuleFiredDefined)
                    {
                        logger.LogMessage("ckb.CSToolbox.StandardNoRuleFiredDefined");
                        standardNoRuleFiredDefined = true;
                        //if this ckb has a no rule fired handler, fire it
                        noRuleFiredReply.KBItem = ckb.KnowledgeBaseItem;
                        string noReplyText = ckb.CSToolbox.ExecuteOnNoRuleFired(state, noRuleFiredReply);
                        noRuleFiredReply.Text += ckb.DoTextReplacements(noReplyText);
                        noRuleFiredReply.AgentText += ckb.DoAgentTextReplacements(noReplyText);
                    }
				}
			}
            if (standardNoRuleFiredDefined)
                return noRuleFiredReply;
			return null; //if there's no reply, return null
		}//GetReply(string input)

		private byte[] gk(byte s, string t)
		{
			SHA256Managed sha256 = new SHA256Managed();
			byte[] x = Encoding.ASCII.GetBytes(t);
			x = sha256.ComputeHash(x, 0, x.Length);
			byte[] o = new byte[s];
			for(int i = 0; i < s; i++)
				o[i] = x[i%x.Length];
			return o;
		}//gk(byte s, string t)

		private void ckb_OnCompileError(string errorText, string lineText)
		{
			if(this.OnCompileError != null)
				this.OnCompileError(errorText, lineText);
		}

		private void ckb_OnCompileWarning(string warningText, string lineText)
		{
			if(this.OnCompileWarning != null)
				this.OnCompileWarning(warningText, lineText);
		}
	}//class Verbot5Engine

	[Serializable]
	public class CompiledKnowledgeBase : ISerializable
	{
		private CSharpToolbox csToolbox = null;
        public CSharpToolbox CSToolbox
        {
            get { return this.csToolbox; }
            set { this.csToolbox = value; }
        }
		public bool ContainsCode{get{return this.csToolbox.ContainsCode;}}
		
		public string Code{get{return this.csToolbox.Code;}}

        private Dictionary<string, List<InputRecognizer>> inputs;
        public Dictionary<string, List<InputRecognizer>> Inputs { get{return this.inputs;} set{this.inputs = value;}}

		private Dictionary<string, List<Output>> outputs;
        public Dictionary<string, List<Output>> Outputs { get{return this.outputs;} set{this.outputs = value;}}

        private Dictionary<string, Synonym> synonyms;
        public Dictionary<string, Synonym> Synonyms { get{return this.synonyms;} set{this.synonyms = value;}}

		private List<Replacement> replacements;
        public List<Replacement> Replacements { get{return this.replacements;} set{this.replacements = value;}}

		private List<InputReplacement> inputReplacements;
        public List<InputReplacement> InputReplacements { get{return this.inputReplacements;} set{this.inputReplacements = value;}}

		private Dictionary<string, List<string>> recentOutputsByRule;
        public Dictionary<string, List<string>> RecentOutputsByRule { get{return this.recentOutputsByRule;} set{this.recentOutputsByRule = value;}}

		private int build;
		public int Build
		{
			get
			{
				return this.build;
			}
			set
			{
				this.build = value;
			}
		}

		
		private KnowledgeBaseItem knowledgeBaseItem;
		public KnowledgeBaseItem KnowledgeBaseItem
		{
			get
			{
				return this.knowledgeBaseItem;
			}
			set
			{
				this.knowledgeBaseItem = value;
			}
		}

		private KnowledgeBaseInfo knowledgeBaseInfo;
		public KnowledgeBaseInfo KnowledgeBaseInfo
		{
			get
			{
				return this.knowledgeBaseInfo;
			}
			set
			{
				this.knowledgeBaseInfo = value;
			}
		}

		[NonSerialized]
		private Random random;

		[NonSerialized]
		public string Name;//This is just used for comparison when reloading

		public delegate void RuleCompiled(int completed, int total);
		public event RuleCompiled OnRuleCompiled;

		public delegate void RuleCompileFailed(string ruleId, string ruleName, string errorMessage);
		public event RuleCompileFailed OnRuleCompileFailed;

		public delegate void CompileWarning(string warningText, string lineText);
		public event CompileWarning OnCompileWarning;

		public delegate void CompileError(string errorText, string lineText);
		public event CompileError OnCompileError;

		public CompiledKnowledgeBase()
		{
			this.synonyms = new Dictionary<string,Synonym>();
			this.replacements = new List<Replacement>();
			this.inputReplacements = new List<InputReplacement>();
			this.inputs = new Dictionary<string,List<InputRecognizer>>();
			this.outputs = new Dictionary<string,List<Output>>();
			this.knowledgeBaseItem = new KnowledgeBaseItem();
			this.knowledgeBaseInfo = new KnowledgeBaseInfo();
			this.build = -1;
			this.Name = "";
			this.random = new Random();
			this.recentOutputsByRule = new Dictionary<string,List<string>>();
			this.initializeCSToolbox();
		}

		protected CompiledKnowledgeBase(SerializationInfo info, StreamingContext context)
		{
            this.synonyms = (Dictionary<string, Synonym>)info.GetValue("s", typeof(Dictionary<string, Synonym>));
            this.replacements = (List<Replacement>)info.GetValue("r", typeof(List<Replacement>));
            this.inputs = (Dictionary<string, List<InputRecognizer>>)info.GetValue("i", typeof(Dictionary<string, List<InputRecognizer>>));
            this.outputs = (Dictionary<string, List<Output>>)info.GetValue("o", typeof(Dictionary<string, List<Output>>));
			this.knowledgeBaseItem = (KnowledgeBaseItem)info.GetValue("k", typeof(KnowledgeBaseItem));
			this.knowledgeBaseInfo = (KnowledgeBaseInfo)info.GetValue("kbi", typeof(KnowledgeBaseInfo));
			this.build = info.GetInt32("b");
			this.random = new Random();
            this.recentOutputsByRule = new Dictionary<string, List<string>>();
			
			//use a try/catch block around any new vales
			try
			{
                this.inputReplacements = (List<InputReplacement>)info.GetValue("ir", typeof(List<InputReplacement>));
			}
			catch
			{
				this.inputReplacements = new List<InputReplacement>();
			}

			this.initializeCSToolbox();
			try
			{
                this.csToolbox.CodeModules = (List<CodeModule>)info.GetValue("cm", typeof(List<CodeModule>));
			}
			catch
			{
                this.csToolbox.CodeModules = new List<CodeModule>();
			}
		}
		[SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("s", this.synonyms);
			info.AddValue("r", this.replacements);
			info.AddValue("i", this.inputs);
			info.AddValue("o", this.outputs);
			info.AddValue("k", this.knowledgeBaseItem);
			info.AddValue("kbi", this.knowledgeBaseInfo);
			info.AddValue("b", (Int32)this.build);
			info.AddValue("ir", this.inputReplacements);

			info.AddValue("cm", this.csToolbox.CodeModules);
		}

		private void initializeCSToolbox()
		{
			this.csToolbox = new CSharpToolbox();
			this.csToolbox.OnCompileError += new Conversive.Verbot5.CSharpToolbox.CompileError(csToolbox_OnCompileError);
			this.csToolbox.OnCompileWarning += new Conversive.Verbot5.CSharpToolbox.CompileWarning(csToolbox_OnCompileWarning);
		}

		public void AddConditionsAndCode()
		{
			try
			{
				foreach(List<InputRecognizer> irs in this.inputs.Values)
				{
					foreach(InputRecognizer ir in irs)
					{
						if(ir.Condition != "")
							this.csToolbox.AddCondition(ir.InputId, ir.Condition);
					}
				}
				foreach(List<Output> os in this.outputs.Values)
				{
					foreach(Output o in os)
					{
						if(o.Condition != "")
							this.csToolbox.AddCondition(o.Id, o.Condition);
						if(this.csToolbox.ContainsCSharpTags(o.Text))
							this.csToolbox.AddOutput(o.Id, o.Text);
						if(this.csToolbox.ContainsCSharpTags(o.Cmd))
							this.csToolbox.AddOutput(o.Id + "_cmd", o.Cmd);
					}
				}
				this.csToolbox.Compile();
			}
			catch(Exception exBin)
			{
				string st = exBin.ToString();
			}
		}

		public Dictionary<string, List<InputRecognizer>> GetInputs()
		{
			return this.inputs;
		}

		public void LoadKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
		{
			this.knowledgeBaseItem = knowledgeBaseItem;
			this.knowledgeBaseInfo = kb.Info;
			this.LoadResourceFiles(kb.ResourceFiles);
			KnowledgeBase decompressedKB = kb.DecompressTemplates(knowledgeBaseItem.Fullpath);
			if(decompressedKB != null)
				this.compileRules("_root", decompressedKB.Rules);
			else
				this.compileRules("_root", kb.Rules);
			this.csToolbox.Compile();
		}

		public void LoadResourceFiles(List<ResourceFile> resourceFiles)
		{
			this.loadSynonyms(resourceFiles);
			this.loadReplacementProfiles(resourceFiles);
			this.loadCodeModules(resourceFiles);
        }//LoadResourceFiles(List<ResourceFile> resourceFiles)

		private void loadSynonyms(List<ResourceFile> resourceFiles)
		{
			XMLToolbox xmlToolbox = new XMLToolbox(typeof(SynonymGroup));
			SynonymGroup sg;
			foreach(ResourceFile rf in resourceFiles)
			{
				if(rf.Filetype == ResourceFileType.SynonymFile)
				{
					sg = (SynonymGroup)xmlToolbox.LoadXML(this.knowledgeBaseItem.Fullpath + rf.Filename);
					foreach(Synonym s in sg.Synonyms)
					{
						s.Phrases.Sort();
						this.synonyms[s.Name.ToLower()] = s;
					}
				}//end if SynonymFile
			}//foreach resource file
        }//loadSynonyms(List<ResourceFile> resourceFiles)

        private void loadReplacementProfiles(List<ResourceFile> resourceFiles)
		{
			XMLToolbox xmlToolbox = new XMLToolbox(typeof(ReplacementProfile));
			ReplacementProfile rp;
			foreach(ResourceFile rf in resourceFiles)
			{
				if(rf.Filetype == ResourceFileType.ReplacementProfileFile)
				{
					rp = (ReplacementProfile)xmlToolbox.LoadXML(this.knowledgeBaseItem.Fullpath + rf.Filename);
					this.replacements.AddRange(rp.Replacements);
					this.inputReplacements.AddRange(rp.InputReplacements);
				}//end if ReplacementProfileFile
			}//foreach resource file
        }//loadReplacementProfiles(List<ResourceFile> resourceFiles)

        private void loadCodeModules(List<ResourceFile> resourceFiles)
		{
			XMLToolbox xmlToolbox = new XMLToolbox(typeof(CodeModule));
			CodeModule cm;
			foreach(ResourceFile rf in resourceFiles)
			{
				if(rf.Filetype == ResourceFileType.CodeModuleFile)
				{
					cm = (CodeModule)xmlToolbox.LoadXML(this.knowledgeBaseItem.Fullpath + rf.Filename);
					this.csToolbox.AddCodeModule(cm);
				}//end if ReplacementProfileFile
			}//foreach resource file
        }//loadCodeModules(List<ResourceFile> resourceFiles)

		private void compileRules(string parentId, List<Rule> rules)
		{
			int ruleCount = rules.Count;
			int ruleCurrent = 0;
			foreach(Rule r in rules)
			{
				try
				{
					ruleCurrent++;
					foreach(Input i in r.Inputs)
					{
						if(i.Condition != "")
							this.csToolbox.AddCondition(i.Id, i.Condition);
						InputRecognizer ir = new InputRecognizer(i.Text, r.Id, i.Id, i.Condition, this.synonyms, this.inputReplacements);
						if(!this.inputs.ContainsKey(parentId))
							this.inputs.Add(parentId, new List<InputRecognizer>());
						this.inputs[parentId].Add(ir);
						//Go through virtualparents and add this input recognizer to virtual parent id keys
						foreach(string virtualParentId in r.VirtualParents)
						{
							if(!this.inputs.ContainsKey(virtualParentId))
								this.inputs.Add(virtualParentId, new List<InputRecognizer>());
							this.inputs[virtualParentId].Add(ir);
						}
					}
					foreach(Output o in r.Outputs)
					{
						if(o.Condition != "")
							this.csToolbox.AddCondition(o.Id, o.Condition);
						if(this.csToolbox.ContainsCSharpTags(o.Text))
							this.csToolbox.AddOutput(o.Id, o.Text);
						if(this.csToolbox.ContainsCSharpTags(o.Cmd))
							this.csToolbox.AddOutput(o.Id + "_cmd", o.Cmd);
						if(!this.outputs.ContainsKey(r.Id))
							this.outputs.Add(r.Id, new List<Output>());
						this.outputs[r.Id].Add(o);
					}
				}
				catch (Exception e)
				{
					if(this.OnRuleCompileFailed != null)
						this.OnRuleCompileFailed(r.Id, r.Name, e.ToString() + "\r\n" + e.StackTrace);
				}
				//compile children
				this.compileRules(r.Id, r.Children);
				if(this.OnRuleCompiled != null && parentId == "_root")
					this.OnRuleCompiled(ruleCurrent, ruleCount);
			}
        }//compileRules(string parentId, List<Rule> rules)

		public Reply GetReply(string input, string lastfired, Hashtable vars, string prependOutput)
		{
			List<Match> matches = new List<Match>();
			//do replacements, strip áccents is done in ReplaceOnInput if there are no input replacements
			string inputReplaced = TextToolbox.ReplaceOnInput(input, this.inputReplacements);
			//search the children and virtual children (links) of lastfired rule
			if(this.inputs.ContainsKey(lastfired))
			{
				foreach(InputRecognizer ir in this.inputs[lastfired])
				{
					//if the input recognizer is a capture, use the original input
					Match match = ir.Matches((ir.IsCapture ? input : inputReplaced), vars, this.csToolbox);

					if(match.ConfidenceFactor != 0.0)
					{
						//copy shortTermMemory vars to inputVars
						Hashtable inputVars = new Hashtable();
						foreach(object key in vars.Keys)
							inputVars[key] = vars[key];
						//captures the variables and adds them to the inputVars object
						ir.CaptureVars(input, inputVars);
						matches.Add(match);
					}
				}
			}

			if(matches.Count == 0 && this.inputs.ContainsKey("_root"))
			{
				//search all of the primaries
				foreach(InputRecognizer ir in this.inputs["_root"])
				{
					//if the input recognizer is a capture, use the original input
					Match match = ir.Matches((ir.IsCapture ? input : inputReplaced), vars, this.csToolbox);
					if(match.ConfidenceFactor != 0.0)
					{						
						//copy shortTermMemory vars to inputVars
						Hashtable inputVars = new Hashtable();
						foreach(object key in vars.Keys)
							inputVars[key] = vars[key];
						//captures the variables and adds them to the inputVars object
						ir.CaptureVars(input, inputVars);
						matches.Add(match);
					}
				}
			}

			if(matches.Count == 0)
				return null;
			
			matches.Sort();
			Match matchBest = (Match)matches[0];
			//use the matching vars, but maintain the original object so that it persists
			vars.Clone();
			foreach(object key in matchBest.Vars.Keys)
				vars[key] = matchBest.Vars[key];
			//increment the usage count on the chosed InputRecognizer
			matchBest.InputRecognizer.IncUsageCount();

			string ruleId = matchBest.InputRecognizer.RuleId;

            if (!this.outputs.ContainsKey(ruleId) || this.outputs[ruleId].Count == 0)
            {
                Reply noOutputFoundReply = new Reply("Rule contains no outputs.", "", "", ruleId, 0, this.knowledgeBaseItem);

                if (this.csToolbox.StandardNoOutputFoundDefined)
                {
                    string noOutputFoundText = this.csToolbox.ExecuteOnNoOutputFound(vars, noOutputFoundReply);
                    noOutputFoundReply.Text += this.DoTextReplacements(noOutputFoundText);
                    noOutputFoundReply.AgentText += this.DoAgentTextReplacements(noOutputFoundText);
                }

                return noOutputFoundReply;
            }

			List<Output> alOutputs = new List<Output>();
            if (this.outputs.ContainsKey(ruleId))
                alOutputs = new List<Output>(this.outputs[ruleId]);

            bool trueNonEmptyOutputConditionFound = false;

			//filter out outputs with false conditions
			for(int i = 0; i < alOutputs.Count; i++)
			{
				Output o = (Output)alOutputs[i];
				if(!this.csToolbox.ExecuteCondition(o.Id, vars))
				{
					alOutputs.RemoveAt(i);
					i--;
				}
                else if (o.Condition != null && o.Condition != "")
                {
                    trueNonEmptyOutputConditionFound = true;
                }
			}
            if (alOutputs.Count == 0)//all outputs were removed
            {
                Reply noOutputFoundReply = new Reply("No true output found.", "", "", ruleId, 0, this.knowledgeBaseItem);

                if (this.csToolbox.StandardNoOutputFoundDefined)
                {
                    string noOutputFoundText = this.csToolbox.ExecuteOnNoOutputFound(vars, noOutputFoundReply);
                    noOutputFoundReply.Text += this.DoTextReplacements(noOutputFoundText);
                    noOutputFoundReply.AgentText += this.DoAgentTextReplacements(noOutputFoundText);
                }

                return noOutputFoundReply;
            }

            //filter out outputs with no conditions if there is an output which has a true condition
            if (trueNonEmptyOutputConditionFound)
            {
                for (int i = 0; i < alOutputs.Count; i++)
                {
                    Output o = (Output)alOutputs[i];
                    if (o.Condition == null || o.Condition == "")
                    {
                        alOutputs.RemoveAt(i);
                        i--;
                    }
                }
            }

			//choose an output at random
			Output outputChosen = null;
			for(int i = 0; i < alOutputs.Count; i++)//the try again loop
			{
				outputChosen = ((Output)alOutputs[random.Next(alOutputs.Count)]);
				if(!this.recentOutputsByRule.ContainsKey(ruleId) || !this.recentOutputsByRule[ruleId].Contains(outputChosen.Id))
					break;
			}

			//update the recent list for this rule
			if(alOutputs.Count > 1)
			{
				if(!this.recentOutputsByRule.ContainsKey(ruleId))
					this.recentOutputsByRule[ruleId] = new List<string>(alOutputs.Count - 1);
				List<string> recent = this.recentOutputsByRule[ruleId];
				int index = recent.IndexOf(outputChosen.Id);
				if(index != -1)
					recent.RemoveAt(index);
				else if(recent.Count == alOutputs.Count - 1)
					recent.RemoveAt(0);
				recent.Add(outputChosen.Id);					
			}
			//replace vars and output synonyms
            string outputChosenText = prependOutput + outputChosen.Text;
			if(this.csToolbox.OutputExists(outputChosen.Id))
				outputChosenText = this.csToolbox.ExecuteOutput(outputChosen.Id, vars);
			outputChosenText = TextToolbox.ReplaceVars(outputChosenText, vars);
			outputChosenText = TextToolbox.ReplaceOutputSynonyms(outputChosenText, this.synonyms);
			//execute c# code in the command field
			string outputChosenCmd = outputChosen.Cmd;
			if(this.csToolbox.OutputExists(outputChosen.Id + "_cmd"))
				outputChosenCmd = this.csToolbox.ExecuteOutput(outputChosen.Id + "_cmd", vars);

			string outputText = this.DoTextReplacements(outputChosenText);
			string agentText = this.DoAgentTextReplacements(outputChosenText);
			string outputCmd = TextToolbox.ReplaceVars(outputChosenCmd, vars);

            return new Reply(outputText, agentText, outputCmd, matchBest.InputRecognizer.RuleId, matchBest.ConfidenceFactor, this.knowledgeBaseItem);
		}//GetReply(string input, string lastfired)

		public string DoTextReplacements(string text)
		{
			foreach(Replacement r in this.replacements)
			{
				if(r.TextToFind != null && r.TextToFind != "")
				{
					int pos = text.IndexOf(r.TextToFind);
					while(pos != -1)
					{
						if(!TextToolbox.IsInCommand(text, pos, r.TextToFind.Length))
						{
							if(pos + r.TextToFind.Length < text.Length - 1)
								text = text.Substring(0, pos)
									+ r.TextForOutput
									+ text.Substring(pos + r.TextToFind.Length);
							else
								text = text.Substring(0, pos)
									+ r.TextForOutput;
						}
						if(pos < text.Length - 1)
							pos = text.IndexOf(r.TextToFind, pos + 1);
						else
							pos = -1;
					}//while
				}//if
			}//foreach

			return text;
		}//doTextReplacements(string text)

		public string DoAgentTextReplacements(string text)
		{

			foreach(Replacement r in this.replacements)
			{
				if(r.TextToFind != null && r.TextToFind != "")
				{
					int pos = text.IndexOf(r.TextToFind);
					while(pos != -1)
					{
						if(!TextToolbox.IsInCommand(text, pos, r.TextToFind.Length))
						{
							if(pos + r.TextToFind.Length < text.Length - 1)
								text = text.Substring(0, pos)
									+ r.TextForAgent
									+ text.Substring(pos + r.TextToFind.Length);
							else
								text = text.Substring(0, pos)
									+ r.TextForAgent;
						}
						if(pos < text.Length - 1)
							pos = text.IndexOf(r.TextToFind, pos + 1);
						else
							pos = -1;
					}//while
				}//if
			}//foreach

			return text;
		}//doAgentTextReplacements(string text)

		private void csToolbox_OnCompileError(string errorText, string lineText)
		{
			if(this.OnCompileError != null)
				this.OnCompileError(errorText, lineText);
		}

		private void csToolbox_OnCompileWarning(string warningText, string lineText)
		{
			if(this.OnCompileWarning != null)
				this.OnCompileWarning(warningText, lineText);
		}
	}//class CompiledKnowledgeBase

	public class Match : IComparable
	{
		public InputRecognizer InputRecognizer;
		public double ConfidenceFactor;
		public Hashtable Vars;

		public Match()
		{
			this.InputRecognizer = null;
			this.ConfidenceFactor = 0.0;
			this.Vars = null;
		}

		public int CompareTo(object o)
		{
			double diff = ((Match)o).ConfidenceFactor - this.ConfidenceFactor;
			if(diff == 0)
				return 0;
			else if(diff > 0)
				return 1;
			else
				return -1;
		}
	}

	public class Reply
	{
        public string Text;
		public string AgentText;
		public string Cmd;
		public string RuleId;
        public double ConfidenceFactor;
		public KnowledgeBaseItem KBItem;

		public Reply(string text, string agentText, string cmd, string ruleId, double cf, KnowledgeBaseItem knowledgeBaseItem)
		{
            this.Text = text;
			this.AgentText = agentText;
			this.Cmd = cmd;
			this.RuleId = ruleId;
            this.ConfidenceFactor = cf;
			this.KBItem = knowledgeBaseItem;
		}
	}//class Reply

	[Serializable]
	public class InputRecognizer : ISerializable
	{
		private Regex regex;
		public Regex Regex
		{
			get { return this.regex; }
			set { this.regex = value; }
		}

		private string ruleId;
		public string RuleId
		{
			get { return this.ruleId; }
			set { this.ruleId = value; }
		}

		private string inputId;
		public string InputId
		{
			get { return this.inputId; }
			set { this.inputId = value; }
		}

		private string condition;
		public string Condition
		{
			get { return this.condition; }
			set { this.condition = value; }
		}

		private bool bIsCapture = false;
		public bool IsCapture
		{
			get { return this.bIsCapture; }
            set { this.bIsCapture = value; }
		}

		private int length;
        public int Length
        {
            get { return this.length; }
            set { this.length = value; }
        }

		[NonSerialized]
		private static Random random = null;
		[NonSerialized]
		private int usageCount = 0;

		[NonSerialized]
		private List<InputReplacement> inputReplacements = null;

		public InputRecognizer(string text, string ruleId, string inputId, string condition, Dictionary<string, Synonym> synonyms, List<InputReplacement> alInputReplacements)
		{
			this.inputReplacements = alInputReplacements;

			System.Text.RegularExpressions.Regex regexVarsEq = new Regex(@"\[.+?=.+?\]");
			System.Text.RegularExpressions.Regex regexVars = new Regex(@"\[.*?\]");
			System.Text.RegularExpressions.Regex regexSyns = new Regex(@"\(.*?\)");
			string textReplaced = regexVarsEq.Replace(text, "xx");
			textReplaced = regexVars.Replace(textReplaced, "x");
			textReplaced = regexSyns.Replace(textReplaced, "x");
			textReplaced = textReplaced.Replace("*", "");
			this.length = textReplaced.Length;

			text = TextToolbox.ReplaceSynonyms(text, synonyms);
			//do this last because replacements haven't been applied to synonyms
			text = TextToolbox.ReplaceOnInput(text, alInputReplacements, out this.bIsCapture);

			string pattern = TextToolbox.TextToPattern(text);
			this.regex = new Regex(pattern, /*RegexOptions.Compiled | */RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);//TODO: Does the IgnoreCase Work?

			//regex.IsMatch("x");
			this.ruleId = ruleId;
			this.inputId = inputId;
			this.condition = condition;

			if(InputRecognizer.random == null)
				InputRecognizer.random = new Random();
		}

		protected InputRecognizer(SerializationInfo info, StreamingContext context)
		{
			this.regex = (Regex)info.GetValue("r", typeof(Regex));
			this.ruleId = info.GetString("i");
			try
			{
				this.inputId = info.GetString("ii");
			}
			catch
			{
				this.inputId = "";
			}
			try
			{
				this.condition = info.GetString("c");
			}
			catch
			{
				this.condition = "";
			}
			try
			{
				this.bIsCapture = info.GetBoolean("ic");
			}
			catch
			{
				this.bIsCapture = false;
			}
			this.length = info.GetInt32("l");
			if(InputRecognizer.random == null)
				InputRecognizer.random = new Random();
		}
		[SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("r", this.regex, typeof(Regex));
			info.AddValue("i", this.ruleId);
			info.AddValue("ii", this.inputId);
			info.AddValue("c", this.condition);
			info.AddValue("l", (Int32)this.length);
			info.AddValue("ic", this.bIsCapture);
		}

		public Match Matches(string input, Hashtable vars, CSharpToolbox csToolbox)
		{
			Match match = new Match();
			if(this.regex.IsMatch(input))
			{
				//copy shortTermMemory vars to inputVars
				Hashtable inputVars = new Hashtable();
				foreach(object key in vars.Keys)
					inputVars[key] = vars[key];
				//captures the variables and adds them to the inputVars object
				this.CaptureVars(input, inputVars);

				if(csToolbox.ExecuteCondition(this.inputId, inputVars))
				{
					double noise = InputRecognizer.random.NextDouble() * 0.0001;
					double usageBonus = (0.01 / (this.usageCount + 1));//goes lower the more it's used
					double cf = 0;
					if(this.length == 0)//is this ever true? yes, when input is *
						cf = usageBonus + noise + 0.0001;
					else
						cf = usageBonus + noise + (this.length / (double)input.Length);
					match.InputRecognizer = this;
					match.Vars = inputVars;
					match.ConfidenceFactor = cf;
				}
			}//if(this.regex.IsMatch(input))
			return match;
		}//Matches(string input)

		public void CaptureVars(string input, Hashtable vars)
		{
			Hashtable tempVars = new Hashtable();
			GroupCollection gc = this.regex.Match(input).Groups;
			if(gc.Count > 0)
			{
				string[] groupNames = this.regex.GetGroupNames();
				foreach(string name in groupNames)
				{
					string v = gc[name].Value;
					int start = input.IndexOf(v);
					int length = v.Length;
					if(start != -1)
						tempVars[name.ToLower()] = input.Substring(start, length);
				}

				//move all of the non-nested vars into the vars Hashtable
				foreach(DictionaryEntry entry in tempVars)
				{
					if(((string)entry.Key).IndexOf("_s_") == -1)
						vars[((string)entry.Key).ToLower()] = entry.Value;
				}

				//process all of the vars that have vars in their name
				foreach(DictionaryEntry entry in tempVars)
				{
					string key = ((string)entry.Key);
					int start = key.IndexOf("_s_");
					while(start != -1)
					{
						int end = key.IndexOf("_e_", start);
						if(end != -1)
						{
							string subVal = (string)vars[key.Substring(start + 3, end - start - 3)];
							if(subVal == null)
								subVal = "";
							if(end + 1 == key.Length)
								key = key.Substring(0, start) + subVal;
							else
								key = key.Substring(0, start) + subVal + key.Substring(end+3);
							start = key.IndexOf("_s_");
						}//end if there was a var within this key
						else
						{
							break;//out of working on this key, it is messed up
						}
					}//end while there are more internal vars
					vars[key.ToLower()] = entry.Value;
				}//end foreach entry in the vars of this IR
			}//if(gc.Count > 0)
		}//CaptureVars(string input, Hashtable vars)

		public void IncUsageCount()
		{
			this.usageCount++;
		}

	}//class InputRecognizer
}
