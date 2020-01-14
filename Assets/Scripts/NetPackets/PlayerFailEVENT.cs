using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct PlayerFailEVENT : INetPacket<PlayerFailEVENT> {
    public PacketType type => PacketType.PlayerFailEVENT;

    public int id;

    public PlayerFailEVENT(int id) {
      this.id = id;
    }

    public PlayerFailEVENT Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      this.id = stream.ReadInt(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4*2, Allocator.Temp)) {
        writer.Write((int) this.type);
        
        writer.Write(id);
        
        connection.Send(driver, writer);
      }
    }
  }
}