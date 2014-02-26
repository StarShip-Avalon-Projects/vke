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

namespace Conversive.Verbot5
{
	/// <summary>
	/// Logs errors to a text file or the event registry.
	/// </summary>
	public class ErrorLogger
	{

		public static void SendToLogFile(string stPath, string stToLog)
		{
			try
			{
				stToLog = DateTime.Now.ToString("s") + ": " + stToLog + "\r\n";
				FileStream fs = new FileStream(stPath, FileMode.Append);
				StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
				sw.Write(stToLog);
				sw.Flush();
				sw.Close();
			}
			catch(Exception e)
			{
				SendToEventLog("Log File Error: " + e.ToString() + "\r\n" + e.StackTrace);
			}
		}

		public static void SendToEventLog(string stErrorMessage)
		{
			string sSource = "Verbot5";
			string sLog = "Application";
			string sEvent = stErrorMessage;

			if (!System.Diagnostics.EventLog.SourceExists(sSource))
				System.Diagnostics.EventLog.CreateEventSource(sSource,sLog);

			System.Diagnostics.EventLog.WriteEntry(sSource, sEvent);
			//System.Diagnostics.EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Warning, 234);
		}
	}//class ErrorLogger
}//namespace Conversive.Verbot5
