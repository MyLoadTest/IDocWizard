using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Config;

namespace MyLoadTest.SapIDocGenerator
{
    internal static class DebugLog
    {
        #region Constants and Fields

        private const string Log4NetConfiguarationFileName = "log4net.config";
        private const string LogDirPropertyName = "LogDir";

        private static readonly ILog Log = LogManager.GetLogger(typeof(DebugLog));

        #endregion

        #region Constructors

        #region Public Methods

        /// <summary>
        ///     Initializes the <see cref="DebugLog"/> class.
        /// </summary>
        static DebugLog()
        {
            //// TODO [VM] Probably one should use manual (hard-coded) configuration as VuGen also uses log4net

            var assemblyDirectory = typeof(DebugLog).Assembly.GetDirectory();
            GlobalContext.Properties[LogDirPropertyName] = assemblyDirectory;

            var configFilePath = new Uri(Path.Combine(assemblyDirectory, Log4NetConfiguarationFileName));
            XmlConfigurator.Configure(configFilePath);
        }

        #endregion

        public static void Write(string text)
        {
            Log.Debug(text);
        }

        public static void Write(string format, params object[] args)
        {
            Write(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        #endregion
    }
}