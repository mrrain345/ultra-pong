using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Server : MonoBehaviour {
  
  public UdpNetworkDriver driver;

  NativeList<NetworkConnection> connections;

  private void Start() {
    driver = new UdpNetworkDriver(new INetworkParameter[0]);
    NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
    endpoint.Port = 9000;
    if (driver.Bind(endpoint) != 0) {
      Debug.LogWarning("Failed to bind to port 9000");
    } else {
      driver.Listen();
    }

    connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
  }


  void SendValue(int conn, int value) {
    using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
      writer.Write(value);
      driver.Send(NetworkPipeline.Null, connections[conn], writer);
    }
  }

  void OnDataReceived(int conn, ref DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    int value = stream.ReadInt(ref context);
    Debug.LogFormat("Server received: {0}", value);

    SendValue(conn, value+1);
  }



  private void OnDestroy() {
    driver.Dispose();
    connections.Dispose();
  }

  private void Update() {
    driver.ScheduleUpdate().Complete();

    // Cleaning up connections
    for (int i = 0; i < connections.Length; i++) {
      if (!connections[i].IsCreated) {
        connections.RemoveAtSwapBack(i);
        --i;
      }
    }

    // Accept new connections
    NetworkConnection conn;
    while ((conn = driver.Accept()) != default(NetworkConnection)) {
      connections.Add(conn);
      Debug.Log("Accepted a connection");
    }

    DataStreamReader stream;
    for (int i = 0; i < connections.Length; i++) {
      if (!connections[i].IsCreated) continue;
      NetworkEvent.Type cmd;
      while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty) {
        if (cmd == NetworkEvent.Type.Data) OnDataReceived(i, ref stream);
        else if (cmd == NetworkEvent.Type.Disconnect) {
          Debug.Log("Client disconnected from server");
          connections[i] = default(NetworkConnection);
        }
      }
    }
  }
}
