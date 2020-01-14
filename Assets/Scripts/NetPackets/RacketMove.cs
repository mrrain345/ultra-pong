using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct RacketMove : INetPacket<RacketMove> {
    public PacketType type => PacketType.RacketMove;

    public float position;

    public RacketMove(float position) {
      this.position = position;
    }

    public RacketMove Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      this.position = stream.ReadFloat(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4*2, Allocator.Temp)) {
        writer.Write((int) this.type);
        
        writer.Write(position);
        
        connection.Send(driver, writer);
      }
    }
  }
}