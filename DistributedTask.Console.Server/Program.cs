using DistributedTask.Core;
using static DistributedTask.Core.Logger;

namespace DistributedTask.Console.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Log("Server Start");
            new DistributedServer().Start();
            System.Console.ReadLine();
        }
    }
}
