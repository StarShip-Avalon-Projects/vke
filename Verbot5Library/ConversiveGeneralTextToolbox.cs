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
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Conversive.Verbot5
{
	/// <summary>
	/// Toolbox of common functions.
	/// </summary>
	public class ConversiveGeneralTextToolbox
	{
		public static string CleanCSVValue(string stValue, char fieldDelimiter, char textDelimiter)
		{
			stValue = stValue.Replace("\n", "");
			stValue = stValue.Replace("\r", "");
			stValue = stValue.Replace(textDelimiter.ToString(), "");
			stValue = stValue.Replace(fieldDelimiter.ToString(), "");
			return stValue;
		}

		public static string MakeCSV(List<Dictionary<string, string>> data, char fieldDelimiter, char textDelimiter)
		{
			return ConversiveGeneralTextToolbox.MakeCSV(data, fieldDelimiter, textDelimiter, true);
		}

        public static string MakeCSV(List<Dictionary<string, string>> data, char fieldDelimiter, char textDelimiter, bool bAddHeaderRow)
		{
			//data is an List of String Dictionaries, the keys of the first item will be the headings
			StringBuilder sb = new StringBuilder();

            for (int i = 0; i < data.Count; i++)
            {
                string stRow = "";
                Dictionary<string, string> row = data[i];
                if (bAddHeaderRow && i == 0)//add header row
                {
                    foreach (string stKey in row.Keys)
                    {
                        if (stRow != "")
                            stRow += fieldDelimiter;
                        stRow += textDelimiter + stKey + textDelimiter;
                    }
                    if (stRow != "")
                    {
                        sb.Append(stRow + "\r\n");
                        stRow = "";
                    }
                }
                foreach (string stKey in row.Keys)
                {
                    if (stRow != "")
                        stRow += fieldDelimiter;
                    stRow += textDelimiter + CleanCSVValue(row[stKey], fieldDelimiter, textDelimiter) + textDelimiter;
                }
                if (stRow != "")
                    sb.Append(stRow + "\r\n");
            }

			return sb.ToString();
		}

		public static List<List<string>> SplitCSV(string data, char fieldDelimiter, char textDelimiter)
		{
			//notes: fields don't need "'s around them unless they have ,'s or \n's
			//reference: http://www.creativyst.com/Doc/Articles/CSV/CSV01.htm
			//sample line => "Last, First",,27,m
			//should become => {"Last, First", "", "27", "m"}

			List<List<string>> lines = new List<List<string>>();
			List<string> fields = new List<string>();

			int startIndex = 0;

			while(startIndex < data.Length)
			{
				//skip white space
				while(startIndex < data.Length &&
					(data[startIndex] == ' ' && fieldDelimiter != ' ' ||
					data[startIndex] == '\t' && fieldDelimiter != '\t'))
				{
					startIndex++;
				}

				//check for end of data
				if(startIndex >= data.Length)
				{
					if(fields.Count > 0)
						lines.Add(fields);
					break;//return
				}

					//check for end of line
				else if(data[startIndex] == '\n' || data[startIndex] == '\r')
				{
					if(fields.Count > 0)
					{
						lines.Add(fields);
						fields = new List<string>();
					}
					startIndex++;
					continue;//get the next line
				}

					//check for empty field
				else if(data[startIndex] == fieldDelimiter)
				{
					fields.Add("");
					startIndex++;
					continue;
				}

					//check for textDelimiter
				else if(data[startIndex] == textDelimiter)
				{
					//read quoted text field
					StringBuilder field = new StringBuilder();
					startIndex++;
					while(startIndex + 1 < data.Length && data[startIndex] != textDelimiter && data[startIndex] != '\n' && data[startIndex] != '\r')
					{
						field.Append(data[startIndex]);
						startIndex++;
					}
					fields.Add(field.ToString());
					startIndex += 2;//skip the textDelimiter and the fieldDelimiter
					continue;
				}
				else
				{
					//read unquoted field
					StringBuilder field = new StringBuilder();
					while(startIndex < data.Length && data[startIndex] != fieldDelimiter && data[startIndex] != '\n' && data[startIndex] != '\r')
					{
						field.Append(data[startIndex]);
						startIndex++;
					}					
					fields.Add(field.ToString().Trim());
					startIndex++;
					continue;
				}//else it's an unquoted field
			}//while not done
			if(fields.Count > 0)
				lines.Add(fields);
			return lines;

		}//SplitCSVLine(string line, char fieldDelimiter, char textDelimiter)

		public static string GetMD5String(string text)
		{
			MD5 md5 = MD5.Create();
			return byteArrayToString(md5.ComputeHash(Encoding.UTF8.GetBytes(text)));
		}

		private static string byteArrayToString(byte[] bytes)
		{
			StringBuilder sb = new StringBuilder(bytes.Length);
			for (int i = 0; i < bytes.Length; i++)
			{
				sb.Append(bytes[i].ToString("x2"));
			}
			return sb.ToString();
		}//byteArrayToString(byte[] bytes)
	}
}
