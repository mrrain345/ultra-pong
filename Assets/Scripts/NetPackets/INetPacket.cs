using Unity.Networking.Transport;

namespace NetPackets {
  public interface INetPacket<T> {
    PacketType type { get; }

    void Send(UdpNetworkDriver driver, NetworkConnection connection);
    T Receive(ref DataStreamReader stream, ref DataStreamReader.Context context);
  }
}