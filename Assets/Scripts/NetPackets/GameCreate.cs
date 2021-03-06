using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace NetPackets {
  public struct GameCreate : INetPacket<GameCreate> {
    public PacketType type => PacketType.GameCreate;

    public GameInfo.Mode mode;
    public int players;
    public string name;

    public GameCreate(GameInfo.Mode mode, int players, string name) {
      this.mode = mode;
      this.players = players;
      this.name = name;
    }

    public GameCreate Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      this.mode = (GameInfo.Mode) stream.ReadInt(ref context);
      this.players = stream.ReadInt(ref context);
      int nameLength = stream.ReadInt(ref context);
      byte[] name = stream.ReadBytesAsArray(ref context, nameLength);
      this.name = Encoding.UTF8.GetString(name);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      byte[] name = Encoding.UTF8.GetBytes(this.name);

      using (var writer = new DataStreamWriter(4*4 + name.Length, Allocator.Temp)) {
        writer.Write((int) this.type);

        writer.Write((int) mode);
        writer.Write(players);
        writer.Write(name.Length);
        writer.Write(name, name.Length);
        
        connection.Send(driver, writer);
      }
    }
  }
}