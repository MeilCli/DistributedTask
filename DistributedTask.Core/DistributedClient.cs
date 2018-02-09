using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace DistributedTask.Core
{
    public class DistributedClient
    {

        public void Start()
        {
            var channel = new IpcClientChannel();
            ChannelServices.RegisterChannel(channel, true);
        }
    }
}
