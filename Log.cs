using System;
using System.Globalization;
using System.IO;

namespace SecondaryServer
{
    class Log : IDisposable
    {
        private bool disposed;
        public void WriteToFile(string Message, string type = "info")
        {
            string datetostring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CreateSpecificCulture("pl-PL"));
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            string pre = "[INFO] ";
            if (type == "error")
            {
                pre = "[" + type.ToUpper() + "] ";
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\SecondaryServer.log";
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine("[" + datetostring + "]" + pre + Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("[" + datetostring + "]" + pre + Message);
                }
            }
        }

        ~Log()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                }
            }
            disposed = true;
        }
    }
}
