using UnityEngine;
using Unity.Networking.Transport;

public struct Player {
  public int id;
  public bool lobby;
  public NetworkConnection connection;

  public void Destroy() {}
  
  public void SetLobby(bool lobby) {
    this.lobby = lobby;
  }
}