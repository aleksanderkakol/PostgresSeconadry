using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace SecondaryServer
{
    class Postgres
    {
        private Log log = new Log();
        public string localPostgresPath = ConfigurationManager.AppSettings.Get("localPostgresPath");
        private void PGbasebackup(string ip, string port, string usr, string pwd, string path)
        {
            //string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            ProcessStartInfo startinfo = new ProcessStartInfo
            {
                FileName = localPostgresPath + @"\bin\pg_basebackup.exe",
                Arguments = $@" --dbname=postgresql://{usr}:{pwd}@{ip}:{port}/postgres -X stream -R -P -D ""{path}\data""",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startinfo))
            {
                using (StreamReader reader = process.StandardError)
                {
                    log.WriteToFile(reader.ReadToEnd());
                }
                process.WaitForExit();
                process.Close();
            }
            return;
        }

        public bool PostgresReplica(string ip, string port, string user, string password)
        {
            string datetostring = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("pl-PL"));
            try
            {
                if (Directory.Exists($@"{localPostgresPath}\data"))
                {
                    Directory.Move($@"{localPostgresPath}\data", $@"{localPostgresPath}\data_{datetostring}");
                    PGbasebackup(ip, port, user, password, localPostgresPath);

                    if (File.Exists($@"{localPostgresPath}\data\recovery.done"))
                    {
                        File.Delete($@"{localPostgresPath}\data\recovery.done");
                    }

                    if (File.Exists($@"{localPostgresPath}\data\recovery.conf"))
                    {
                        File.Delete($@"{localPostgresPath}\data\recovery.conf");
                        File.Copy($@"{localPostgresPath}\data\recovery_file\recovery.conf", $@"{localPostgresPath}\data\recovery.conf");
                    }

                    if (File.Exists($@"{localPostgresPath}\data\failover\failover.trigger"))
                    {
                        File.Delete($@"{localPostgresPath}\data\failover\failover.trigger");
                    }
                }
                log.WriteToFile("Secondary Server Set As Replica");
                return true;
            }
            catch (Exception ex)
            {
                log.WriteToFile(ex.Message);
                log.WriteToFile(ex.StackTrace);
                return false;
            }
        }
    }
}
