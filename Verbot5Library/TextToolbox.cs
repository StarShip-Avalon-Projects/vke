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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace Conversive.Verbot5
{
	/// <summary>
	/// Toolbox of text manipulation functions.
	/// </summary>
	public class TextToolbox
	{
		static Random Random = new Random();

		public static string GetNewId()
		{
			return (TextToolbox.Random.Next(int.MinValue, int.MaxValue).ToString("X") + TextToolbox.Random.Next(int.MinValue, int.MaxValue).ToString("X"));
		}

		public static string ReplaceOnInput(string text, List<InputReplacement> replacements)
		{
			bool bIsCapture = false;
			return ReplaceOnInput(text, replacements, out bIsCapture);
		}

		/// <summary>
		/// Get the extension for the file
		/// </summary>
		/// <param name="stFileNameOrFullPath"></param>
		/// <returns>To Lower of the file extension with the period (i.e. ".ckb")</returns>
		public static string GetFileExtension(string stFileNameOrFullPath)
		{
			string stRet = stFileNameOrFullPath;
			if(stRet.IndexOf(".") != -1)
			{
				stRet = stRet.Substring(stRet.LastIndexOf(".")).ToLower();
			}
			return stRet;
		}

		/// <summary>
		/// Uses given InputReplacements to change text
		/// </summary>
		/// <param name="text">change text</param>
		/// <param name="inputReplacements">list of Replacements</param>
		/// <returns>resulting replaced text</returns>
		public static string ReplaceOnInput(string text, List<InputReplacement> replacements, out bool bIsCapture)
		{
			StringBuilder sb = new StringBuilder();
			if(text.IndexOf("[") == -1)
			{
				bIsCapture = false;
				if(replacements.Count > 0)
				{
					int depth = 0;
					for(int i = 0; i < text.Length; i++)
					{
						bool replaced = false;
						if(text[i] == '[')
						{
							depth++;
						}
						else if(text[i] == ']')
						{
							depth--;
						}
						else if(depth == 0)
						{
							foreach(InputReplacement ir in replacements)
							{
								if(i < text.Length - ir.TextToFind.Length + 1 
									&& ir.TextToFind == text.Substring(i, ir.TextToFind.Length))
								{
									replaced = true;
									sb.Append(ir.TextToInput);
									i += ir.TextToFind.Length - 1;
									break;//can only replace with one thing
								}
							}//foreach(InputReplacement repl in inputReplacements)
						}
						if(!replaced)
							sb.Append(text[i]);
					}//for(int i = 0; i < text.Length; i++)
				}
				else//if we don't have any input replacements, just do the default (remove accents)
				{
					//TODO: add if(prefs.useDefaultInputReplacements)
					sb.Append(TextToolbox.ApplyDefaultInputReplacements(text));
				}
			}
			else
			{
				bIsCapture = true;
				sb.Append(text);//if it is a capture, we won't replace anything
			}

			return sb.ToString();
        }//ReplaceOnInput(string text, List<InputReplacement> replacements)

		public static string ReplaceSynonyms(string input, Dictionary<string, Synonym> synonyms)
		{
			int start = input.IndexOf("(");
			int end;

			while(start != -1)
			{
				end = TextToolbox.FindNextMatchingChar(input, start);
				if(end != -1)
				{
					string synonymName = input.Substring(start + 1, end - start - 1).Trim().ToLower();
					if(synonyms.ContainsKey(synonymName))
						input = input.Substring(0, start + 1) + ((Synonym)synonyms[synonymName]).GetPhrases() + input.Substring(end);
					else
						input = input.Substring(0, start) + "(" + synonymName + ")" + input.Substring(end + 1);
					start = input.IndexOf("(", start + 1);
				}
				else//there's an error with this input, it won't compile
				{					
					//TODO: log an error somewhere?
					return "";
				}
			}
			return input;
        }//replaceSynonyms(string input, Dictionary<string, Synonym> synonymGroups)

		public static string ReplaceOutputSynonyms(string text, Dictionary<string, Synonym> synonyms)
		{
			int start = text.IndexOf("(");
			int end;

			while(start != -1)
			{
				if(!TextToolbox.IsEscaped(text, start))
				{
					end = TextToolbox.FindNextMatchingChar(text, start);
					if(end != -1)
					{
						string synonymName = text.Substring(start + 1, end - start - 1).Trim().ToLower();
						if(synonyms.ContainsKey(synonymName) && !TextToolbox.IsInCommand(text, start, end-start))
						{
							Synonym syn = ((Synonym)synonyms[synonymName]);
							if(syn.Phrases.Count > 0)
							{
								Random rand = new Random();
								int pick = rand.Next(syn.Phrases.Count);
								if(end == text.Length - 1)//the end is the end
									text = text.Substring(0, start) + ((Phrase)syn.Phrases[pick]).Text;
								else
									text = text.Substring(0, start) + ((Phrase)syn.Phrases[pick]).Text + text.Substring(end + 1);
							}
							else
							{
								text = text.Substring(0, start + 1) + synonymName + text.Substring(end);
							}
						}
						start = text.IndexOf("(", start + 1);
					}
					else
					{					
						//TODO: log an error somewhere?
						return text;
					}
				}
				else //it's escaped \(..
				{
					//remove the '\'
					text = text.Remove(start-1, 1);
					//find the next start
					if(start >= text.Length - 1)//the old start is now at the end
						start = -1;
					else
						start = text.IndexOf("(", start);
				}
			}//while
			return text;
        }//replaceSynonyms(string input, Dictionary<string, Synonym> synonymGroups)
		
		public static string ReplaceVars(string text, Hashtable vars)
		{
			int start = text.IndexOf("[");
			int end;
			while(start != -1)
			{
				if(!TextToolbox.IsEscaped(text, start))
				{
					end = TextToolbox.FindNextMatchingChar(text, start);
					if(end != -1)
					{
						string varName = TextToolbox.ReplaceVars(text.Substring(start + 1, end - start - 1), vars);
						string varDefaultValue = "";
						if(varName.IndexOf(':') != -1)
						{
							varDefaultValue = varName.Substring(varName.IndexOf(':') + 1).Trim();
							varName = varName.Substring(0, varName.IndexOf(':')).Trim();
						}
						varName = varName.Replace(" ", "_");
						string varValue = (string)vars[varName.ToLower()];
						if(varValue == null)
							varValue = varDefaultValue;

						if(end == text.Length - 1)//var runs to the end
						{
							text = text.Substring(0, start) + varValue;
							start = -1;
						}
						else
						{
							text = text.Substring(0, start) + varValue + text.Substring(end + 1);
							start = text.IndexOf("[", start + varValue.Length);
						}
					}
					else//there's an error with this input, it won't compile
					{					
						//TODO: log an error somewhere?
						return "";
					}
				}
				else// [ is escaped
				{
					//remove the '\'
					text = text.Remove(start-1, 1);
					//find the next start
					if(start >= text.Length - 1)//the old start is now at the end
						start = -1;
					else
						start = text.IndexOf("[", start);
				}
			}//while we have more [vars]

			string embCmd = "<mem.get ";
			start = text.IndexOf(embCmd);
			while(start != -1)
			{
				if(!TextToolbox.IsEscaped(text, start))
				{
					end = TextToolbox.FindNextMatchingChar(text, start);
					if(end != -1)
					{
						string name = text.Substring(start + embCmd.Length, end - start - embCmd.Length);
						string left = "";
						if(start > 0)
							left = text.Substring(0, start);
						string right = "";
						if(end + 1 != text.Length)
							right = text.Substring(end + 1);
						text = left + vars[name.ToLower()] + right;
					}
					start = text.IndexOf(embCmd);
				}
				else// < is escaped
				{
					//remove the '\'
					text = text.Remove(start-1, 1);
					//find the next start
					if(start >= text.Length - 1)//the old start is now at the end
						start = -1;
					else
						start = text.IndexOf(embCmd, start);
				}

			}//while we have more <mem.get name>s

			embCmd = "<mem.set ";
			start = text.IndexOf(embCmd);
			while(start != -1)
			{
				if(!TextToolbox.IsEscaped(text, start))
				{
					end = TextToolbox.FindNextMatchingChar(text, start);
					if(end != -1)
					{
						string nameValue = text.Substring(start + embCmd.Length, end - start - embCmd.Length);
						string left = "";
						string right = "";
						string[] cmdArgs = TextToolbox.splitOnFirstUnquotedSpace(nameValue);
						
						if(cmdArgs.Length > 1)
						{
							string name = cmdArgs[0];
							//remove quotes if they are there
							if(name.Length > 1 && name[0] == '"')
								name = name.Substring(1);
							if(name.Length > 2 && name[name.Length-1]  == '"')
								name = name.Substring(0, name.Length - 1);
							string val = cmdArgs[1];
							vars[name.ToLower()] = val;
						}

						if(start > 0)
							left = text.Substring(0, start);	
						if(end + 1 != text.Length)
							right = text.Substring(end + 1);					
						text = left + right;
					}
					start = text.IndexOf(embCmd);
				}
				else// < is escaped
				{
					//remove the '\'
					text = text.Remove(start-1, 1);
					//find the next start
					if(start >= text.Length - 1)//the old start is now at the end
						start = -1;
					else
						start = text.IndexOf(embCmd, start);
				}
			}//while we have more <mem.set name>s

			embCmd = "<mem.del ";
			start = text.IndexOf(embCmd);
			while(start != -1)
			{
				if(!TextToolbox.IsEscaped(text, start))
				{
					end = TextToolbox.FindNextMatchingChar(text, start);
					if(end != -1)
					{
						string name = text.Substring(start + embCmd.Length, end - start - embCmd.Length);
						vars.Remove(name.Trim().ToLower());
						string left = "";
						string right = "";
						if(start > 0)
							left = text.Substring(0, start);	
						if(end + 1 != text.Length)
							right = text.Substring(end + 1);					
						text = left + right;
					}
					start = text.IndexOf(embCmd);
				}
				else// < is escaped
				{
					//remove the '\'
					text = text.Remove(start-1, 1);
					//find the next start
					if(start >= text.Length - 1)//the old start is now at the end
						start = -1;
					else
						start = text.IndexOf(embCmd, start);
				}
			}//while we have more <mem.set name>s
			return text;
		}//ReplaceVars(string text, Hashtable vars)

		public static string TextToPattern(string stPat)
		{
			/*
				"quoted text" doesn't get changed
				* => .*?
				[.,;:!?] => NOTHING
				\WSPACE\W => .+?
				\WSPACE\w => .+?\b
				\wSPACE\W => \b.+?
				\wSPACE\w => \b.+?\b
				[blaw] => (?<blaw>.+)
				^| => ^
				^\w => ^.*?\b
				^\W => ^.*?
				|$ => $
				\w$ => \b.*?$
				\W$ => .*?$
							
			*/
			bool inQuote = false;
			int varDepth = 0;
			StringBuilder pat = new StringBuilder(stPat);
			//nonWord matches to non-word characters other than parens
			System.Text.RegularExpressions.Regex nonWord = new System.Text.RegularExpressions.Regex(@"[^\w\(\)]");

			bool regexVar = false;

			//handle special characters
			for(int j = 0; j < pat.Length; j++)
			{
				//ignore quoted sections
				if(pat[j] == '"' && !IsEscaped(pat, j))
					inQuote = !inQuote;

				if(inQuote || IsEscaped(pat, j))
					continue;

				if(pat[j] == '*')//wildcard
				{
					pat.Replace("*", ".*?", j, 1);
					j += 2;
				}
				else if(pat[j] == '.' || pat[j] == '?' || pat[j] == '+')
				{
					pat.Insert(j, '\\');
					j += 1;
				}
				else if(pat[j] == ' ' && (j <= 3 || pat.ToString().Substring(j-4, 4) != ">.+)") && (j == pat.Length - 1 || pat[j+1] != '['))
				{
					string replacement = "_";
					if(varDepth == 0)
					{
						if(j == 0 || nonWord.IsMatch(pat[j-1].ToString()))//starts with nothing or a non-word
						{
							if(j == pat.Length - 1 || nonWord.IsMatch(pat[j+1].ToString()))//ends with nothing or a non-word
								replacement = @".+?";
							else//ends with word character
								replacement = @".+?\b";
						}
						else//starts with a word character
						{
							if(j == pat.Length - 1 || nonWord.IsMatch(pat[j+1].ToString()))//ends with nothing or a non-word
								replacement = @"\b.+?";
							else//ends with word character
								replacement = @"\b.+?\b";
						}
					}//end if not in a var
					
					if(!regexVar)
					{
						pat.Replace(" ", replacement, j, 1);
						j += (replacement.Length - 1);
					}					
				}//end pat[j] == ' '
				else if(pat[j] == '[')
				{
					regexVar = false;
					varDepth++;
					if(varDepth == 1)
					{
						pat.Replace("[", "(?<", j, 1);
						j += 2;
					}
					else
					{
						pat.Replace("[", "_s_", j, 1);
						j += 1;
					}
				}
				else if(pat[j] == '=' && varDepth > 0)
				{
					pat.Replace("=", ">", j, 1);
					regexVar = true;
				}
				else if(pat[j] == ']')
				{
					varDepth--;
					if(varDepth == 0)
					{
						if(regexVar)
						{
							pat.Replace("]", ")", j, 1);
						}
						else
						{
							pat.Replace("]", ">.+)", j, 1);
							j += 3;
						}
					}
					else
					{
						pat.Replace("]", "_e_", j, 1);
					}
					regexVar = false;
				}
			}

			//remove quotes
			for(int j = 0; j < pat.Length; j++)
			{
				if(pat[j] == '"')
				{
					if(IsEscaped(pat, j))
					{
						pat.Remove(j-1, 1);
					}
					else
					{
						pat.Remove(j, 1);
					}
					j--;
				}
			}
			//add initial and trailing wildcards
			stPat = pat.ToString();
			if(stPat.IndexOf("(?<") != 0 && stPat.IndexOf("|") != 0)
			{
				stPat = @"(|.*?\b|.*?\s)" + stPat;
			}
			if(stPat.LastIndexOf(">.*?)") != stPat.Length - 6 && stPat.LastIndexOf("|") != stPat.Length - 1)
			{
				stPat += @"(|\b.*?|\s.*?)";
			}

			pat = new StringBuilder(stPat);
			//remove the pipes (walls)
			if(pat.Length > 0 && pat[0] == '|')
				pat.Remove(0, 1);
			if(pat.Length > 0 && pat[pat.Length-1] == '|')
				pat.Remove(pat.Length-1, 1);

			pat.Insert(0, '^');
			pat.Append('$');
		
			return pat.ToString();
		}//TextToPattern(string stPat)

		public static string[] splitOnFirstUnquotedSpace(string text)
		{
			string[] pieces = new string[2];
			int index = text.IndexOf(" ");
			//find the right place to break
			while(index != -1 && IsQuoted(text, index, 1))
			{
				if(index < text.Length - 1)
					index = text.IndexOf(" ", index + 1);
				else
					index = -1;
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

		public static bool IsQuoted(string text, int start, int length)
		{
			//returns true if any of the characters from start to start + length are quoted

			bool inQuote = false;
			for(int i = 0; i < start + length; i++)
			{
				if(text[i] == '"' && !IsEscaped(text, i))
					inQuote = !inQuote;
				if(i >= start && inQuote)
					return true;
			}
			return false;
		}
		
		public static bool IsInCommand(string text, int start, int length)
		{
			//returns true if any of the characters from start to start + length are in a command

			char startCommand = '<';
			char endCommand = '>';
			int depth = 0;
			for(int i = 0; i < start + length; i++)
			{
				if(text[i] == startCommand && !IsEscaped(text, i))
					depth++;
				else if(text[i] == endCommand && !IsEscaped(text, i))
					depth--;
				if(i >= start && depth > 0)
					return true;
			}
			return false;
		}//IsInCommand(string text, int start, int length)

		public static bool IsEscaped(string text, int index)
		{
			// Examples:
			// IsExcaped( hello , 2) -> false
			// IsEscaped( test "this" , 5) -> false
			// IsEscaped( test \"this\", 6) -> true
			// IsEscaped( test \\"this", 7) -> false
			// IsEscaped( test \\\"this", 8) -> true
			if(index == 0 || text[index - 1] != '\\')
				return false;
			else//the previous index is a backslash
				return !IsEscaped(text, index - 1);
		}

		public static bool IsEscaped(StringBuilder text, int index)
		{
			// Examples:
			// IsExcaped( hello , 2) -> false
			// IsEscaped( test "this" , 5) -> false
			// IsEscaped( test \"this\", 6) -> true
			// IsEscaped( test \\"this", 7) -> false
			// IsEscaped( test \\\"this", 8) -> true
			if(index == 0 || text[index - 1] != '\\')
				return false;
			else//the previous index is a backslash
				return !IsEscaped(text, index - 1);
		}//IsEscaped(StringBuilder text, int index)

		public static List<InputReplacement> GetDefaultInputReplacements()
		{
            List<InputReplacement> alRet = new List<InputReplacement>();
			alRet.Add(new InputReplacement("À", "A"));
			alRet.Add(new InputReplacement("Á", "A"));
			alRet.Add(new InputReplacement("Â", "A"));
			alRet.Add(new InputReplacement("Ã", "A"));
			alRet.Add(new InputReplacement("Ä", "A"));
			alRet.Add(new InputReplacement("Å", "A"));
			alRet.Add(new InputReplacement("à", "a"));
			alRet.Add(new InputReplacement("á", "a"));
			alRet.Add(new InputReplacement("â", "a"));
			alRet.Add(new InputReplacement("ã", "a"));
			alRet.Add(new InputReplacement("ä", "a"));
			alRet.Add(new InputReplacement("å", "a"));
			alRet.Add(new InputReplacement("Æ", "AE"));
			alRet.Add(new InputReplacement("æ", "ae"));
			alRet.Add(new InputReplacement("Ç", "C"));
			alRet.Add(new InputReplacement("ç", "c"));
			alRet.Add(new InputReplacement("È", "E"));
			alRet.Add(new InputReplacement("É", "E"));
			alRet.Add(new InputReplacement("Ê", "E"));
			alRet.Add(new InputReplacement("Ë", "E"));
			alRet.Add(new InputReplacement("è", "e"));
			alRet.Add(new InputReplacement("é", "e"));
			alRet.Add(new InputReplacement("ê", "e"));
			alRet.Add(new InputReplacement("ë", "e"));
			alRet.Add(new InputReplacement("Ì", "I"));
			alRet.Add(new InputReplacement("Í", "I"));
			alRet.Add(new InputReplacement("Î", "I"));
			alRet.Add(new InputReplacement("Ï", "I"));
			alRet.Add(new InputReplacement("ì", "i"));
			alRet.Add(new InputReplacement("í", "i"));
			alRet.Add(new InputReplacement("î", "i"));
			alRet.Add(new InputReplacement("ï", "i"));
			alRet.Add(new InputReplacement("Ñ", "N"));
			alRet.Add(new InputReplacement("ñ", "n"));
			alRet.Add(new InputReplacement("Ò", "O"));
			alRet.Add(new InputReplacement("Ó", "O"));
			alRet.Add(new InputReplacement("Ô", "O"));
			alRet.Add(new InputReplacement("Õ", "O"));
			alRet.Add(new InputReplacement("Ö", "O"));
			alRet.Add(new InputReplacement("ò", "o"));
			alRet.Add(new InputReplacement("ó", "o"));
			alRet.Add(new InputReplacement("ô", "o"));
			alRet.Add(new InputReplacement("õ", "o"));
			alRet.Add(new InputReplacement("ö", "o"));
			alRet.Add(new InputReplacement("Ù", "U"));
			alRet.Add(new InputReplacement("Ú", "U"));
			alRet.Add(new InputReplacement("Û", "U"));
			alRet.Add(new InputReplacement("Ü", "U"));
			alRet.Add(new InputReplacement("ù", "u"));
			alRet.Add(new InputReplacement("ú", "u"));
			alRet.Add(new InputReplacement("û", "u"));
			alRet.Add(new InputReplacement("ü", "u"));
			alRet.Add(new InputReplacement("Ý", "Y"));
			alRet.Add(new InputReplacement("ý", "y"));

			alRet.Add(new InputReplacement(".", ""));
			alRet.Add(new InputReplacement("!", ""));
			alRet.Add(new InputReplacement("?", ""));
			alRet.Add(new InputReplacement(",", ""));
			alRet.Add(new InputReplacement("\"", ""));
			alRet.Add(new InputReplacement("'", ""));
			alRet.Add(new InputReplacement(":", ""));
			alRet.Add(new InputReplacement(";", ""));

			return alRet;
		}

		public static string ApplyDefaultInputReplacements(string text)
		{
			//ÀÁÂÃÄÅàáâãäåÆæÇçÈÉÊËèéêëÌÍÎÏìíîïÑñÒÓÔÕÖòóôõöÙÚÛÜùúûüÝýÿ.!?,"':;
			StringBuilder sb = new StringBuilder(text);
            List<InputReplacement> alDefaultInputReplacements = TextToolbox.GetDefaultInputReplacements();
			foreach(InputReplacement ir in alDefaultInputReplacements)
			{
				sb.Replace(ir.TextToFind, ir.TextToInput);
			}

			return sb.ToString();
		}//ApplyDefaultInputReplacements(string text)

		public static int FindNextUnexcapedChar(string text, char ch)
		{
			return TextToolbox.FindNextUnescapedChar(text, ch, 0);
		}

		public static int FindNextUnescapedChar(string text, char ch, int start)
		{
			for(int i = start; i < text.Length; i++)
			{
				if(text[i] == ch && !TextToolbox.IsEscaped(text, i))
					return i;
			}
			return -1;
		}

		public static int FindNextMatchingChar(string text, int index)
		{
			if(index > -1 && index < text.Length)
			{
				int depth = 1;
				char openChar;
				char closeChar;
				switch(text[index])
				{
					case '[':
						openChar = '[';
						closeChar = ']';
						break;
					case '(':
						openChar = '(';
						closeChar = ')';
						break;
					case '<':						
						openChar = '<';
						closeChar = '>';
						break;
					case '{':
						openChar = '{';
						closeChar = '}';
						break;
					default:
						return -1;
				}//switch
				for(int i = index + 1; i < text.Length; i++)
				{
					if(text[i] == closeChar)
					{
						depth--;
						if(depth == 0)
							return i;
					}
					else if(text[i] == openChar)
					{
						depth++;
					}					
				}//for each char after the index
				return -1;
			}//end if within bounds
			return -1;
		}//FindNextMatch(string text, int start)

		public static string AddCarriageReturns(string text)
		{
			int index = text.IndexOf('\n');
			if(index != -1 && (index == 0 || text[index-1] != '\r'))
				text = text.Replace("\n", "\r\n");
			return text;
		}//AddCarriageReturns(string text)

		/// <summary>
		/// Generates a list of word permutations from stInput
		/// </summary>
		/// <param name="stInput">stInput to generates inputs from</param>
		/// <returns>list of word permutations</returns>
        public static List<string> GetWordPermutations(string stInput)
		{
			return GetWordPermutations(stInput, -1);
		}//GetWordPermutations(string stInput)
		
		/// <summary>
		/// Generates a list of word permutations from stInput up to strings with maxSize number of words
		/// </summary>
		/// <param name="stInput">stInput to generates inputs from</param>
		/// <param name="maxSize">maximum number of words in each permutation</param>
		/// <returns>list of word permutations</returns>
        public static List<string> GetWordPermutations(string stInput, int maxSize)
		{
            List<string> alRet = new List<string>();
							   
			if(stInput == null)
				return alRet;

			//do this replacement so that we can find synonyms on
			//the right side of the = in a capture
			stInput = stInput.Replace("=", "= ");
			stInput = stInput.Replace("]", " ]");

			string[] stArr = stInput.Split(' ');
			StringBuilder sb = new StringBuilder();

			if(maxSize == -1 || maxSize > stArr.Length)
				maxSize = stArr.Length;

			for(int size = maxSize; size > 0 ; size--)//how many words to get
			{
				for(int start = 0; start <= (stArr.Length - size); start++) //index to start at
				{
					sb.Remove(0, sb.Length);
					for(int i = start; i < start + size; i++)
					{
						if(sb.Length > 0)
							sb.Append(" ");
						sb.Append(stArr[i]);
					}
					string part = sb.ToString().Trim();
					if(part != "" && !alRet.Contains(part))
						alRet.Add(part);
				}
			}
			return alRet;
		}//GetWordPermutations(string stInput, int maxSize)

        public static List<SynonymNode> GetAllAvailableSynonymNodes(List<string> splits, Dictionary<string, List<SynonymNode>> synWordIndex)
		{
			return GetAllAvailableSynonymNodes(splits, synWordIndex, null);
        }//GetAllAvailableSynonymNodes(List<string> splits, Dictionary<string, List<SynonymNode>> synWordIndex)

        public static List<SynonymNode> GetAllAvailableSynonymNodes(List<string> splits, Dictionary<string, List<SynonymNode>> synWordIndex, string idNotToAdd)
		{
            List<SynonymNode> synonymNodes = new List<SynonymNode>();

			foreach(string split in splits)
			{
				if(synWordIndex.ContainsKey(split))
				{
					foreach(SynonymNode sn in synWordIndex[split])
					{
						if(!sn.MatchedPhrases.Contains(split.ToLower()))
							sn.MatchedPhrases.Add(split.ToLower());
						if(sn.Id != idNotToAdd && !synonymNodes.Contains(sn))
							synonymNodes.Add(sn);
					}
				}//foreach(string split in splits)
			}

			return synonymNodes;
        }//GetAllAvailableSynonymNodes(List<string> splits, Dictionary<string, List<SynonymNode>> synWordIndex, string idNotToAdd)

		public static string FetchWebPage(string url)
		{
			string page = "";
			try
			{
				byte[] buf = new byte[1024 * 8];

				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				Stream stream = response.GetResponseStream();

				int count = stream.Read(buf, 0, buf.Length);

				page = Encoding.UTF8.GetString(buf, 0, count);

				stream.Close(); 
			}
			catch(Exception e)
			{
				string st = e.ToString();
			}
			return page;
		}//FetchWebPage(string url)
	}//class TextToolbox

	public class SynonymNode : System.Windows.Forms.TreeNode, IComparable
	{
		public string Name;
		public string Id;

		public List<string> MatchedPhrases;

		public SynonymNode(string id, string name)
		{
			this.Id = id;
			this.Name = name;
			this.Text = name;
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;

			this.MatchedPhrases = new List<string>();
		}

		public override string ToString()
		{
			return this.Text;
		}

		public override bool Equals(Object other)
		{
			SynonymNode snOther = (SynonymNode)other;
			if(snOther.Id == this.Id && snOther.Name == this.Name)
				return true;
			else
				return false;
		}

		public override int GetHashCode()
		{
			return (Id + Name).GetHashCode();
		}


		public int CompareTo(Object other)
		{
			SynonymNode snOther = (SynonymNode)other;
			return (this.Name.CompareTo(snOther.Name));
		}
	}//class SynonymNode
}//namespace Conversive.Verbot5
