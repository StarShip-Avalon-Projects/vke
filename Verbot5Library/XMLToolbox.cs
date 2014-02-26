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
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Conversive.Verbot5
{
	/// <summary>
	/// Toolbox for reading and writing XML serialized files.
	/// </summary>
	public class XMLToolbox
	{
		public delegate void XmlSaveError(Exception e, string stPath);
		public event XmlSaveError OnXmlSaveError;

		public delegate void XmlLoadError(Exception e, string stPath);
		public event XmlLoadError OnXmlLoadError;

		private XmlSerializer xmlSerializer;

		public XMLToolbox(Type type)
		{
			Type[] types = {typeof(KnowledgeBase),
							   typeof(Rule),
							   typeof(Input),
							   typeof(Output),
							   typeof(ResourceFile),
							   typeof(SynonymGroup),
							   typeof(Synonym),
							   typeof(Phrase),
							   typeof(ReplacementProfile),
							   typeof(Replacement),
							   typeof(InputReplacement),
							   typeof(CodeModule),
							   typeof(Function),
							   typeof(Verbot5Preferences),
							   typeof(KnowledgeBaseItem),
							   typeof(KnowledgeBaseInfo),
							   typeof(KnowledgeBaseRating),
							   typeof(Verbot5Skin),
							   typeof(TTSModes),
							   typeof(TTSMode),
							   typeof(Font),
							   typeof(ArrayList)};
			xmlSerializer = new XmlSerializer(type, types);
		}

		private void raiseXmlSaveError(Exception e, string stPath)
		{
			if(this.OnXmlSaveError != null)
			{
				this.OnXmlSaveError(e, stPath);
			}
		}

		private void raiseXmlLoadError(Exception e, string stPath)
		{
			if(this.OnXmlLoadError != null)
			{
				this.OnXmlLoadError(e, stPath);
			}
		}

		public void SaveXML(object o, string stPath)
		{
			try
			{
				FileStream fs = new FileStream(stPath, FileMode.Create);
				StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
				xmlSerializer.Serialize(sw, o);
				sw.Flush();
				sw.Close();
			}
			catch (Exception e)
			{
				this.raiseXmlSaveError(e, stPath);
				//TODO: add a line to an error log
			}	
		}

		public object LoadXML(string stPath)
		{
			object obj = null;
			try
			{
				FileStream fs = new FileStream(stPath, FileMode.Open);
				XmlTextReader xtr = new XmlTextReader(fs);
				obj = xmlSerializer.Deserialize(xtr);
				fs.Flush();
				fs.Close();
				xtr.Close();
				return obj;
			} 
			catch (Exception e)
			{
				this.raiseXmlLoadError(e, stPath);
				//TODO: add a line to an error log
			}
			return obj;
		}
	}
}
