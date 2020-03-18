using System;
using System.Net;
using System.Net.NetworkInformation;

namespace SecondaryServer
{
    public partial class Server
    {
        private Log log = new Log();
        public bool CheckServerConnection(string ip)
        {
            Ping pingRequest = new Ping();
            PingReply requestReply = pingRequest.Send(IPAddress.Parse(ip));
            try
            {
                if (requestReply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        requestReply = pingRequest.Send(IPAddress.Parse(ip));
                        if (requestReply.Status == IPStatus.Success)
                        {
                            return true;
                        }
                        else
                        {
                            log.WriteToFile(requestReply.Status.ToString());
                        }
                        System.Threading.Thread.Sleep(2000);
                    }
                    return false;
                }

            }
            catch (Exception ex)
            {
                log.WriteToFile(ex.Message);
                return false;
            }
        }
    }
}
