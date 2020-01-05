using System.Collections.Generic;
using UnityEngine;

public class Game {
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

  public List<Player> players;

  public Game(int id, int owner, string name, Mode mode, int playerCount) {
    this.id = id;
    this.owner = owner;
    this.name = name;
    this.mode = mode;
    this.playerCount = playerCount;

    players = new List<Player>(playerCount);
  }
}