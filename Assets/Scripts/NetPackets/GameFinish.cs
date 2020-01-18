using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct GameFinish : INetPacket<GameFinish> {
    public PacketType type => PacketType.GameFinish;

    public int id;

    public GameFinish(int id) {
      this.id = id;
    }

    public GameFinish Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
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