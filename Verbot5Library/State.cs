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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace Conversive.Verbot5
{
	/// <summary>
	/// Contains the user's state information.
	/// </summary>
	public class State
	{
        public List<string> CurrentKBs = new List<string>();
		public Hashtable Vars = new Hashtable();
		public string Lastfired = "";
		public string Lastinput = "";
		public DateTime LastRefreshedTime = DateTime.Now;//this only applies to Verbots Online
		
		public void LoadVars(string filepath)
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream fs = null;
			try
			{
				fs = new FileStream(filepath, FileMode.Open);
				this.Vars = (Hashtable)bf.Deserialize(fs);
			} 
			catch {}
			finally
			{
				if(fs != null)
				{
					fs.Close();
				}
			}
		}
		
		public void SaveVars(string filepath)
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream fs = null;
			try
			{
				fs = new FileStream(filepath, FileMode.Create);
				bf.Serialize(fs, this.Vars);
			}
			catch {}
			finally
			{
				if(fs != null)
				{
					fs.Close();
				}
			}
		}
	}

}
