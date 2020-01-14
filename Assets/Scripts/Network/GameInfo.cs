using System.Collections.Generic;
using UnityEngine;

public class GameInfo {
  public enum Mode : int {
    PlayerVsPlayer = 0,
    Octagon,
    BattleRoyal
  }

  public int id { get; private set; }
  public int owner { get; private set; }
  public string name { get; private set; }
  public Mode mode { get; private set; }
  public int playerCount { get; private set; }
  public int acceptedPlayers { get; private set; }

  public List<int> playerIDs;

  public GameInfo(int id, int owner, string name, Mode mode, int playerCount, int acceptedPlayers) {
    this.id = id;
    this.owner = owner;
    this.name = name;
    this.mode = mode;
    this.playerCount = playerCount;
    this.acceptedPlayers = acceptedPlayers;
  }

  public void AddPlayer(int id) {
    if (playerIDs == null) playerIDs = new List<int>(playerCount);
    playerIDs.Add(id);
    acceptedPlayers++;
  }

  public void RemovePlayer(int id) {
    if (playerIDs == null) return;
    if (playerIDs.Remove(id)) acceptedPlayers--;
  }

  public bool ContainsPlayer(int id) {
    return playerIDs.Contains(id);
  }
}