using UnityEngine;
using Unity.Networking.Transport;

public struct Player {

  public enum Mode : int {
    None = 0,
    Lobby,
    GameMaking,
    Game
  }

  public int id;
  public NetworkConnection connection;
  public Mode mode;
  public Racket racket;

  public void Destroy() {

  }
}