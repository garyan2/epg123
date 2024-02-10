using GaRyan2.Utilities;
using System.ServiceProcess;

namespace epg123Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Logger.Initialize(Helper.ServerLogPath, "Beginning EPG123 token/proxy/cache service", false);

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Server()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
