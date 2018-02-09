using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace DistributedTask.Core
{
    public class DistributedServer
    {

        public void Start()
        {
            var distributedObject = new DistributedObject();

            var channel = new IpcServerChannel("DistributedChannel");
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(distributedObject, "DistributedObject");
        }
    }
}
