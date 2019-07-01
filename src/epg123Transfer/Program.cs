using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using epg123;

namespace epg123Transfer
{
    static class Program
    {
        #region ========== Binding Redirects ==========
        static Program()
        {
            string[] assemblies = { "mcepg", "mcstore" };

            string version = FindDLLVersion(assemblies[0]);
            foreach (string assembly in assemblies)
            {
                try
                {
                    RedirectAssembly(assembly, version);
                }
                catch { }
            }
        }
        private static string FindDLLVersion(string shortName)
        {
            string[] targetVersions = { "6.1.0.0", "6.2.0.0", "6.3.0.0" };
            foreach (string targetVersion in targetVersions)
            {
                if (IsAssemblyInGAC(string.Format("{0}, Version={1}, Culture=neutral, PublicKeyToken=31bf3856ad364e35", shortName, targetVersion)))
                {
                    return targetVersion;
                }
            }
            return null;
        }
        public static bool IsAssemblyInGAC(string assemblyString)
        {
            bool result = false;
            try
            {
                result = Assembly.ReflectionOnlyLoad(assemblyString).GlobalAssemblyCache;
            }
            catch { }
            return result;
        }
        private static void RedirectAssembly(string shortName, string targetVersionStr)
        {
            ResolveEventHandler handler = null;
            handler = (sender, args) =>
            {
                var requestedAssembly = new AssemblyName(args.Name);
                if (requestedAssembly.Name != shortName) return null;

                requestedAssembly.Version = new Version(targetVersionStr);
                requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=31bf3856ad364e35").GetPublicKeyToken());
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve -= handler;
                return Assembly.Load(requestedAssembly);
            };
            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            EstablishFileFolderPaths();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string file = string.Empty;
            if ((args != null) && (args.Length > 0))
            {
                if (File.Exists(args[0])) file = args[0];
            }

            try
            {
                Application.Run(new frmTransfer(file));
            }
            catch { }
        }

        static void EstablishFileFolderPaths()
        {
            // set the base path and the working directory
            Helper.ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(Helper.ExecutablePath);
        }
    }
}
