using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using log4net.Core;
using log4net.Layout.Pattern;

namespace MyLoadTest.Log4NetConverters
{
    public sealed class HexThreadConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            long id;
            if (long.TryParse(loggingEvent.ThreadName, out id))
            {
                writer.Write("0x{0:X8}", id);
            }
            else
            {
                writer.Write(loggingEvent.ThreadName);
            }
        }
    }
}