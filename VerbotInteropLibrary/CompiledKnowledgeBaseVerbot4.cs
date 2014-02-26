using System;
using System.Collections.Generic;
using System.Text;

namespace VerbotInteropLibrary
{
    public class CompiledKnowledgeBaseVerbot4
    {
        public Conversive.Verbot4.CompiledKnowledgeBase CKB4;

        public CompiledKnowledgeBaseVerbot4(Conversive.Verbot4.CompiledKnowledgeBase ckb4) 
        {
            this.CKB4 = ckb4;
        }

        public Conversive.Verbot5.CompiledKnowledgeBase GetVerbot5CompiledKnowledgeBae()
        {
            if (this.CKB4 == null)
                return null;

            Conversive.Verbot5.CompiledKnowledgeBase ckb5 = new Conversive.Verbot5.CompiledKnowledgeBase();

            //cs toolbox
            if (this.CKB4.CSToolbox != null)
            {
                ckb5.CSToolbox = new Conversive.Verbot5.CSharpToolbox();
                foreach (Conversive.Verbot4.CodeModule cm in this.CKB4.CSToolbox.CodeModules)
                {
                    Conversive.Verbot5.CodeModule cm5 = new Conversive.Verbot5.CodeModule();
                    cm5.Name = cm.Name;
                    cm5.Language = Conversive.Verbot5.CodeLanguages.CSharp;
                    cm5.Includes = cm.Includes;
                    cm5.Vars = cm.Vars;
                    foreach (Conversive.Verbot4.Function f in cm.Functions)
                    {
                        Conversive.Verbot5.Function f5 = new Conversive.Verbot5.Function();
                        f5.Id = f.Id;
                        f5.Name = f.Name;
                        f5.ReturnType = f.ReturnType;
                        f5.Parameters = f.Parameters;
                        f5.Code = f.Code;
                        cm5.Functions.Add(f5);
                    }
                    ckb5.CSToolbox.CodeModules.Add(cm5);
                }
                if (this.CKB4.CSToolbox.Outputs != null)
                {
                    ckb5.CSToolbox.Outputs = new Dictionary<string,string>();
                    foreach (string key in this.CKB4.CSToolbox.Outputs.Keys)
                    {
                        ckb5.CSToolbox.Outputs.Add(key, this.CKB4.CSToolbox.Outputs[key].ToString());
                    }
                }
                if (this.CKB4.CSToolbox.Conditions != null)
                {
                    ckb5.CSToolbox.Conditions = new Dictionary<string, string>();
                    foreach (string key in this.CKB4.CSToolbox.Conditions.Keys)
                    {
                        ckb5.CSToolbox.Conditions.Add(key, this.CKB4.CSToolbox.Conditions[key].ToString());
                    }
                }

                ckb5.CSToolbox.Compile();
            }

            //private Dictionary<string, List<InputRecognizer>> inputs;
            if (this.CKB4.Inputs != null)
            {
                ckb5.Inputs = new Dictionary<string, List<Conversive.Verbot5.InputRecognizer>>();
                foreach (string key in this.CKB4.Inputs.Keys)
                {
                    List<Conversive.Verbot5.InputRecognizer> ir5List = new List<Conversive.Verbot5.InputRecognizer>();
                    foreach (Conversive.Verbot4.InputRecognizer ir in (System.Collections.ArrayList)this.CKB4.Inputs[key])
                    {
                        Conversive.Verbot5.InputRecognizer ir5 = new Conversive.Verbot5.InputRecognizer("", ir.RuleId, ir.InputId, ir.Condition, new Dictionary<string, Conversive.Verbot5.Synonym>(), new List<Conversive.Verbot5.InputReplacement>());
                        ir5.Regex = ir.Regex;
                        ir5.IsCapture = ir.IsCapture;
                        ir5.Length = ir.Length;
                        ir5List.Add(ir5);
                    }
                    ckb5.Inputs.Add(key, ir5List);
                }
            }

		    //private Dictionary<string, List<Output>> outputs;
            if (this.CKB4.Outputs != null)
            {
                ckb5.Outputs = new Dictionary<string, List<Conversive.Verbot5.Output>>();
                foreach (string key in this.CKB4.Outputs.Keys)
                {
                    List<Conversive.Verbot5.Output> outList = new List<Conversive.Verbot5.Output>();
                    foreach (Conversive.Verbot4.Output o in (System.Collections.ArrayList)this.CKB4.Outputs[key])
                    {
                        Conversive.Verbot5.Output o5 = new Conversive.Verbot5.Output();
                        o5.Id = o.Id;
                        o5.Text = o.Text;
                        o5.Condition = o.Condition;
                        o5.Cmd = o.Cmd;
                        outList.Add(o5);
                    }
                    ckb5.Outputs.Add(key, outList);
                }
            }

            //private Dictionary<string, Synonym> synonyms;
            if (this.CKB4.Synonyms != null)
            {
                ckb5.Synonyms = new Dictionary<string, Conversive.Verbot5.Synonym>();
                foreach (string key in this.CKB4.Synonyms.Keys)
                {
                    Conversive.Verbot4.Synonym s = (Conversive.Verbot4.Synonym)this.CKB4.Synonyms[key];
                    
                    Conversive.Verbot5.Synonym s5 = new Conversive.Verbot5.Synonym();
                    s5.Id = s.Id;
                    s5.Name = s.Name;
                    foreach (Conversive.Verbot4.Phrase p in s.Phrases)
                    {
                        Conversive.Verbot5.Phrase p5 = new Conversive.Verbot5.Phrase();
                        p5.Id = p.Id;
                        p5.Text = p.Text;
                        s5.Phrases.Add(p5);
                    }
                    
                    ckb5.Synonyms.Add(key, s5);
                }
            }

		    //private List<Replacement> replacements;
            if (this.CKB4.Replacements != null)
            {
                ckb5.Replacements = new List<Conversive.Verbot5.Replacement>();

                foreach (Conversive.Verbot4.Replacement r in this.CKB4.Replacements)
                {
                    Conversive.Verbot5.Replacement r5 = new Conversive.Verbot5.Replacement();
                    r5.TextForAgent = r.TextForAgent;
                    r5.TextForOutput = r.TextForOutput;
                    r5.TextToFind = r.TextToFind;
                    ckb5.Replacements.Add(r5);
                }
            }
        

		    //private List<InputReplacement> inputReplacements;
            if (this.CKB4.InputReplacements != null)
            {
                ckb5.InputReplacements = new List<Conversive.Verbot5.InputReplacement>();

                foreach (Conversive.Verbot4.InputReplacement ir in this.CKB4.InputReplacements)
                {
                    Conversive.Verbot5.InputReplacement ir5 = new Conversive.Verbot5.InputReplacement();
                    ir5.TextToFind = ir.TextToFind;
                    ir5.TextToInput = ir.TextToInput;
                    ckb5.InputReplacements.Add(ir5);
                }
            }

		    //private Dictionary<string, List<string>> recentOutputsByRule;
            if (this.CKB4.RecentOutputsByRule != null)
            {
                ckb5.RecentOutputsByRule = new Dictionary<string, List<string>>();
                foreach (string key in this.CKB4.RecentOutputsByRule.Keys)
                {
                    System.Collections.ArrayList al = (System.Collections.ArrayList)this.CKB4.RecentOutputsByRule[key];
                    List<string> recentIds = new List<string>(al.Count);
                    foreach(string stId in al)
                    {
                        recentIds.Add(stId);
                    }
                    ckb5.RecentOutputsByRule.Add(key, recentIds);
                }
            }

	    	//private int build;
            ckb5.Build = this.CKB4.Build;
		
		    //private KnowledgeBaseItem knowledgeBaseItem;
            if (this.CKB4.KnowledgeBaseItem != null)
            {
                ckb5.KnowledgeBaseItem = new Conversive.Verbot5.KnowledgeBaseItem();
                ckb5.KnowledgeBaseItem.Filename = this.CKB4.KnowledgeBaseItem.Filename;
                ckb5.KnowledgeBaseItem.Fullpath = this.CKB4.KnowledgeBaseItem.Fullpath;
                ckb5.KnowledgeBaseItem.Build = this.CKB4.KnowledgeBaseItem.Build;
                ckb5.KnowledgeBaseItem.Used = this.CKB4.KnowledgeBaseItem.Used;
                ckb5.KnowledgeBaseItem.Trusted = this.CKB4.KnowledgeBaseItem.Trusted;
                ckb5.KnowledgeBaseItem.Untrusted = this.CKB4.KnowledgeBaseItem.Untrusted;
            }

		    //private KnowledgeBaseInfo knowledgeBaseInfo;
            if (this.CKB4.KnowledgeBaseInfo != null)
            {
                ckb5.KnowledgeBaseInfo = new Conversive.Verbot5.KnowledgeBaseInfo();
                /*
                 public string Author;
		            public string Copyright;
		            public string License;
		            public string AuthorWebsite;
		            public System.DateTime CreationDate;
		            public System.DateTime LastUpdateDate;
		            public KnowledgeBaseRating Rating;
		            public CategoryType Category;
		            public LanguageType Language;
                 */
                ckb5.KnowledgeBaseInfo.Author = this.CKB4.KnowledgeBaseInfo.Author;
                ckb5.KnowledgeBaseInfo.AuthorWebsite = this.CKB4.KnowledgeBaseInfo.AuthorWebsite;
                ckb5.KnowledgeBaseInfo.Category = (Conversive.Verbot5.KnowledgeBaseInfo.CategoryType)Enum.ToObject(typeof(Conversive.Verbot5.KnowledgeBaseInfo.CategoryType), (int)this.CKB4.KnowledgeBaseInfo.Category);
                ckb5.KnowledgeBaseInfo.Language = (Conversive.Verbot5.KnowledgeBaseInfo.LanguageType)Enum.ToObject(typeof(Conversive.Verbot5.KnowledgeBaseInfo.LanguageType), (int)this.CKB4.KnowledgeBaseInfo.Language);
                ckb5.KnowledgeBaseInfo.Comment = this.CKB4.KnowledgeBaseInfo.Comment;
                ckb5.KnowledgeBaseInfo.Copyright = this.CKB4.KnowledgeBaseInfo.Copyright;
                ckb5.KnowledgeBaseInfo.CreationDate = this.CKB4.KnowledgeBaseInfo.CreationDate;
                ckb5.KnowledgeBaseInfo.LastUpdateDate = this.CKB4.KnowledgeBaseInfo.LastUpdateDate;
                ckb5.KnowledgeBaseInfo.License = this.CKB4.KnowledgeBaseInfo.License;
                if (this.CKB4.KnowledgeBaseInfo.Rating != null)
                {
                    ckb5.KnowledgeBaseInfo.Rating = new Conversive.Verbot5.KnowledgeBaseRating();
                    ckb5.KnowledgeBaseInfo.Rating.Description = this.CKB4.KnowledgeBaseInfo.Rating.Description;
                    ckb5.KnowledgeBaseInfo.Rating.Language = this.CKB4.KnowledgeBaseInfo.Rating.Language;
                    ckb5.KnowledgeBaseInfo.Rating.Other = this.CKB4.KnowledgeBaseInfo.Rating.Other;
                    ckb5.KnowledgeBaseInfo.Rating.Rating = (Conversive.Verbot5.KnowledgeBaseRating.RatingLevel)Enum.ToObject(typeof(Conversive.Verbot5.KnowledgeBaseRating.RatingLevel), (int)this.CKB4.KnowledgeBaseInfo.Rating.Rating);
                    ckb5.KnowledgeBaseInfo.Rating.Sexual = this.CKB4.KnowledgeBaseInfo.Rating.Sexual;
                    ckb5.KnowledgeBaseInfo.Rating.Violence = this.CKB4.KnowledgeBaseInfo.Rating.Violence;
                }
                
            }

            return ckb5;
        }
    }
}
