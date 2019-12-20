using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Client : MonoBehaviour {

  public UdpNetworkDriver driver;
  public NetworkConnection connection;
  public string ip = "127.0.0.1";
  public ushort port = 9000;

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
    Debug.Log("We are now connected to the server");
  }

  void OnDataReceived(DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    PacketType type = (PacketType) stream.ReadInt(ref context);
    Debug.LogFormat("[CLIENT]: {0}", type);

    if (type == PacketType.RacketMove) {
      int id = stream.ReadInt(ref context);
      float position = stream.ReadFloat(ref context);
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
          Debug.Log("Client got disconnected from server");
          connection = default(NetworkConnection);
          IsConnected = false;
          break;
      }
    }
  }
}