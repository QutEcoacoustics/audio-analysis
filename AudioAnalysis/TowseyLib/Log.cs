// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Log.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Log type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TowseyLib
{
    using log4net;

    public static class Log
    {
        //private const string MesgFormat = "{0:yyyy-MM-dd HH:mm:ss.fff}  {1}";

        private static readonly ILog _logger = LogManager.GetLogger(typeof(Log));

        static Log()
        {
            Verbosity = 0;
        }

        public static ILog Logger
        {
            get
            {
                return _logger;
            }
        }

        public static int Verbosity { get; set; }


        public static void WriteLine(string format, params object[] args)
        {
            _logger.InfoFormat(format, args);

            //#if LOGTOCONSOLE
            //Console.WriteLine(MesgFormat, DateTime.Now, string.Format(format, args));
            //#endif
        }

        public static void WriteIfVerbose(string format, params object[] args)
        {
            //#if LOGTOCONSOLE
            if (Verbosity > 0)
            {
                _logger.InfoFormat(format, args);
                //Console.WriteLine(MesgFormat, DateTime.Now, string.Format(format, args));
            }
            //#endif
        }
    }
}