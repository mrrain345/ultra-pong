using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct RacketMoveEVENT : INetPacket<RacketMoveEVENT> {
    public PacketType type => PacketType.RacketMoveEVENT;

    public int playerID;
    public float position;
    public float velocity;

    public RacketMoveEVENT(int playerID, float position, float velocity) {
      this.playerID = playerID;
      this.position = position;
      this.velocity = velocity;
    }

    public RacketMoveEVENT Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      this.playerID = stream.ReadInt(ref context);
      this.position = stream.ReadFloat(ref context);
      this.velocity = stream.ReadFloat(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4*4, Allocator.Temp)) {
        writer.Write((int) this.type);
        
        writer.Write(playerID);
        writer.Write(position);
        writer.Write(velocity);
        
        connection.Send(driver, writer);
      }
    }
  }
}