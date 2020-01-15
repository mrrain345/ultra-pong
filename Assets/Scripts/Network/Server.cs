using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;

public class Server : MonoBehaviour {
  
  public UdpNetworkDriver driver;
  public ushort port = 9000;

  [SerializeField] List<PacketType> disableLogger = new List<PacketType>();

  Dictionary<int, Player> players;
  List<GameInfo> gameLobby;
  List<GameInfo> activeGames;

  int gameNew = 0;
  int playerNew = 0;

  private void Start() {
    driver = new UdpNetworkDriver(new INetworkParameter[0]);
    NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
    endpoint.Port = port;
    if (driver.Bind(endpoint) != 0) {
      Debug.LogWarningFormat("[SERVER] Failed to bind to port {0}", port);
    } else {
      driver.Listen();
    }

    players = new Dictionary<int, Player>();
    gameLobby = new List<GameInfo>();
    activeGames = new List<GameInfo>();
  }

  void OnDataReceived(Player player, ref DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    PacketType type = (PacketType) stream.ReadInt(ref context);
    if (!disableLogger.Contains(type)) Debug.LogFormat("[SERVER] ({0}): {1}", player.id, type);
    
    int gameID;

    switch (type) {
      // MENU
      case PacketType.GameCreate:
        var gameCreate = new NetPackets.GameCreate().Receive(ref stream, ref context);
        LobbyCreate(player, gameCreate);
        break;
      
      case PacketType.GameList:
        new NetPackets.GameListACK(gameLobby).Send(driver, player.connection);
        break;

      case PacketType.GameJoin:
        gameID = new NetPackets.GameJoin().Receive(ref stream, ref context).id;
        LobbyJoin(player, gameID);
        break;
      
      case PacketType.GameCancel:
        gameID = new NetPackets.GameCancel().Receive(ref stream, ref context).id;
        LobbyCancel(player, gameID);
        break;
      
      // GAME
      case PacketType.RacketMove:
        float RacketPos = new NetPackets.RacketMove().Receive(ref stream, ref context).position;
        RacketMove(player, RacketPos);
        break;

      case PacketType.BallMove:
        var ballMove = new NetPackets.BallMove().Receive(ref stream, ref context);
        BallMove(player, ballMove);
        break;
      
      case PacketType.PlayerFail:
        int playerID = new NetPackets.PlayerFail().Receive(ref stream, ref context).id;
        PlayerFail(player, playerID);
        break;
    }
  }

  void LobbyChangedEvent() {
    var lobbyChanged = new NetPackets.LobbyChangedEVENT();
    foreach (Player player in players.Values) {
      if (!player.lobby) continue;
      lobbyChanged.Send(driver, player.connection);
    }
  }


  void LobbyCreate(Player player, NetPackets.GameCreate gameCreate) {
    GameInfo game = new GameInfo(gameNew++, player.id, gameCreate.name, gameCreate.mode, gameCreate.players, 0);
    game.AddPlayer(player.id);
    gameLobby.Add(game);

    new NetPackets.GameCreateACK(game).Send(driver, player.connection);
    Debug.LogFormat("[SERVER] GAME CREATED  name: '{0}', players: {1}, mode: {2}", gameCreate.name, gameCreate.players, gameCreate.mode);
    LobbyChangedEvent();
  }

  void LobbyJoin(Player player, int gameID) {
    GameInfo game = gameLobby.Find(g => g.id == gameID);
    if (game == null || game.acceptedPlayers >= game.playerCount) {
      new NetPackets.GameJoinACK(false);
    } else {
      game.AddPlayer(player.id);
      new NetPackets.GameJoinACK(game).Send(driver, player.connection);

      var joinEvent = new NetPackets.GameJoinEVENT(game);
      foreach (int playerID in game.playerIDs) {
        if (playerID == player.id) continue;
        joinEvent.Send(driver, players[playerID].connection);
      }

      if (game.acceptedPlayers == game.playerCount) GameStart(game);
      LobbyChangedEvent();
    }
  }

  bool LobbyCancel(Player player, int gameID) {
    GameInfo game = gameLobby.Find(g => g.id == gameID);
    if (game == null) return false;
    if (game.owner == player.id) {
      var destroyEvent = new NetPackets.LobbyDestroyEVENT(gameID);
      foreach (int playerID in game.playerIDs) {
        destroyEvent.Send(driver, players[playerID].connection);
      }
      LobbyChangedEvent();
      return gameLobby.Remove(game);
    }
    else {
      game.RemovePlayer(player.id);
      var cancelEvent = new NetPackets.GameCancelEVENT(game);
      foreach (int playerID in game.playerIDs) {
        if (playerID == player.id) continue;
        cancelEvent.Send(driver, players[playerID].connection);
      }
    }
    new NetPackets.GameCancelACK(gameID).Send(driver, player.connection);
    LobbyChangedEvent();
    return false;
  }

  void GameStart(GameInfo game) {
    gameLobby.Remove(game);
    activeGames.Add(game);

    foreach (int PlayerID in game.playerIDs) {
      players[PlayerID].SetLobby(false);
      new NetPackets.GameStartEVENT(game, PlayerID).Send(driver, players[PlayerID].connection);
    }
  }

  void RacketMove(Player player, float position) {
    GameInfo game = activeGames.Find(g => g.ContainsPlayer(player.id));
    if (game == null) return;

    var racketMoveEvent = new NetPackets.RacketMoveEVENT(player.id, position);
    foreach (int playerID in game.playerIDs) {
      if (playerID == player.id) continue;
      racketMoveEvent.Send(driver, players[playerID].connection);
    }
  }

  void BallMove(Player player, NetPackets.BallMove ballMove) {
    GameInfo game = activeGames.Find(g => g.ContainsPlayer(player.id));
    if (game == null) return;
    if (game.owner != player.id) return;

    var ballMoveEvent = new NetPackets.BallMoveEVENT(ballMove.position, ballMove.velocity);
    foreach (int playerID in game.playerIDs) {
      if (playerID == player.id) continue;
      ballMoveEvent.Send(driver, players[playerID].connection);
    }
  }

  void PlayerFail(Player player, int playerID) {
    GameInfo game = activeGames.Find(g => g.ContainsPlayer(player.id));
    if (game == null) return;
    if (game.owner != player.id) return;

    var playerFailEvent = new NetPackets.PlayerFailEVENT(playerID);
    foreach (int id in game.playerIDs) {
      playerFailEvent.Send(driver, players[id].connection);
    }
  }


  void PlayerDisconnect(Player player) {
    for (int i = 0; i < gameLobby.Count; i++) {
      if (gameLobby[i].ContainsPlayer(player.id)) {
        if (LobbyCancel(player, gameLobby[i].id)) {
          gameLobby.RemoveAt(i--);
        }
      }
    }

    for (int i = 0; i < activeGames.Count; i++) {
      GameInfo game = activeGames[i];
      if (game.ContainsPlayer(player.id)) {
        var gameDestroyEVENT = new NetPackets.GameDestroyEVENT(game.id);
        Debug.LogFormat("[SERVER] DESTROY GAME: {0}", game.id);
        foreach (int playerID in game.playerIDs) {
          if (playerID == player.id) continue;
          gameDestroyEVENT.Send(driver, players[playerID].connection);
          players[playerID].SetLobby(true);
        }
        activeGames.RemoveAt(i--);
      }
    }
  }



  private void OnDestroy() {
    driver.Dispose();
  }

  List<int> disconnected = new List<int>();

  private void Update() {
    driver.ScheduleUpdate().Complete();
    
    // Cleaning up connections
    foreach (Player player in players.Values) {
      if (!player.connection.IsCreated) {
        PlayerDisconnect(player);
        disconnected.Add(player.id);
      }
    }
    foreach (int id in disconnected) players.Remove(id);
    disconnected.Clear();

    // Accept new connections
    NetworkConnection conn;

    while ((conn = driver.Accept()) != default(NetworkConnection)) {
      Player player = new Player(playerNew, conn);
      players[playerNew++] = player;
      Debug.LogFormat("[SERVER] NEW CONNECTION id: {0}", player.id);
    }

    DataStreamReader stream;

    foreach (Player player in players.Values) {
      if (!player.connection.IsCreated) continue;
      NetworkEvent.Type cmd;
      while ((cmd = driver.PopEventForConnection(player.connection, out stream)) != NetworkEvent.Type.Empty) {
        if (cmd == NetworkEvent.Type.Data) OnDataReceived(player, ref stream);
        else if (cmd == NetworkEvent.Type.Disconnect) {
          Debug.LogFormat("[SERVER] DISCONNECTED id: {0}", player.id);
          PlayerDisconnect(player);
          disconnected.Add(player.id);
        }
      }
    }

    foreach (int id in disconnected) players.Remove(id);
    disconnected.Clear();
  }
}
