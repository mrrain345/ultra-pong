using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Server : MonoBehaviour {
  
  public UdpNetworkDriver driver;
  public ushort port = 9000;

  List<Player> players;

  private void Start() {
    driver = new UdpNetworkDriver(new INetworkParameter[0]);
    NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
    endpoint.Port = port;
    if (driver.Bind(endpoint) != 0) {
      Debug.LogWarningFormat("Failed to bind to port {0}", port);
    } else {
      driver.Listen();
    }

    players = new List<Player>();
  }

  void OnDataReceived(int id, ref DataStreamReader stream) {
    var context = default(DataStreamReader.Context);
    PacketType type = (PacketType) stream.ReadInt(ref context);
    Debug.LogFormat("[SERVER] ({0}): {1}", id, type.ToString());

    switch (type) {
      case PacketType.PlayerMove:
        float position = stream.ReadFloat(ref context);
        PlayerMove(id, position);
        break;
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
    GameObject[] rackets = GameObject.FindGameObjectsWithTag("Racket");

    while ((conn = driver.Accept()) != default(NetworkConnection)) {
      int id = players.Count;
      Player player = new Player {
        id = id,
        connection = conn,
        racket = rackets[id].GetComponent<Racket>()
      };

      players.Add(player);
      Debug.Log("Accepted a connection");
    }

    DataStreamReader stream;
    for (int i = 0; i < players.Count; i++) {
      if (!players[i].connection.IsCreated) continue;
      NetworkEvent.Type cmd;
      while ((cmd = driver.PopEventForConnection(players[i].connection, out stream)) != NetworkEvent.Type.Empty) {
        if (cmd == NetworkEvent.Type.Data) OnDataReceived(i, ref stream);
        else if (cmd == NetworkEvent.Type.Disconnect) {
          Debug.Log("Client disconnected from server");
          players[i].Destroy();
          players[i] = default(Player);
        }
      }
    }
  }
}
