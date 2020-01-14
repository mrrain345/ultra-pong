using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct LobbyChangedEVENT : INetPacket<LobbyChangedEVENT> {
    public PacketType type => PacketType.LobbyChangedEVENT;

    public LobbyChangedEVENT Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
        writer.Write((int) this.type);

        connection.Send(driver, writer);
      }
    }
  }
}