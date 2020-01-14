using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;

public class Server : MonoBehaviour {
  
  public UdpNetworkDriver driver;
  public ushort port = 9000;

  List<Player> players;
  List<GameInfo> gameLobby;
  List<GameInfo> activeGames;

  int gameID = 0;
  int playerID = 0;

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
    gameLobby = new List<GameInfo>();
    activeGames = new List<GameInfo>();
  }

  void OnDataReceived(int id, ref DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    PacketType type = (PacketType) stream.ReadInt(ref context);
    //Debug.LogFormat("[SERVER] ({0}): {1}", id, type.ToString());
    int gameID;

    switch (type) {
      // MENU
      case PacketType.GameCreate:
        var gameCreate = new NetPackets.GameCreate().Receive(ref stream, ref context);
        GameCreate(id, gameCreate);
        break;
      
      case PacketType.GameList:
        new NetPackets.GameListACK(gameLobby).Send(driver, players[id].connection);
        break;

      case PacketType.GameJoin:
        gameID = new NetPackets.GameJoin().Receive(ref stream, ref context).id;
        GameJoin(id, gameID);
        break;
      
      case PacketType.GameCancel:
        gameID = new NetPackets.GameCancel().Receive(ref stream, ref context).id;
        GameCancel(id, gameID);
        break;
      
      // GAME
      case PacketType.RacketMove:
        float RacketPos = new NetPackets.RacketMove().Receive(ref stream, ref context).position;
        RacketMove(id, RacketPos);
        break;

      case PacketType.BallMove:
        var ballMove = new NetPackets.BallMove().Receive(ref stream, ref context);
        BallMove(id, ballMove);
        break;
      
      case PacketType.PlayerFail:
        int playerID = new NetPackets.PlayerFail().Receive(ref stream, ref context).id;
        PlayerFail(id, playerID);
        break;
    }
  }

  void LobbyChangedEvent() {
    var lobbyChanged = new NetPackets.LobbyChangedEVENT();
    foreach (Player player in players) {
      if (!player.lobby) continue;
      lobbyChanged.Send(driver, player.connection);
    }
  }


  void GameCreate(int id, NetPackets.GameCreate gameCreate) {
    GameInfo game = new GameInfo(gameID++, players[id].id, gameCreate.name, gameCreate.mode, gameCreate.players, 0);
    game.AddPlayer(id);
    gameLobby.Add(game);

    new NetPackets.GameCreateACK(game).Send(driver, players[id].connection);
    Debug.LogFormat("[SERVER] GAME CREATED  name: '{0}', players: {1}, mode: {2}", gameCreate.name, gameCreate.players, gameCreate.mode);
    LobbyChangedEvent();
  }

  void GameJoin(int id, int gameID) {
    GameInfo game = gameLobby.Find(g => g.id == gameID);
    if (game == null || game.acceptedPlayers >= game.playerCount) {
      new NetPackets.GameJoinACK(false);
    } else {
      game.AddPlayer(id);
      new NetPackets.GameJoinACK(game).Send(driver, players[id].connection);

      var joinEvent = new NetPackets.GameJoinEVENT(game);
      foreach (int playerID in game.playerIDs) {
        if (playerID == id) continue;
        joinEvent.Send(driver, players[playerID].connection);
      }

      if (game.acceptedPlayers == game.playerCount) GameStart(game);
      LobbyChangedEvent();
    }
  }

  void GameCancel(int id, int gameID) {
    GameInfo game = gameLobby.Find(g => g.id == gameID);
    if (game == null) return;
    if (game.owner == id) {
      var destroyEvent = new NetPackets.GameDestroyEVENT(gameID);
      foreach (int playerID in game.playerIDs) {
        if (playerID == id) continue;
        destroyEvent.Send(driver, players[playerID].connection);
      }
      gameLobby.Remove(game);
    }
    else {
      game.RemovePlayer(id);
      var cancelEvent = new NetPackets.GameCancelEVENT(game);
      foreach (int playerID in game.playerIDs) {
        if (playerID == id) continue;
        cancelEvent.Send(driver, players[playerID].connection);
      }
    }
    new NetPackets.GameCancelACK(gameID).Send(driver, players[id].connection);
    LobbyChangedEvent();
  }

  void GameStart(GameInfo game) {
    gameLobby.Remove(game);
    activeGames.Add(game);

    foreach (int id in game.playerIDs) {
      players[id].SetLobby(false);
      new NetPackets.GameStartEVENT(game, id).Send(driver, players[id].connection);
    }
  }

  void RacketMove(int id, float position) {
    GameInfo game = activeGames.Find(g => g.ContainsPlayer(id));
    if (game == null) return;

    var racketMoveEvent = new NetPackets.RacketMoveEVENT(id, position);
    foreach (int playerID in game.playerIDs) {
      if (playerID == id) continue;
      racketMoveEvent.Send(driver, players[playerID].connection);
    }
  }

  void BallMove(int id, NetPackets.BallMove ballMove) {
    GameInfo game = activeGames.Find(g => g.ContainsPlayer(id));
    if (game == null) return;
    if (game.owner != id) return;

    var ballMoveEvent = new NetPackets.BallMoveEVENT(ballMove.position, ballMove.velocity);
    foreach (int playerID in game.playerIDs) {
      if (playerID == id) continue;
      ballMoveEvent.Send(driver, players[playerID].connection);
    }
  }

  void PlayerFail(int id, int playerID) {
    GameInfo game = activeGames.Find(g => g.ContainsPlayer(id));
    if (game == null) return;
    if (game.owner != id) return;

    var playerFailEvent = new NetPackets.PlayerFailEVENT(playerID);
    foreach (int player in game.playerIDs) {
      playerFailEvent.Send(driver, players[player].connection);
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
      Player player = new Player {
        id = playerID,
        lobby = true,
        connection = conn
      };

      players.Insert(playerID++, player);
      Debug.LogFormat("[SERVER] NEW CONNECTION id: {0}", player.id);
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
