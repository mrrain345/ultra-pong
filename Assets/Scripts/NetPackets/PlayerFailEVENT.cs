using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct PlayerFailEVENT : INetPacket<PlayerFailEVENT> {
    public PacketType type => PacketType.PlayerFailEVENT;

    public int failed;
    public int lastTouched;

    public PlayerFailEVENT(int failed, int lastTouched) {
      this.failed = failed;
      this.lastTouched = lastTouched;
    }

    public PlayerFailEVENT Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      this.failed = stream.ReadInt(ref context);
      this.lastTouched = stream.ReadInt(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4*3, Allocator.Temp)) {
        writer.Write((int) this.type);
        
        writer.Write(failed);
        writer.Write(lastTouched);
        
        connection.Send(driver, writer);
      }
    }
  }
}