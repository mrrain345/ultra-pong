using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct GameList : INetPacket<GameList> {
    public PacketType type => PacketType.GameList;

    public GameList Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
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