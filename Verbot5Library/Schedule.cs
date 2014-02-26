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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Verbot5Library
{
	/// <summary>
	/// Like Cron, let's you schedule inputs.
	/// </summary>
	public class Schedule
	{
		public List<Event> events;
		public Schedule()
		{
			this.events = new List<Event>();
		}

		public Schedule(string filepath)
		{
			this.events = new List<Event>();
			this.Load(filepath);
		}

		public void Load(string filepath)
		{
			try
			{
				FileStream fs = File.OpenRead(filepath);
				char ch = 'a';//the current character
				StringBuilder line = new StringBuilder();
				string stLine;
				bool inComment = false;
				while((int)ch != 65535)//! use EOF?
				{
					ch = (char)fs.ReadByte();
					if(ch == '\n' || ch == '\r' || (int)ch == 65535)
					{
						stLine = line.ToString().Trim();
						if(stLine != "")
							this.processLine(stLine);
						inComment = false;
						line.Remove(0, line.Length);
					}
					else if(ch == '#')
					{
						inComment = true;
					}
					else if(!inComment)
					{
						line.Append(ch);
					}
				}//while((int)ch != 65535)
			} 
			catch{}
		}//Load(string filepath)

		private void processLine(string stLine)
		{
			Regex linePattern = new Regex(@"(?<min>\S+)\s+(?<hr>\S+)\s+(?<dom>\S+)\s+(?<mon>\S+)\s+(?<dow>\S+)\s+(?<text>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			Match match = linePattern.Match(stLine);
			GroupCollection gc = match.Groups;
			if(gc.Count == 7)
			{
				int min = -1;
				try	
				{
					min = Int32.Parse(gc["min"].Value);
				} 
				catch {}
				int hr = -1;
				try	
				{
					hr = Int32.Parse(gc["hr"].Value);
				} 
				catch {}
				int dom = -1;
				try	
				{
					dom = Int32.Parse(gc["dom"].Value);
				} 
				catch {}
				int mon = -1;
				try	
				{
					mon = Int32.Parse(gc["mon"].Value);
				} 
				catch {}
				int dow = -1;
				try	
				{
					dow = Int32.Parse(gc["dow"].Value);
				} 
				catch {}
				string text = gc["text"].Value;
				if(text != null)
				{
					Event e = new Event(min, hr, dom, mon, dow, text);
					this.events.Add(e);
				}
			}
		}//processLine(string stLine)

		public void AddEvent(int minute, int hour, int dayOfMonth, int month, int dayOfWeek, string text)
		{
			this.events.Add(new Event(minute, hour, dayOfMonth, month, dayOfWeek, text));
		}

		public string GetCurrentEvent()
		{
			foreach(Event e in this.events)
			{
				if(e.IsNow)
					return e.Text;
			}
			return null;
		}//GetCurrentEvent()

		public List<string> GetCurrentEvents()
		{
            List<string> texts = new List<string>();
			foreach(Event e in this.events)
			{
				if(e.IsNow)
					texts.Add(e.Text);
			}
			return texts;
		}//GetCurrentEvents()

		public class Event
		{
			public int Minute = -1;
			public int Hour = -1;
			public int DayOfMonth = -1;
			public int Month = -1;
			public int DayOfWeek = -1;
			public string Text = "";

			public Event(int minute, int hour, int dayOfMonth, int month, int dayOfWeek, string text)
			{
				this.Minute = minute;
				this.Hour = hour;
				this.DayOfMonth = dayOfMonth;
				this.Month = month;
				this.DayOfWeek = dayOfWeek;
				this.Text = text;
			}

			public bool IsNow
			{
				get
				{
					DateTime now = DateTime.Now;
					return (this.Minute == -1 || this.Minute == now.Minute)
						&& (this.Hour == -1 || this.Hour == now.Hour)
						&& (this.DayOfMonth == -1 || this.DayOfMonth == now.Day)
						&& (this.Month == -1 || this.Month == now.Month)//1 = Jan
						&& (this.DayOfWeek == -1 || this.DayOfWeek == (int)now.DayOfWeek);//0 = Sun
				}
			}
		}//class Event
	}//class Schedule
}//namespace Verbot5Library
