using UnityEngine;
using Unity.Networking.Transport;

public class Player {
  public int id;
  public bool lobby;
  public float lastPing;
  public NetworkConnection connection;

  public Player(int id, NetworkConnection connection) {
    this.id = id;
    this.lobby = true;
    this.lastPing = 0f;
    this.connection = connection;
  }

  public void Destroy() {}
  
  public void SetLobby(bool lobby) {
    this.lobby = lobby;
  }
}