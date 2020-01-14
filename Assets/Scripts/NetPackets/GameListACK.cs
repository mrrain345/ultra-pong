using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

//int length, [int id, int owner, int mode, int playerCount, int acceptedPlayers, int nameLength, byte[] name]
namespace NetPackets {
  public struct GameListACK : INetPacket<GameListACK> {
    public PacketType type => PacketType.GameListACK;

    public List<GameLobby> gameLobby;

    public GameListACK(List<GameLobby> gameLobby) {
      this.gameLobby = gameLobby;
    }

    public GameListACK Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      int length = stream.ReadInt(ref context);
      gameLobby = new List<GameLobby>(length);
      Debug.LogFormat("[CLIENT] GameListACK games: {0}", length);

      for (int i = 0; i < length; i++) {
        int id = stream.ReadInt(ref context);
        int owner = stream.ReadInt(ref context);
        int mode = stream.ReadInt(ref context);
        int playerCount = stream.ReadInt(ref context);
        int acceptedPlayers = stream.ReadInt(ref context);
        int nameLength = stream.ReadInt(ref context);
        byte[] name = stream.ReadBytesAsArray(ref context, nameLength);

        gameLobby.Insert(i, new GameLobby(id, owner, Encoding.UTF8.GetString(name), (GameLobby.Mode)mode, playerCount, acceptedPlayers));
      }

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      Debug.LogFormat("[SERVER] GameListACK games: {0}", gameLobby.Count);
      int packetLength = 4*2;
      foreach (var game in gameLobby) {
        packetLength += 4*6 + Encoding.UTF8.GetByteCount(game.name);
      }

      using (var writer = new DataStreamWriter(packetLength, Allocator.Temp)) {
        writer.Write((int) this.type);
        writer.Write(gameLobby.Count);

        foreach (var game in gameLobby) {
          writer.Write(game.id);
          writer.Write(game.owner);
          writer.Write((int)game.mode);
          writer.Write(game.playerCount);
          writer.Write(game.acceptedPlayers);
          byte[] name = Encoding.UTF8.GetBytes(game.name);
          writer.Write(name.Length);
          writer.Write(name);
        }

        connection.Send(driver, writer);
      }
    }
  }
}