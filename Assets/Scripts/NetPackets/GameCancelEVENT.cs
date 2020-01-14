using System.Text;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;

// int id, int playerCount, int acceptedPlayers
namespace NetPackets {
  public struct GameCancelEVENT : INetPacket<GameCancelEVENT> {
    public PacketType type => PacketType.GameCancelEVENT;

    public int id;
    public int playerCount;
    public int acceptedPlayers;

    public GameCancelEVENT(int id, int playerCount, int acceptedPlayers) {
      this.id = id;
      this.playerCount = playerCount;
      this.acceptedPlayers = acceptedPlayers;
    }

    public GameCancelEVENT(GameInfo game) {
      this.id = game.id;
      this.playerCount = game.playerCount;
      this.acceptedPlayers = game.acceptedPlayers;
    }

    public GameCancelEVENT Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
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