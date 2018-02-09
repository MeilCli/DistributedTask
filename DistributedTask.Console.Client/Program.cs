using DistributedTask.Core;
using System.Threading.Tasks;
using static DistributedTask.Core.Logger;

namespace DistributedTask.Console.Test
{

    public class Program
    {

        public static async Task Main(string[] args)
        {
            new DistributedClient().Start();
            Log("SimplePre");
            var i1 = await Simple();
            Log($"Simple1Post Result:{i1}");

            Log("DistributedPre");
            var i2 = await Distributed();
            Log($"DistributedPost Result:{i2}");
        }

        static async AsyncTask<int> Simple()
        {
            Log("Pre");
            int i = await Task.Run(() =>
            {
                Log("Run");
                return -1;
            });
            Log($"Post Check:-1={i}");
            return 1;
        }

        static async DistributedAsyncTask<int> Distributed()
        {
            Log("Pre");
            int i = await Task.Run(() =>
            {
                Log("Run");
                return -1;
            });
            Log($"Post Check:-1={i}");
            return 2;
        }
    }

#pragma warning disable 1998
    public class SampleClass
    {

        public static async Task Sample()
        {
            int simpleResult = await Simple();
            int distributedResult = await Distributed();
        }

        // 自律的なコンピューターの場合
        public static async
            DistributedAsyncTask<int> Distributed()
        {
            return 1;
        }

        // 非自律的なコンピューターの場合
        public static async AsyncTask<int> Simple()
        {
            return 2;
        }
    }
}
