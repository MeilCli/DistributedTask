using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DistributedTask.Core
{
    public static class Logger
    {

        public static bool IsShow { get; set; } = true;

        public static void Log(string message = "",[CallerMemberName] string callerMemberName = null)
        {
            if (IsShow == false)
            {
                return;
            }
            string head = callerMemberName == null ? string.Empty : $"[{callerMemberName}] ";
            Console.WriteLine($"{head}{message} in Thread{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
