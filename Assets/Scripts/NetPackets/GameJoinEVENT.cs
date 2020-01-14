using System.Text;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;

// int id, int playerCount, int acceptedPlayers
namespace NetPackets {
  public struct GameJoinEVENT : INetPacket<GameJoinEVENT> {
    public PacketType type => PacketType.GameJoinEVENT;

    public int id;
    public int playerCount;
    public int acceptedPlayers;

    public GameJoinEVENT(int id, int playerCount, int acceptedPlayers) {
      this.id = id;
      this.playerCount = playerCount;
      this.acceptedPlayers = acceptedPlayers;
    }

    public GameJoinEVENT(GameInfo game) {
      this.id = game.id;
      this.playerCount = game.playerCount;
      this.acceptedPlayers = game.acceptedPlayers;
    }

    public GameJoinEVENT Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      id = stream.ReadInt(ref context);
      playerCount = stream.ReadInt(ref context);
      acceptedPlayers = stream.ReadInt(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4*4, Allocator.Temp)) {
        writer.Write((int) this.type);

        writer.Write(id);
        writer.Write(playerCount);
        writer.Write(acceptedPlayers);

        connection.Send(driver, writer);
      }
    }
  }
}