using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Collections;
using Unity.Networking.Transport;

public class Client : MonoBehaviour {

  public UdpNetworkDriver driver;
  public NetworkConnection connection;
  public string ip = "127.0.0.1";
  public ushort port = 9000;

  [SerializeField] Menu menu;
  [SerializeField] List<PacketType> disableLogger = new List<PacketType>();

  GameController gameController;
  float time;
  float lastPing = 0f;

  public bool IsConnected { get; private set; }
  public GameInfo activeGame { get; private set; }
  public int playerID { get; private set; }
  public bool isGame => gameController != null;

  private void Start() {
    Connect();
  }

  void Connect() {
    if (SceneManager.GetActiveScene().buildIndex == 1) {
      SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
      SceneManager.UnloadSceneAsync(1);
      menu.gameObject.SetActive(true);
    }

    time = 0f;
    lastPing = 0f;

    driver = new UdpNetworkDriver(new INetworkParameter[0]);
    connection = default(NetworkConnection);

    NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);
    connection = driver.Connect(endpoint);
  }

  void OnConnected() {
    Debug.Log("[CLIENT] CONNECTED");
    menu.OnConnect();
    menu.RefreshGameList();
  }

  public void SetGameController(GameController gameController) {
    this.gameController = gameController;
  } 

  public void StartGame(GameInfo game, int playerID) {
    menu.gameObject.SetActive(false);
    activeGame = game;
    this.playerID = playerID;
    SceneManager.LoadScene(1, LoadSceneMode.Additive);
  }

  public void StopGame() {
    activeGame = null;
    gameController = null;
    SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
    SceneManager.UnloadSceneAsync(1);
    menu.gameObject.SetActive(true);
    menu.RefreshGameList();
  }

  private void FixedUpdate() {
    lastPing += Time.fixedDeltaTime;
    if (lastPing > 3.1f) menu.OnDisconnect();

    if (!IsConnected) return;
    time += Time.fixedDeltaTime;

    if (time > 1f) {
      time = 0;
      new NetPackets.Ping().Send(driver, connection);
    }

    if (lastPing > 3.1f) {
      Debug.Log("[CLIENT] RECONNECT");
      menu.OnDisconnect();
      connection.Disconnect(driver);
      driver.Dispose();
      IsConnected = false;
      Connect();
    }
  }


  void OnDataReceived(DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    PacketType type = (PacketType) stream.ReadInt(ref context);
    if (!disableLogger.Contains(type)) Debug.LogFormat("[CLIENT]: {0}", type);

    switch(type) {
      case PacketType.PingACK:
        lastPing = 0f;
        break;

      // MENU
      case PacketType.LobbyChangedEVENT:
        if (menu == null) break;
        new NetPackets.GameList().Send(driver, connection);
        break;

      case PacketType.GameListACK:
        if (menu == null) break;
        var gameList = new NetPackets.GameListACK().Receive(ref stream, ref context);
        menu.GameListACK(gameList);
        break;

      case PacketType.GameCreateACK:
        if (menu == null) break;
        var gameCreate = new NetPackets.GameCreateACK().Receive(ref stream, ref context);
        menu.GameCreateACK(gameCreate);
        break;
      
      case PacketType.GameJoinACK:
        if (menu == null) break;
        var gameJoin = new NetPackets.GameJoinACK().Receive(ref stream, ref context);
        menu.GameJoinACK(gameJoin);
        break;
      
      case PacketType.GameJoinEVENT:
        if (menu == null) break;
        var gameJoinEvent = new NetPackets.GameJoinEVENT().Receive(ref stream, ref context);
        menu.GameJoinEVENT(gameJoinEvent);
        break;
      
      case PacketType.GameCancelEVENT:
        if (menu == null) break;
        var gameCancelEvent = new NetPackets.GameCancelEVENT().Receive(ref stream, ref context);
        menu.GameCancelEVENT(gameCancelEvent);
        break;

      case PacketType.LobbyDestroyEVENT:
        if (menu == null) break;
        var lobbyDestroyEvent = new NetPackets.LobbyDestroyEVENT().Receive(ref stream, ref context);
        menu.LobbyDestroyEVENT(lobbyDestroyEvent);
        break;
      
      case PacketType.GameStartEVENT:
        if (menu == null) break;
        var gameStartEvent = new NetPackets.GameStartEVENT().Receive(ref stream, ref context);
        menu.GameStartEVENT(gameStartEvent);
        break;

      // GAME
      case PacketType.RacketMoveEVENT:
        if (!isGame) break;
        var racketMoveEvent = new NetPackets.RacketMoveEVENT().Receive(ref stream, ref context);
        gameController.OnRacketMove(racketMoveEvent.playerID, racketMoveEvent.position, racketMoveEvent.velocity);
        break;

      case PacketType.BallMoveEVENT:
        if (!isGame) break;
        var ballMoveEvent = new NetPackets.BallMoveEVENT().Receive(ref stream, ref context);
        gameController.OnBallMove(ballMoveEvent.position, ballMoveEvent.velocity, ballMoveEvent.bounceMode);
        break;

      case PacketType.PlayerFailEVENT:
        if (!isGame) break;
        var playerFailEvent = new NetPackets.PlayerFailEVENT().Receive(ref stream, ref context);
        gameController.OnPlayerFail(playerFailEvent.failed, playerFailEvent.lastTouched);
        break;
      
      case PacketType.GameDestroyEVENT:
        if (!isGame) break;
        var gameDestroyEvent = new NetPackets.GameDestroyEVENT().Receive(ref stream, ref context);
        if (gameDestroyEvent.id == activeGame.id) gameController.OnGameDestroy();
        break;

      case PacketType.GameFinishEVENT:
        if (!isGame) break;
        var gameFinishEvent = new NetPackets.GameFinishEVENT().Receive(ref stream, ref context);
        if (gameFinishEvent.id == activeGame.id) gameController.OnGameFinish();
        break;
    }
  }



  private void OnDestroy() {
    connection.Disconnect(driver);
    connection = default(NetworkConnection);
    driver.Dispose();
  }

  private void Update() {
    driver.ScheduleUpdate().Complete();
    if (!connection.IsCreated) return;

    DataStreamReader stream;
    NetworkEvent.Type cmd;
    while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty) {
      switch (cmd) {
        case NetworkEvent.Type.Connect:
          OnConnected();
          IsConnected = true;
          break;
        case NetworkEvent.Type.Data:
          OnDataReceived(stream);
          break;
        case NetworkEvent.Type.Disconnect:
          Debug.Log("[CLIENT] DISCONNECTED");
          connection = default(NetworkConnection);
          IsConnected = false;
          Connect();
          break;
      }
    }
  }
}