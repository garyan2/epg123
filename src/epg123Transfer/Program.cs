using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace epg123Transfer
{
    static class Program
    {
        #region ========== Binding Redirects ==========
        static Program()
        {
            string[] assemblies = { "mcepg", "mcstore" };

            var version = FindDllVersion(assemblies[0]);
            foreach (var assembly in assemblies)
            {
                try
                {
                    RedirectAssembly(assembly, version);
                }
                catch
                {
                    // ignored
                }
            }
        }
        private static string FindDllVersion(string shortName)
        {
            string[] targetVersions = { "6.1.0.0", "6.2.0.0", "6.3.0.0" };
            return targetVersions.FirstOrDefault(targetVersion => IsAssemblyInGac($"{shortName}, Version={targetVersion}, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
        }
        public static bool IsAssemblyInGac(string assemblyString)
        {
            var result = false;
            try
            {
                result = Assembly.ReflectionOnlyLoad(assemblyString).GlobalAssemblyCache;
            }
            catch
            {
                // ignored
            }

            return result;
        }
        private static void RedirectAssembly(string shortName, string targetVersionStr)
        {
            Assembly Handler(object sender, ResolveEventArgs args)
            {
                var requestedAssembly = new AssemblyName(args.Name);
                if (requestedAssembly.Name != shortName) return null;

                requestedAssembly.Version = new Version(targetVersionStr);
                requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=31bf3856ad364e35").GetPublicKeyToken());
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve -= Handler;
                return Assembly.Load(requestedAssembly);
            }

            AppDomain.CurrentDomain.AssemblyResolve += Handler;
        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var file = string.Empty;
            if (args != null && args.Length > 0)
            {
                if (File.Exists(args[0])) file = args[0];
            }

            try
            {
                Application.Run(new frmTransfer(file));
            }
            catch
            {
                // ignored
            }
        }
    }
}
