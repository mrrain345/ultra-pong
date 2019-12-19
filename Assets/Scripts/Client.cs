using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Client : MonoBehaviour {

  public UdpNetworkDriver driver;
  public NetworkConnection connection;

  int data = 1;
  float time = 0;

  private void Start() {
    driver = new UdpNetworkDriver(new INetworkParameter[0]);
    connection = default(NetworkConnection);

    NetworkEndPoint endpoint = NetworkEndPoint.LoopbackIpv4;
    endpoint.Port = 9000;
    connection = driver.Connect(endpoint);
  }

  void SendValue(int value) {
    using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
      writer.Write(value);
      connection.Send(driver, writer);
    }
  }


  void OnConnected() {
    Debug.Log("We are now connected to the server");
    SendValue(data);
  }

  void OnDataReceived(DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    int value = stream.ReadInt(ref context);
    Debug.LogFormat("Client received: {0}", value);
    data = value;
  }

  private void FixedUpdate() {
    time += Time.fixedDeltaTime;

    if (time >= 1f) {
      time -= 1f;
      SendValue(data);
    }
  }



  public void OnDestroy() {
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
          break;
        case NetworkEvent.Type.Data:
          OnDataReceived(stream);
          break;
        case NetworkEvent.Type.Disconnect:
          Debug.Log("Client got disconnected from server");
          connection = default(NetworkConnection);
          break;
      }
    }
  }
}