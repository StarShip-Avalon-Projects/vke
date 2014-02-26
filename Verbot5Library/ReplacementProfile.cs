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
using System.Xml.Serialization;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace Conversive.Verbot5
{
	/// <summary>
	/// Replaces input and output text with the given strings.
	/// </summary>
	public class ReplacementProfile
	{

		[XmlArrayItem("Replacement")]
		public List<Replacement> Replacements;

		[XmlArrayItem("InputReplacement")]
		public List<InputReplacement> InputReplacements;

		public ReplacementProfile()
		{
			this.Replacements = new List<Replacement>();
			this.InputReplacements = new List<InputReplacement>();
		}

		/*
		 * Unserialized Attributes
		 */

		private bool changed;
		[XmlIgnoreAttribute]
		public bool Changed
		{
			get
			{
				return this.changed;
			}
			set
			{
				this.changed = value;
			}
		}
	}

	[Serializable]
	public class Replacement : ISerializable
	{
		private string textToFind;
		public string TextToFind
		{
			get
			{
				return this.textToFind;
			}
			set
			{
				this.textToFind = value;
			}
		}

		private string textForAgent;
		public string TextForAgent
		{
			get
			{
				return this.textForAgent;
			}
			set
			{
				this.textForAgent = value;
			}
		}

		private string textForOutput;
		public string TextForOutput
		{
			get
			{
				return this.textForOutput;
			}
			set
			{
				this.textForOutput = value;
			}
		}

		public Replacement()
		{
			this.TextToFind = "";
			this.TextForAgent = "";
			this.TextForOutput = "";
		}

		protected Replacement(SerializationInfo info, StreamingContext context)
		{
			this.TextToFind = info.GetString("ttf");
			this.TextForAgent = info.GetString("tfa");
			this.TextForOutput = info.GetString("tfo");
			//use a try/catch block around any new vales
		}
		[SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ttf", this.TextToFind);
			info.AddValue("tfa", this.TextForAgent);
			info.AddValue("tfo", this.TextForOutput);
		}
	}//class Replacement

	[Serializable]
	public class InputReplacement : ISerializable
	{
		private string textToFind;
		public string TextToFind
		{
			get
			{
				return this.textToFind;
			}
			set
			{
				this.textToFind = value;
			}
		}

		private string textToInput;
		public string TextToInput
		{
			get
			{
				return this.textToInput;
			}
			set
			{
				this.textToInput = value;
			}
		}

		public InputReplacement()
		{
			this.textToFind = "";
			this.textToInput = "";
		}

		public InputReplacement(string stTextToFind, string stTextToInput)
		{
			this.textToFind = stTextToFind;
			this.textToInput = stTextToInput;
		}

		protected InputReplacement(SerializationInfo info, StreamingContext context)
		{
			this.TextToFind = info.GetString("ttf");
			this.textToInput = info.GetString("tti");
			//use a try/catch block around any new vales
		}
		[SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ttf", this.TextToFind);
			info.AddValue("tti", this.TextToInput);
		}
	}//class InputReplacement
}
