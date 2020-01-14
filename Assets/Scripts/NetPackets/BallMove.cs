using System.Text;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct BallMove : INetPacket<BallMove> {
    public PacketType type => PacketType.BallMove;

    public Vector2 position;
    public Vector2 velocity;

    public BallMove(Vector2 position, Vector2 velocity) {
      this.position = position;
      this.velocity = velocity;
    }

    public BallMove Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      position.x = stream.ReadFloat(ref context);
      position.y = stream.ReadFloat(ref context);
      velocity.x = stream.ReadFloat(ref context);
      velocity.y = stream.ReadFloat(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4*5, Allocator.Temp)) {
        writer.Write((int) this.type);
        
        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(velocity.x);
        writer.Write(velocity.y);
        
        connection.Send(driver, writer);
      }
    }
  }
}