using UnityEngine;
using Unity.Networking.Transport;

public struct Player {

  public int id;
  public NetworkConnection connection;
  public Racket racket;

  public void Destroy() {

  }
}