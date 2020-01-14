using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct RacketMoveEVENT : INetPacket<RacketMoveEVENT> {
    public PacketType type => PacketType.RacketMoveEVENT;

    public int playerID;
    public float position;

    public RacketMoveEVENT(int playerID, float position) {
      this.playerID = playerID;
      this.position = position;
    }

    public RacketMoveEVENT Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      this.playerID = stream.ReadInt(ref context);
      this.position = stream.ReadFloat(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4*3, Allocator.Temp)) {
        writer.Write((int) this.type);
        
        writer.Write(playerID);
        writer.Write(position);
        
        connection.Send(driver, writer);
      }
    }
  }
}