using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct RacketMove : INetPacket<RacketMove> {
    public PacketType type => PacketType.RacketMove;

    public float position;
    public float velocity;

    public RacketMove(float position, float velocity) {
      this.position = position;
      this.velocity = velocity;
    }

    public RacketMove Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      this.position = stream.ReadFloat(ref context);
      this.velocity = stream.ReadFloat(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4*3, Allocator.Temp)) {
        writer.Write((int) this.type);
        
        writer.Write(position);
        writer.Write(velocity);
        
        connection.Send(driver, writer);
      }
    }
  }
}