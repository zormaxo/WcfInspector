using NLog;
using System;

namespace WcfUtility
{
    public static class WcfLogger
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void LogInfo(string message)
        {
            logger.Info(message);
        }

        public static void LogException(Exception ex, string message)
        {
            logger.Error(ex);
            logger.Error(message);
        }
    }
}