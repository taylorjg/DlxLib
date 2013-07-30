using System;

namespace DlxLibDemo3
{
    internal static class Logger
    {
        public static void Log(string format, params object[] args)
        {
            var mtid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            var prefix = string.Format("[{0}] ", mtid);
            Console.WriteLine(prefix + format, args);
        }
    }
}
