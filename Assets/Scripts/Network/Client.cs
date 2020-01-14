using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Client : MonoBehaviour {

  public UdpNetworkDriver driver;
  public NetworkConnection connection;
  public string ip = "127.0.0.1";
  public ushort port = 9000;
  public Menu menu;

  public bool IsConnected { get; private set; }

  private void Start() {
    driver = new UdpNetworkDriver(new INetworkParameter[0]);
    connection = default(NetworkConnection);

    NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);
    connection = driver.Connect(endpoint);
  }

  public void SendData(DataStreamWriter writer) {
    if (!IsConnected) return;
    connection.Send(driver, writer);
  }

  void OnConnected() {
    Debug.Log("[CLIENT] CONNECTED");
  }


  void OnDataReceived(DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    PacketType type = (PacketType) stream.ReadInt(ref context);
    Debug.LogFormat("[CLIENT]: {0}", type);

    switch(type) {
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
      
      case PacketType.GameStartEVENT:
        if (menu == null) break;
        var gameStartEvent = new NetPackets.GameStartEVENT().Receive(ref stream, ref context);
        menu.GameStartEVENT(gameStartEvent);
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
          break;
      }
    }
  }
}