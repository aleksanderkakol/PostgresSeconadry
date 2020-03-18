using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace SecondaryServer
{
    class SQLExpress
    {
        Log log = new Log();


        public bool IsFileOlder(string directory, TimeSpan thresholdAge)
        {
            var dir = new DirectoryInfo(directory);
            var file = dir.GetFiles().Where(fn => fn.Extension == ".bak").OrderByDescending(f => f.LastWriteTime).First();
            string fileName = file.FullName.ToLower();
            return (DateTime.Now - File.GetCreationTime(fileName)) > thresholdAge;
        }


        public bool RestoreDataBase(string IP, string PORT, string DB, string USER, string PASSWORD)
        {
            string localSaltoBackupDir = ConfigurationManager.AppSettings.Get("localSQLExpressBackupsDirectory");
            var directory = new DirectoryInfo(localSaltoBackupDir);
            string connectSQL = $"Data Source={IP},{PORT};Initial Catalog=master;User ID={USER};Password={PASSWORD}";
            //var file = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            var file = directory.GetFiles().Where(fn => fn.Extension == ".bak").OrderByDescending(f => f.LastWriteTime).First();
            using (SqlConnection connection = new SqlConnection(connectSQL))
            {
                try
                {
                    connection.Open();
                    //restore SQL
                    //disconnect users
                    string sqlStmt1 = string.Format($"ALTER DATABASE [{DB}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
                    SqlCommand sqlRes1 = new SqlCommand(sqlStmt1, connection);
                    sqlRes1.ExecuteNonQuery();
                    //rename DB
                    //string sqlStmt2 = string.Format($"ALTER DATABASE [{DB}] MODIFY NAME = [{DB}_{datetostring}]");
                    //SqlCommand sqlRes2 = new SqlCommand(sqlStmt2, connection);
                    //sqlRes2.ExecuteNonQuery();
                    //restore from file
                    string sqlStmt3 = $@"USE MASTER RESTORE DATABASE [{DB}] FROM DISK = '{localSaltoBackupDir}{file}' WITH REPLACE;";
                    SqlCommand sqlRes3 = new SqlCommand(sqlStmt3, connection);
                    sqlRes3.ExecuteNonQuery();

                    string sqlStmt4 = string.Format($"ALTER DATABASE [{DB}] SET MULTI_USER");
                    SqlCommand sqlRes4 = new SqlCommand(sqlStmt4, connection);
                    sqlRes4.ExecuteNonQuery();
                    connection.Close();
                    //server.WriteToFile(backupFileName);
                    return true;
                }
                catch (Exception ex)
                {
                    log.WriteToFile(ex.Message);
                    return false;
                }
            }

        }

        private bool BackupDataBase(string IP, string PORT, string DB, string USER, string PASSWORD)
        {
            string BACKUP_PATH = ConfigurationManager.AppSettings.Get("remoteSQLExpressBackupsDirectory");
            string datetostring = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("pl-PL"));
            string connectSQL = $"Data Source={IP},{PORT};Initial Catalog=master;User ID={USER};Password={PASSWORD}";
            string backupFileName = $"{BACKUP_PATH}{DB}_{datetostring}.bak";
            using (SqlConnection connection = new SqlConnection(connectSQL))
            {
                try
                {
                    connection.Open();
                    SqlDataReader reader;
                    //backup SQL
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = $@"BACKUP DATABASE [{DB}] TO DISK = '{backupFileName}' WITH COPY_ONLY, INIT";
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    reader = cmd.ExecuteReader();
                    connection.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    log.WriteToFile(ex.Message);
                    return false;
                }
            }
        }

        public bool Backup()
        {
            string sqlExpressPort = ConfigurationManager.AppSettings.Get("sqlExpressPort");
            string sqlExpressDatabase = ConfigurationManager.AppSettings.Get("sqlExpressDatabase");
            string sqlExpressUsername = ConfigurationManager.AppSettings.Get("sqlExpressUsername");
            string sqlExpressPassword = ConfigurationManager.AppSettings.Get("sqlExpressPassword");
            string localsqlExpressDir = ConfigurationManager.AppSettings.Get("localSQLExpressBackupsDirectory");
            string primaryIP = ConfigurationManager.AppSettings.Get("primaryServerIP");
            string saltoServiceName = "ProAccessSpaceService";
            ServiceController saltoRemoteService = new ServiceController(saltoServiceName, primaryIP);

            Service service = new Service();

            if (service.IsServiceRunning(saltoRemoteService))
            {
                bool oldEnough = IsFileOlder($@"{localsqlExpressDir}", new TimeSpan(0, 23, 0, 0));
                if (oldEnough)
                {
                    log.WriteToFile("MSQL Backup Succeed");
                    return BackupDataBase(primaryIP, sqlExpressPort, sqlExpressDatabase, sqlExpressUsername, sqlExpressPassword);
                }
                log.WriteToFile("MSQL Backup Not Created");
                return false;
            }
            else
            {
                log.WriteToFile("Cannot Create Backup, because Salto Service is Not Running", "error");
                return false;
            }
        }
    }
}
