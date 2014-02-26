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
using System.Collections;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Conversive.Verbot5
{
	/// <summary>
	/// Toolbox for dynamically compiling and executing C# code.
	/// </summary>
	public class CSharpToolbox
	{
		public bool ContainsCode
		{
			get
			{
				bool bRet = true;
				if(this.codeModules.Count == 0 && this.conditions.Count == 0 && this.outputs.Count == 0)
					bRet = false;
				return bRet;
			}
		}

        private string STD_MODULE_NAME = "VerbotStandard";
        private string STD_EVENT_NAME_BEFORE_RULE_FIRED = "OnBeforeRuleFired";
        private string STD_EVENT_NAME_AFTER_RULE_FIRED = "OnAfterRuleFired";
        private string STD_EVENT_NAME_NO_RULE_FIRED = "OnNoRuleFired";
        private string STD_EVENT_NAME_NO_OUTPUT_FOUND = "OnNoOutputFound";

        private bool standardBeforeRuleFiredDefined = false;
        private bool standardAfterRuleFiredDefined = false;
        
        private bool standardNoRuleFiredDefined = false;
        public bool StandardNoRuleFiredDefined { get { return this.standardNoRuleFiredDefined; } }

        private bool standardNoOutputFoundDefined = false;
        public bool StandardNoOutputFoundDefined { get { return this.standardNoOutputFoundDefined; } }

        

		private string COND_PREFIX = "Cond_";
		private string OUTPUT_PREFIX = "Output_";

		private List<CodeModule> codeModules;//list of CodeModule objects
        public List<CodeModule> CodeModules
		{
			get{return this.codeModules;}
			set{this.codeModules = value;}
		}

		private Dictionary<string, string> conditions;//keys are ids, values are condition strings
        public Dictionary<string, string> Conditions
        {
            get { return this.conditions; }
            set { this.conditions = value; }
        }

        private Dictionary<string, string> outputs;//keys are ids, values are output strings
        public Dictionary<string, string> Outputs
        {
            get { return this.outputs; }
            set { this.outputs = value; }
        }

		private string code;
		public string Code{get{return this.code;}}
		
		string openTag = "<?csharp";
		string closeTag = "?>";

		private Assembly assembly;

		private Dictionary<Thread, Job> threadJobs;//keys are Thread objects, values are jobs or results

		public delegate void CompileWarning(string warningText, string lineText);
		public event CompileWarning OnCompileWarning;
		public delegate void CompileError(string errorText, string lineText);
		public event CompileError OnCompileError;

		public CSharpToolbox()
		{
			this.codeModules = new List<CodeModule>();
			this.conditions = new Dictionary<string,string>();
			this.outputs = new Dictionary<string,string>();
			this.assembly = null;
			this.threadJobs = new Dictionary<Thread,Job>();
		}

		public void AddCodeModule(CodeModule cm)
		{
			this.codeModules.Add(cm);
		}

		public void AddCondition(string id, string condition)
		{
            if (!this.conditions.ContainsKey(id))
                this.conditions.Add(id, condition);
            else
			    this.conditions[id] = condition;
		}

		public bool ContainsCSharpTags(string text)
		{
			//need to see escaped csharp tags as csharp tags
			//so that we can remove the backslashes later in the code
			return (text.IndexOf(this.openTag) != -1);
		}
		
		public void AddOutput(string id, string output)
		{
            if (!this.outputs.ContainsKey(id))
                this.outputs.Add(id, output);
            else
			    this.outputs[id] = output;
		}

		public void AssembleCode()
		{
			string assemblyCode = "";
			assemblyCode += "using System;\r\n";//using lines?
			assemblyCode += "using System.Collections;\r\n";
			assemblyCode += "using Conversive.Verbot5;\r\n";
			foreach(CodeModule cm in this.codeModules)
			{
				assemblyCode += "\r\n";
				//convert cm to class
				assemblyCode += this.codeModule2Class(cm);
			}//foreach(CodeModule cm in this.codeModules)

			assemblyCode += this.conditions2Class();
			assemblyCode += this.outputs2Class();

			this.code = assemblyCode;
		}

		public bool Compile()//true if successful
		{
			bool success;
			this.AssembleCode();//convert the data structures to this.code

			//compile class into assembly
			CSharpCodeProvider codeProvider = new CSharpCodeProvider();
			//ICodeCompiler compiler = codeProvider.CreateCompiler();
			
			CompilerParameters parameters = new CompilerParameters();
			parameters.GenerateInMemory = true;
			parameters.GenerateExecutable = false;
			//this should come from some Using collection
			parameters.ReferencedAssemblies.Add("system.dll");
			//the following line doesn't work in web mode
			//parameters.ReferencedAssemblies.Add(Application.StartupPath + Path.DirectorySeparatorChar + "Verbot5Library.dll");
			//use this instead
			parameters.ReferencedAssemblies.Add(System.Reflection.Assembly.GetExecutingAssembly().Location);

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, this.code);
			if(!results.Errors.HasErrors)//if no errors
			{
				success = true;
				this.assembly = results.CompiledAssembly;
			}
			else// some errors
			{
				success = false;
				foreach(CompilerError error in results.Errors)
				{
					if(error.IsWarning)
					{
						if(this.OnCompileWarning != null)
							this.OnCompileWarning(error.ErrorText, this.getLineSubstring(this.code, error.Line));
					}
					else
					{
						if(this.OnCompileError!= null)
							this.OnCompileError(error.ErrorText, this.getLineSubstring(this.code, error.Line));
					}				
				}//foreach(CompilerError error in results.Errors)
			}
			return success;
		}//Compile()

		public bool ExecuteCondition(string id)
		{
			return this.ExecuteCondition(id, new Hashtable());
		}//ExecuteCondition(string id)

		public bool ExecuteCondition(string id, Hashtable vars)
		{
			try
			{
				if(!this.conditions.ContainsKey(id))
				{
					return true;
				}
				else if(this.assembly != null)
				{
					Thread exeThread = new Thread(new ThreadStart(this.runCondition));
                    Job job = new Job();
					job.Name = this.COND_PREFIX + id;
					job.Args.Add(new StringTable(vars));
					this.threadJobs[exeThread] = job;
					exeThread.IsBackground = true;
					exeThread.Start();
					exeThread.Join(5000);//join when done or in 5 sec.
					bool result = (bool)this.threadJobs[exeThread].Result;
                    this.threadJobs[exeThread] = null;//clean up the thread job
					return result;
				}
			} 
			catch{}
			return false;//error
		}//ExecuteCondition(string id, Hashtable vars)

		public void runCondition()
		{
			try
			{
                Job job = null;
                if (this.threadJobs.ContainsKey(Thread.CurrentThread))
                {
                    job = this.threadJobs[Thread.CurrentThread];
                }
				if(job != null)
				{
					Type type = this.assembly.GetType("Conditions");
					object[] args = { job.Args[0] };//Vars
					bool result = (bool)type.InvokeMember(job.Name, BindingFlags.InvokeMethod, null, assembly, args);
					this.threadJobs[Thread.CurrentThread].Result = result;
				}
			}
			catch
			{
                if(this.threadJobs.ContainsKey(Thread.CurrentThread))
				    this.threadJobs[Thread.CurrentThread].Result = false;
			}
		}//runCondition()

		public string ExecuteOutput(string id)
		{
			return this.ExecuteOutput(id, new Hashtable());
		}//ExecuteOutput(string id)

		public string ExecuteOutput(string id, Hashtable vars)
		{
			string output = "";
			TextWriter consoleOut = Console.Out;
			try
			{
				if(this.assembly != null)
				{
					Thread exeThread = new Thread(new ThreadStart(this.runOutput));
					Job job = new Job();
					job.Name = this.OUTPUT_PREFIX + id;
					job.Args.Add(new StringTable(vars));
					this.threadJobs[exeThread] = job;
					exeThread.IsBackground = true;
					exeThread.Start();
					exeThread.Join(5000);//join when done or in 5 sec.
					//copy any vars changes back into the main vars object
					vars.Clear();//we need to do this in case any were deleted
					foreach(string key in ((StringTable)job.Args[0]).Keys)
					{
						if(vars[key] == null || vars[key] is string)
                            vars[key] = ((StringTable)job.Args[0])[key];
					}
					output = (string)this.threadJobs[exeThread].Result;
					this.threadJobs[exeThread] = null;//clean up the thread job
				}
			} 
			catch{}
			finally
			{
				Console.SetOut(consoleOut);
			}
			return output;
		}//ExecuteOutput(string id, Hashtable vars)

		public void runOutput()
		{
			try
			{
                Job job = null;
                if (this.threadJobs.ContainsKey(Thread.CurrentThread))
                    job = this.threadJobs[Thread.CurrentThread];
				if(job != null)
				{
					MemoryStream memStream = new MemoryStream(512);
					StreamWriter writer = new System.IO.StreamWriter(memStream);
					Console.SetOut(writer);

					Type type = this.assembly.GetType("Outputs");
					object[] args = { job.Args[0] };
					type.InvokeMember(job.Name, BindingFlags.InvokeMethod, null, assembly, args);

					writer.Flush();
					byte[] byteArray = memStream.ToArray();
					char[] charArray = Encoding.UTF8.GetChars(byteArray);
					this.threadJobs[Thread.CurrentThread].Result = new string(charArray);
				}
			}
			catch
			{
				this.threadJobs[Thread.CurrentThread].Result = "";
			}
        }//runOutput()

        public string ExecuteStandardJob(string methodName, object[] args)
        {
            string output = "";
            TextWriter consoleOut = Console.Out;
            try
            {
                if (this.assembly != null)
                {
                    Thread exeThread = new Thread(new ThreadStart(this.runStandardJob));
                    Job job = new Job();
                    job.Name = methodName;
                    job.Args.AddRange(args);
                    this.threadJobs[exeThread] = job;
                    exeThread.IsBackground = true;
                    exeThread.Start();
                    exeThread.Join(5000);//join when done or in 5 sec.

                    output = (string)this.threadJobs[exeThread].Result;

                    this.threadJobs[exeThread] = null;//clean up the thread job
                }
            }
            catch { }
            finally
            {
                Console.SetOut(consoleOut);
            }
            return output;
        }

        public void runStandardJob()
        {
            try
            {
                Job job = null;
                if (this.threadJobs.ContainsKey(Thread.CurrentThread))
                    job = this.threadJobs[Thread.CurrentThread];
                if (job != null)
                {
                    MemoryStream memStream = new MemoryStream(512);
                    StreamWriter writer = new System.IO.StreamWriter(memStream);
                    Console.SetOut(writer);

                    Type type = this.assembly.GetType(this.STD_MODULE_NAME);
                    object[] args = job.Args.ToArray();
                    type.InvokeMember(job.Name, BindingFlags.InvokeMethod, null, assembly, args);

                    writer.Flush();
                    byte[] byteArray = memStream.ToArray();
                    char[] charArray = Encoding.UTF8.GetChars(byteArray);
                    this.threadJobs[Thread.CurrentThread].Result = new string(charArray);
                }
            }
            catch(Exception e)
            {
                this.threadJobs[Thread.CurrentThread].Result = e.ToString();
            }
        }//runStandardJob()

        public string ExecuteOnBeforeRuleFired(State s)
        {
            string ret = "";
            if (this.standardBeforeRuleFiredDefined)
            {
                object[] args = { s };
                ret = this.ExecuteStandardJob(this.STD_EVENT_NAME_BEFORE_RULE_FIRED, args);
            }
            return ret;
        }

        public string ExecuteOnAfterRuleFired(State s, Reply r)
        {
            string ret = "";
            if (this.standardAfterRuleFiredDefined)
            {
                object[] args = { s, r };
                ret = this.ExecuteStandardJob(this.STD_EVENT_NAME_AFTER_RULE_FIRED, args);
            }
            return ret;
        }

        public string ExecuteOnNoRuleFired(State s, Reply r)
        {
            string ret = "";
            if (this.standardNoRuleFiredDefined)
            {
                object[] args = { s, r };
                ret = this.ExecuteStandardJob(this.STD_EVENT_NAME_NO_RULE_FIRED, args);
            }
            return ret;
        }

        public string ExecuteOnNoOutputFound(Hashtable vars, Reply r)
        {
            string ret = "";
            if (this.standardNoOutputFoundDefined)
            {
                object[] args = { vars, r };
                ret = this.ExecuteStandardJob(this.STD_EVENT_NAME_NO_OUTPUT_FOUND, args);
            }
            return ret;
        }

		public string ShowCodeModuleClassCode(CodeModule cm)
		{
			return this.codeModule2Class(cm);
		}

		public bool ConditionExists(string id)
		{
			return this.conditions.ContainsKey(id);
		}

		public bool OutputExists(string id)
		{
			return this.outputs.ContainsKey(id);
		}

		private string codeModule2Class(CodeModule cm)
		{
            bool cmIsStandard = false;
            if (cm.Name == this.STD_MODULE_NAME)
            {
                cmIsStandard = true;
            }

			StringBuilder sb = new StringBuilder();
			sb.Append("public class ");
			sb.Append(cm.Name);
			sb.Append(" {\r\n");//open class
			foreach(Function f in cm.Functions)
			{
                if (cmIsStandard && f.Name == this.STD_EVENT_NAME_BEFORE_RULE_FIRED)
                {
                    this.standardBeforeRuleFiredDefined = true;
                }
                else if (cmIsStandard && f.Name == this.STD_EVENT_NAME_AFTER_RULE_FIRED)
                {
                    this.standardAfterRuleFiredDefined = true;
                }
                else if (cmIsStandard && f.Name == this.STD_EVENT_NAME_NO_RULE_FIRED)
                {
                    this.standardNoRuleFiredDefined = true;
                }
                else if (cmIsStandard && f.Name == this.STD_EVENT_NAME_NO_OUTPUT_FOUND)
                {
                    this.standardNoOutputFoundDefined = true;
                }
				sb.Append("public ");
				sb.Append("static ");//all methods are static
				sb.Append(f.ReturnType);
				sb.Append(" ");
				sb.Append(f.Name);
				sb.Append("(");
				sb.Append(f.Parameters);
				sb.Append(") {\r\n");//open method
				sb.Append(f.Code);
				sb.Append("}\r\n");//close method
			}
			sb.Append("}");//close class
			return sb.ToString();
		}//CodeModule2Class()

		private string conditions2Class()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("public class Conditions");
			sb.Append(" {\r\n");//open class
			foreach(string id in this.conditions.Keys)
			{
				sb.Append("public static bool ");
				sb.Append(this.COND_PREFIX);//conditional prefix
				sb.Append(id);
				sb.Append("(StringTable vars) {\r\n");//open method
				sb.Append("return ");
				sb.Append((string)this.conditions[id]);
				sb.Append(";}\r\n");//close method
			}
			sb.Append("}");//close class
			return sb.ToString();
		}//conditions2Class()

		private string outputs2Class()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("public class Outputs");
			sb.Append(" {\r\n");//open class
			foreach(string id in this.outputs.Keys)
			{
				sb.Append("public static void ");
				sb.Append(this.OUTPUT_PREFIX);//output prefix
				sb.Append(id);
				sb.Append("(StringTable vars) {\r\n");//open method
				sb.Append(this.outputToCode((string)this.outputs[id]));
				sb.Append("}\r\n");//close method
			}
			sb.Append("}");//close class
			return sb.ToString();
		}//outputs2Class()

		private string outputToCode(string text)
		{
			if(text == null)
				return "";

			StringBuilder sb = new StringBuilder();
			string uncode = "";
			string code = "";
			//break the text apart into text and code sections
			//test needs to be written to Console.Out
			//and code needs to be written as is
			while(text != "")
			{
				int openTagPos = text.IndexOf(this.openTag);
				while(openTagPos != -1 && TextToolbox.IsEscaped(text, openTagPos))
				{
					text = text.Remove(openTagPos-1, 1);
					openTagPos = text.IndexOf(this.openTag, openTagPos);
				}

				if(openTagPos != 0)//we have uncode to process
				{
					if(openTagPos == -1)//it's all uncode
					{
						uncode = text;
						text = "";
					}
					else//it's uncode + code
					{
						uncode = text.Substring(0, openTagPos);
						text = text.Substring(openTagPos);
					}

					sb.Append("Console.Write(\"");
					sb.Append(this.escape(uncode));
					sb.Append("\");\r\n");
				}
				else//we have code to process (open is at the beginning)
				{
					int closeTagPos = text.IndexOf(this.closeTag);
					while(closeTagPos != -1 && TextToolbox.IsEscaped(text, closeTagPos))
					{
						text = text.Remove(closeTagPos-1, 1);
						closeTagPos = text.IndexOf(this.closeTag, closeTagPos);
					}
					if(closeTagPos == -1)
						closeTagPos = text.Length;
					code = text.Substring(this.openTag.Length, closeTagPos - this.openTag.Length).Trim();
					if(code != "")
						sb.Append(code);
					if(closeTagPos + this.closeTag.Length < text.Length)
						text = text.Substring(closeTagPos + this.closeTag.Length);
					else
						text = "";
				}
			}//while(text != "")

			return sb.ToString();
		}//outputToCode(string text)

		private string escape(string text)
		{
			//replaces \ with \\, and " with \"
			text = text.Replace("\\", "\\\\");//backslash
			text = text.Replace("\"", "\\\"");//quote
			text = text.Replace("\r", "\\r");//return
			text = text.Replace("\n", "\\n");//new line
			text = text.Replace("\t", "\\t");//tab
			text = text.Replace("\f", "\\f");//line feed
			return text;
		}

		private string getLineSubstring(string text, int lineNum)
		{
			string[] splits = text.Split('\n');
			if(splits.Length >= lineNum)
				return splits[lineNum-1].Trim();
			else
				return "";
		}//getLine(string text, int lineNum)

	}//public class CSharpToolbox

    public class Job
    {
        private string name;
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /*private StringTable vars;
        public StringTable Vars
        {
            get { return this.vars; }
            set { this.vars = value; }
        }*/
        private List<object> args;
        public List<object> Args
        {
            get { return this.args; }
            set { this.args = value; }
        }

        private object result;
        public object Result
        {
            get { return this.result; }
            set { this.result = value; }
        }

        public Job()
        {
            this.name = "";
            this.args = new List<object>();
            this.result = null;
        }

    }

	public class StringTable : Hashtable
	{
		public StringTable(Hashtable table) : base(table)
		{
		}

		public string this [string key]
		{
			get
			{
				key = key.ToLower();
				if(base[key] == null)
					return (string)base[key];//return null string
				else
					return base[key].ToString();
			}
			set
			{
				key = key.ToLower();
				base[key] = value;
			}
		}
	}//class StringTable : Hashtable
}//namespace Verbot5Library
