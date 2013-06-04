using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using log4net;
using log4net.Config;

namespace MyLoadTest
{
    public static class Logger
    {
        #region Constants and Fields

        private const string Log4NetConfigurationFileName = "log4net.config";
        private const string LogDirPropertyName = "LogDir";

        private static readonly Dictionary<LogLevel, Action<string>> WriteMap =
            new Dictionary<LogLevel, Action<string>>
            {
                { LogLevel.Debug, Debug },
                { LogLevel.Info, Info },
                { LogLevel.Warning, Warning },
                { LogLevel.Error, Error }
            };

        private static readonly Dictionary<LogLevel, Action<string, object[]>> WriteFormatMap =
            new Dictionary<LogLevel, Action<string, object[]>>
            {
                { LogLevel.Debug, DebugFormat },
                { LogLevel.Info, InfoFormat },
                { LogLevel.Warning, WarningFormat },
                { LogLevel.Error, ErrorFormat }
            };

        private static readonly ILog Log;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes the <see cref="Logger"/> class.
        /// </summary>
        static Logger()
        {
            var assemblyDirectory = typeof(Logger).Assembly.GetDirectory();
            GlobalContext.Properties[LogDirPropertyName] = assemblyDirectory;

            var configFilePath = new Uri(Path.Combine(assemblyDirectory, Log4NetConfigurationFileName));
            XmlConfigurator.Configure(configFilePath);

            Log = LogManager.GetLogger(typeof(Logger));
        }

        #endregion

        #region Public Methods

        public static void Debug(string text)
        {
            Log.Debug(text);
        }

        public static void DebugFormat(string format, params object[] args)
        {
            Log.DebugFormat(CultureInfo.InvariantCulture, format, args);
        }

        public static void Info(string text)
        {
            Log.Info(text);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            Log.InfoFormat(CultureInfo.InvariantCulture, format, args);
        }

        public static void Warning(string text)
        {
            Log.Warn(text);
        }

        public static void WarningFormat(string format, params object[] args)
        {
            Log.WarnFormat(CultureInfo.InvariantCulture, format, args);
        }

        public static void Error(string text)
        {
            Log.Error(text);
        }

        public static void Error(string text, Exception exception)
        {
            Log.Error(text, exception);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            Log.ErrorFormat(CultureInfo.InvariantCulture, format, args);
        }

        public static void Write(LogLevel logLevel, string text)
        {
            WriteMap[logLevel](text);
        }

        public static void WriteFormat(LogLevel logLevel, string format, params object[] args)
        {
            WriteFormatMap[logLevel](format, args);
        }

        #endregion
    }
}