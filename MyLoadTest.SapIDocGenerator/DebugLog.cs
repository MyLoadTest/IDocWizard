using System;
using System.Globalization;
using System.Linq;
using log4net;
using log4net.Config;

namespace MyLoadTest.SapIDocGenerator
{
    internal static class DebugLog
    {
        #region Constants and Fields

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

            XmlConfigurator.Configure();
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