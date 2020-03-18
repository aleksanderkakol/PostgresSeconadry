using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace SecondaryServer
{
    class Program
    {
        public const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        static void Main(string[] args)
        {
            string primaryIP = ConfigurationManager.AppSettings.Get("primaryServerIP");
            string secondaryIP = ConfigurationManager.AppSettings.Get("secondaryServerIP");
            string postgresPort = ConfigurationManager.AppSettings.Get("postgresServerPort");
            string postgresUser = ConfigurationManager.AppSettings.Get("postgresUsername");
            string postgresPassword = ConfigurationManager.AppSettings.Get("postgresPassword");
            string user = ConfigurationManager.AppSettings.Get("remoteServerUsername");
            string pwd = ConfigurationManager.AppSettings.Get("remoteServerPassword");

            string saltoProcessV321 = "SaltoProcessV3.2.1";
            ServiceController saltoProcessService = new ServiceController(saltoProcessV321);
            string postgresServiceName = "postgresql-x64-10";
            ServiceController postgresService = new ServiceController(postgresServiceName);
            string saltoServiceName = "ProAccessSpaceService";
            ServiceController saltoService = new ServiceController(saltoServiceName);

            Log log = new Log();
            Server server = new Server();
            Service service = new Service();
            Postgres postgres = new Postgres();
            SQLExpress sql = new SQLExpress();
            IsPrimaryServerPromoted primary = new IsPrimaryServerPromoted();
            Promote promote = new Promote();




            if (server.CheckServerConnection(primaryIP))
            {
                promote.PHPConfig(secondaryIP, primaryIP);
                if (GetUpTime() < TimeSpan.FromMinutes(8))
                {
                    try
                    {
                        service.StopService(saltoProcessService);
                        service.StopService(saltoService);
                        service.StopService(postgresService);
                        postgres.PostgresReplica(primaryIP, postgresPort, postgresUser, postgresPassword);
                        service.StartService(postgresService);
                    } catch (Exception ex)
                    {
                        log.WriteToFile(ex.Message);
                    }
                    return;
                }

                if (primary.TriggerExist())
                {
                    service.StopService(postgresService);
                    postgres.PostgresReplica(primaryIP, postgresPort, postgresUser, postgresPassword);
                    service.StartService(postgresService);
                    return;
                    
                } else if(primary.RecoveryConfExists())
                {
                    using (var impersonation = new ImpersonateUser(user, primaryIP, pwd, LOGON32_LOGON_NEW_CREDENTIALS))
                    {
                        sql.Backup();
                        Thread.Sleep(5000);
                        return;
                    }
                } else
                {
                    log.WriteToFile("Secondary Server is already Promoted");
                    
                }
                return;
            } else
            {
                promote.Secondary();
                promote.PHPConfig(primaryIP, secondaryIP);
                return;
            }
        }

        public static TimeSpan GetUpTime()
        {
            return TimeSpan.FromMilliseconds(GetTickCount64());
        }

        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
    }
}
