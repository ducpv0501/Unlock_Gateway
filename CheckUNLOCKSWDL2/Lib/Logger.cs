using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckUNLOCKSWDL2.Lib
{
    public  class Logger
    {
      
        public  void WriteLog(string logFile, string message)
        {
            try
            {
               
                File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error writing log: {ex.Message}");
            }
        }
    }
}
