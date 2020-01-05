using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;

public class Server : MonoBehaviour {
  
  public UdpNetworkDriver driver;
  public ushort port = 9000;

  List<Player> players;
  List<Game> gameLobby;

  int gameID = 0;

  private void Start() {
    driver = new UdpNetworkDriver(new INetworkParameter[0]);
    NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
    endpoint.Port = port;
    if (driver.Bind(endpoint) != 0) {
      Debug.LogWarningFormat("[SERVER] Failed to bind to port {0}", port);
    } else {
      driver.Listen();
    }

    players = new List<Player>();
    gameLobby = new List<Game>();
  }

  void OnDataReceived(int id, ref DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    PacketType type = (PacketType) stream.ReadInt(ref context);
    Debug.LogFormat("[SERVER] ({0}): {1}", id, type.ToString());

    switch (type) {
      case PacketType.GameCreate:
        int mode = stream.ReadInt(ref context);
        int players = stream.ReadInt(ref context);
        int nameLength = stream.ReadInt(ref context);
        byte[] name = stream.ReadBytesAsArray(ref context, nameLength);
        GameCreate(id, mode, players, Encoding.UTF8.GetString(name));
        break;
      
      case PacketType.GameList:
        GameList(id);
        break;

      case PacketType.PlayerMove:
        float position = stream.ReadFloat(ref context);
        PlayerMove(id, position);
        break;
    }
  }

  void GameCreate(int id, int mode, int players, string name) {
    gameLobby.Add(new Game(gameID++, id, name, (Game.Mode)mode, (int)players));
    Debug.LogFormat("[SERVER] GAME CREATED  name: '{0}', players: {1}, mode: {2}", name, players, mode);
  }

  void GameList(int id) {
    // int length, [int id, int owner, int mode, int playerCount, int nameLength, byte[] name]
    int packetLength = 4*2;
    foreach (var game in gameLobby) {
      packetLength += 4*5 + Encoding.UTF8.GetByteCount(game.name);
    }

    using (var writer = new DataStreamWriter(packetLength, Allocator.Temp)) {
      writer.Write((int) PacketType.GameListACK);
      writer.Write(gameLobby.Count);

      foreach (var game in gameLobby) {
        writer.Write(game.id);
        writer.Write(game.owner);
        writer.Write((int)game.mode);
        writer.Write(game.playerCount);
        byte[] name = Encoding.UTF8.GetBytes(game.name);
        writer.Write(name.Length);
        writer.Write(name);
      }

      players[id].connection.Send(driver, writer);
    }
  }

  void PlayerMove(int id, float position) {
    using (var writer = new DataStreamWriter(3*4, Allocator.Temp)) {
      writer.Write((int) PacketType.RacketMove);
      writer.Write(id);
      writer.Write(position);

      foreach (var player in players) {
        //if (player.id == id) continue;
        player.connection.Send(driver, writer);
      }
    }
  }

  private void OnDestroy() {
    driver.Dispose();
  }

  private void Update() {
    driver.ScheduleUpdate().Complete();

    // Cleaning up connections
    for (int i = 0; i < players.Count; i++) {
      if (!players[i].connection.IsCreated) {
        players.RemoveAtSwapBack(i);
        --i;
      }
    }

    // Accept new connections
    NetworkConnection conn;

    while ((conn = driver.Accept()) != default(NetworkConnection)) {
      int id = players.Count;
      Player player = new Player {
        id = id,
        connection = conn,
        mode = Player.Mode.Lobby,
        racket = null,
      };

      players.Add(player);
      Debug.LogFormat("[SERVER] NEW CONNECTION id: {0}", id);
    }

    DataStreamReader stream;
    for (int i = 0; i < players.Count; i++) {
      if (!players[i].connection.IsCreated) continue;
      NetworkEvent.Type cmd;
      while ((cmd = driver.PopEventForConnection(players[i].connection, out stream)) != NetworkEvent.Type.Empty) {
        if (cmd == NetworkEvent.Type.Data) OnDataReceived(i, ref stream);
        else if (cmd == NetworkEvent.Type.Disconnect) {
          Debug.LogFormat("[SERVER] DISCONNECTED id: {0}", i);
          players[i].Destroy();
          players[i] = default(Player);
        }
      }
    }
  }
}
