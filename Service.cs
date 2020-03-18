using System;
using System.ServiceProcess;

namespace SecondaryServer
{
    class Service
    {
        private Log log = new Log();
        public bool IsServiceRunning(ServiceController service)
        {
            if (service.Status.Equals(ServiceControllerStatus.Running))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StopService(ServiceController service)
        {
            if (IsServiceRunning(service))
            {
                try
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                catch (Exception ex)
                {
                    log.WriteToFile(ex.Message);
                }
            }
            return;
        }

        public void StartService(ServiceController service)
        {
            if (!IsServiceRunning(service))
            {
                try
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
                catch (Exception ex)
                {
                    log.WriteToFile(ex.Message);
                    
                }
            }
            return;
        }
    }
}
