using GaRyan2.Utilities;
using System.ServiceProcess;

namespace tokenServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Logger.Initialize(Helper.ServerLogPath);

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Server()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
