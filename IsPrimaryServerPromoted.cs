using System.Configuration;
using System.IO;

namespace SecondaryServer
{
    class IsPrimaryServerPromoted
    {
        readonly string localPostgresPath = ConfigurationManager.AppSettings.Get("localPostgresPath");

        public bool TriggerExist()
        {
            return File.Exists($@"{localPostgresPath}\data\PrimaryPromoted.trigger");
        }
        public bool RecoveryConfExists()
        {
            return File.Exists($@"{localPostgresPath}\data\recovery.conf");
        }
    }
}
