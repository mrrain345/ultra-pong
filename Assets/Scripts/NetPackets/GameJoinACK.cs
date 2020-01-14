using System.Text;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;

// int id, int playerCount, int acceptedPlayers
namespace NetPackets {
  public struct GameJoinACK : INetPacket<GameJoinACK> {
    public PacketType type => PacketType.GameJoinACK;

    public bool success;
    public int id;
    public int playerCount;
    public int acceptedPlayers;

    public GameJoinACK(int id, int playerCount, int acceptedPlayers) {
      this.success = true;
      this.id = id;
      this.playerCount = playerCount;
      this.acceptedPlayers = acceptedPlayers;
    }

    public GameJoinACK(GameLobby game) {
      this.success = true;
      this.id = game.id;
      this.playerCount = game.playerCount;
      this.acceptedPlayers = game.acceptedPlayers;
    }

    public GameJoinACK(bool success) {
      if (success) throw new System.ArgumentException("Argument must be false", "success");
      this.success = false;
      this.id = 0;
      this.playerCount = 0;
      this.acceptedPlayers = 0;
    }

    public GameJoinACK Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      success = stream.ReadByte(ref context) != 0;
      id = stream.ReadInt(ref context);
      playerCount = stream.ReadInt(ref context);
      acceptedPlayers = stream.ReadInt(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      using (var writer = new DataStreamWriter(4*4 + 1, Allocator.Temp)) {
        writer.Write((int) this.type);

        writer.Write((byte) (success ? 0xFF : 0x00));
        writer.Write(id);
        writer.Write(playerCount);
        writer.Write(acceptedPlayers);

        connection.Send(driver, writer);
      }
    }
  }
}