using System;
using System.IO;
using System.Text;

/*
 Class compliments of Jhollman 
 https://stackoverflow.com/questions/4470700/how-to-save-console-writeline-output-to-text-file/4470751#:~:text=in%20the%20answer-,by%20WhoIsNinja%3A,-This%20code%20will
 */
namespace LogFileCleaner
{
    public static class logging
    {
        public static StringBuilder LogString = new StringBuilder();

        public static void WriteLine(string str)
        {
            Console.WriteLine(str);
            LogString.Append(str).Append(Environment.NewLine);

        }

        public static void Write(string str)
        {
            Console.Write(str);
            LogString.Append(str);

        }

        public static void SaveLog(bool Append = false, string Path = "./Log.txt")
        {
            if (LogString != null && LogString.Length > 0)
            {
                if (Append)
                {
                    using (StreamWriter file = File.AppendText(Path))
                    {
                        file.Write(LogString.ToString());
                        file.Close();
                        file.Dispose();
                    }
                }
                else
                {
                    using (StreamWriter file = new StreamWriter(Path))
                    {
                        file.Write(LogString.ToString());
                        file.Close();
                        file.Dispose();
                    }
                }
            }
        }
    }
}
