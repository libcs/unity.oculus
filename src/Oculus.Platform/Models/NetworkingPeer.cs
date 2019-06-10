namespace Oculus.Platform.Models
{
    public class NetworkingPeer
    {
        public NetworkingPeer(ulong id, PeerConnectionState state)
        {
            ID = id;
            State = state;
        }

        public ulong ID { get; private set; }
        public PeerConnectionState State { get; private set; }
    }
}
