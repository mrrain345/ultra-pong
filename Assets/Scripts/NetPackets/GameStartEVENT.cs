using System.Text;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;

// int id, int playerID, int owner, int mode, int nameLength, byte[] name, int playerCount, [int id]
namespace NetPackets {
  public struct GameStartEVENT : INetPacket<GameStartEVENT> {
    public PacketType type => PacketType.GameStartEVENT;

    public int id;
    public int playerID;
    public int owner;
    public GameInfo.Mode mode;
    public string name;
    public int[] playerIDs;

    public GameStartEVENT(int id, int playerID, int owner, GameInfo.Mode mode, string name, int[] playerIDs) {
      this.id = id;
      this.playerID = playerID;
      this.owner = owner;
      this.mode = mode;
      this.name = name;
      this.playerIDs = playerIDs;
    }

    public GameStartEVENT(GameInfo game, int playerID) {
      this.id = game.id;
      this.playerID = playerID;
      this.owner = game.owner;
      this.mode = game.mode;
      this.name = game.name;
      this.playerIDs = game.playerIDs.ToArray();
    }

    public GameInfo GetGame() {
      GameInfo game = new GameInfo(id, owner, name, mode, playerIDs.Length, playerIDs.Length);
      game.playerIDs = new List<int>(playerIDs);
      return game;
    }

    public GameStartEVENT Receive(ref DataStreamReader stream, ref DataStreamReader.Context context) {
      this.id = stream.ReadInt(ref context);
      this.playerID = stream.ReadInt(ref context);
      this.owner = stream.ReadInt(ref context);
      this.mode = (GameInfo.Mode) stream.ReadInt(ref context);
      
      int nameLength = stream.ReadInt(ref context);
      byte[] name = stream.ReadBytesAsArray(ref context, nameLength);
      this.name = Encoding.UTF8.GetString(name);
      
      int playerCount = stream.ReadInt(ref context);
      playerIDs = new int[playerCount];
      for (int i = 0; i < playerCount; i++) playerIDs[i] = stream.ReadInt(ref context);

      return this;
    }

    public void Send(UdpNetworkDriver driver, NetworkConnection connection) {
      byte[] name = Encoding.UTF8.GetBytes(this.name);

      using (var writer = new DataStreamWriter(4*7 + name.Length + playerIDs.Length*4, Allocator.Temp)) {
        writer.Write((int) this.type);

        writer.Write(id);
        writer.Write(playerID);
        writer.Write(owner);
        writer.Write((int) mode);
        writer.Write(name.Length);
        writer.Write(name, name.Length);
        writer.Write(playerIDs.Length);
        foreach (int playerID in playerIDs) writer.Write(playerID);
        
        connection.Send(driver, writer);
      }
    }
  }
}