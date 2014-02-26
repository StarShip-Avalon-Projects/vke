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
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Conversive.Verbot5
{
	/// <summary>
	/// List of TTS Modes.
	/// </summary>
	public class TTSModes
	{
		[XmlArrayItem("TTSMode", typeof(TTSMode))]
		public List<TTSMode> TTSModesList;

		public TTSModes()
		{
            this.TTSModesList = new List<TTSMode>();
		}
	}

	public class TTSMode
	{
		[XmlElement("name", typeof(string))]
		public string Name;
		[XmlElement("id", typeof(string))]
		public string Id;

		public TTSMode()
		{
			this.Name = "";
			this.Id = "";
		}
	}

}
