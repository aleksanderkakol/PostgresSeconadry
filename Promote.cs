using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace SecondaryServer
{
    class Promote
    {
        Log log = new Log();
        Server server = new Server();
        string php_config = @"C:\Server\data\htdocs\config.php";
        public void Secondary()
        {
            string localPostgresPath = ConfigurationManager.AppSettings.Get("localPostgresPath");
            string sqlExpressPort = ConfigurationManager.AppSettings.Get("sqlExpressPort");
            string sqlExpressDatabase = ConfigurationManager.AppSettings.Get("sqlExpressDatabase");
            string sqlExpressUsername = ConfigurationManager.AppSettings.Get("sqlExpressUsername");
            string sqlExpressPassword = ConfigurationManager.AppSettings.Get("sqlExpressPassword");

            string saltoProcessV321 = "SaltoProcessV3.2.1";
            ServiceController saltoProcessV = new ServiceController(saltoProcessV321);

            string saltoServiceName = "ProAccessSpaceService";
            ServiceController saltoService = new ServiceController(saltoServiceName);

            
            SQLExpress sql = new SQLExpress();
            Service service = new Service();
            IsPrimaryServerPromoted primary = new IsPrimaryServerPromoted();

            try
            {
                log.WriteToFile("Cannot Connect to Primary Server", "error");
                log.WriteToFile("Starting Promote Secondary Server...");
                sql.RestoreDataBase("127.0.0.1", sqlExpressPort, sqlExpressDatabase, sqlExpressUsername, sqlExpressPassword);
                service.StartService(saltoService);
                service.StartService(saltoProcessV);
                File.Create($@"{localPostgresPath}\data\failover\failover.trigger").Dispose();
                if(primary.TriggerExist())
                {
                    File.Delete($@"{localPostgresPath}\data\PrimaryPromoted.trigger");
                }
                log.WriteToFile("Secondary Server Promoted Succeed");
                log.Dispose();
            } catch (Exception ex)
            {
                log.WriteToFile(ex.Message);
                log.Dispose();
            }
        }

        public void PHPConfig(string contain_IP, string replace_IP)
        {
            string content = string.Empty;
            using (StreamReader sr = new StreamReader(php_config))
            {
                content = sr.ReadToEnd();
                sr.Close();
            }
            content = Regex.Replace(content, contain_IP, replace_IP);

            using (StreamWriter sw = new StreamWriter(php_config))
            {
                sw.Write(content);
                sw.Close();
            }
        }
    }
}