using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Conversive.Verbot5
{

    public class Logger
    {
        string strErrorFilePath;
        ArrayList Stack = new ArrayList();
        public Logger(string Name, string message)
        {
            //Call Log Error
            strErrorFilePath = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\Silver Monkey\\Log\\" + Name + System.DateTime.Now.ToString("MM_dd_yyyy_H-mm-ss") + ".txt";
            System.IO.Directory.CreateDirectory(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\Silver Monkey\\Log\\");
            LogMessage(message);
        }
        public bool IsFileInUse(string filePath)
        {
            try
            {
                string[] contents = System.IO.File.ReadAllLines(filePath);
            }
            catch (System.IO.IOException ex)
            {
                return (ex.Message.StartsWith("The process cannot access the file") && ex.Message.EndsWith("because it is being used by another process."));
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }


        public void LogMessage(string Message)
        {
            System.IO.StreamWriter ioFile = null;

            try
            {
                ioFile = new System.IO.StreamWriter(strErrorFilePath, true);
                foreach (string line in Stack.ToArray())
                {
                    ioFile.WriteLine(line);
                }
                Stack.Clear();
                ioFile.WriteLine(Message);

                ioFile.Close();
            }
            catch (System.IO.IOException ex)
            {
                if ((ex.Message.StartsWith("The process cannot access the file") && ex.Message.EndsWith("because it is being used by another process.")))
                {
                    Stack.Add(Message);
                }

            }
            catch (Exception exLog)
            {
                if ((ioFile != null))
                {
                    ioFile.Close();
                }
            }
        }




    }
}
