using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;

public class Server : MonoBehaviour {
  
  public UdpNetworkDriver driver;
  public ushort port = 9000;

  List<Player> players;
  List<GameLobby> gameLobby;

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
    gameLobby = new List<GameLobby>();
  }


  void OnDataReceived(int id, ref DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    PacketType type = (PacketType) stream.ReadInt(ref context);
    Debug.LogFormat("[SERVER] ({0}): {1}", id, type.ToString());

    switch (type) {
      case PacketType.GameCreate:
        var gameCreate = new NetPackets.GameCreate().Receive(ref stream, ref context);
        GameCreate(id,gameCreate);
        break;
      
      case PacketType.GameList:
        var gameListACK = new NetPackets.GameListACK() { gameLobby = gameLobby };
        gameListACK.Send(driver, players[id].connection);
        break;
    }
  }


  void GameCreate(int id, NetPackets.GameCreate gameCreate) {
    GameLobby game = new GameLobby(gameID++, id, gameCreate.name, gameCreate.mode, gameCreate.players, 1);
    gameLobby.Add(game);
    game.playerIDs = new List<int>();
    game.playerIDs.Add(id);

    Debug.LogFormat("[SERVER] GAME CREATED  name: '{0}', players: {1}, mode: {2}", gameCreate.name, gameCreate.players, gameCreate.mode);
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
