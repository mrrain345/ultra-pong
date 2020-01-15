using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
  
  public bool isOwner => playerID == game.owner;

  [SerializeField] new CameraController camera;

  Client client;
  GameInfo game => client.activeGame;
  int playerID => client.playerID;
  PlayerInfo localPlayer => players[playerID];

  Dictionary<int, PlayerInfo> players;

  private void Start() {
    client = FindObjectOfType<Client>();
    if (client == null) {
      SceneManager.LoadScene(0);
      return;
    }

    List<Racket> rackets = new List<Racket>(FindObjectsOfType<Racket>());
    rackets.Sort();

    players = new Dictionary<int, PlayerInfo>(game.playerIDs.Count);
    int i = 0;
    foreach (int playerID in game.playerIDs) {
      rackets[i].playerID = playerID;
      if (playerID == this.playerID) {
        rackets[i].isLocalPlayer = true;
      }
      players[playerID] = new PlayerInfo(playerID, rackets[i++]);
    }

    client.SetGameController(this);
    camera.Setup(game, localPlayer.racket.racketID);
  }

  public void OnRacketMove(int playerID, float position) {
    players[playerID].racket.OnMove(position);
  }

  public void OnBallMove(Vector2 position, Vector2 velocity) {
    FindObjectOfType<Ball>().SetPosition(position, velocity);
  }

  public void OnPlayerFail(int playerID) {
    foreach (PlayerInfo player in players.Values) {
      if (playerID == player.id) continue;
      player.score++;
    }
    FindObjectOfType<Ball>().Explode();
  }

  public void RacketMove(float position) {
    new NetPackets.RacketMove(position).Send(client.driver, client.connection);
  }

  public void OnGameDestroy() {
    destroyGame = true;
    //client.StopGame();
    #if UNITY_EDITOR
      Destroy(client);
    #else
      Destroy(client.gameObject);
    #endif
    SceneManager.LoadScene(0);
  }


  // Owner only
  public void BallMove(Vector2 position, Vector2 velocity) {
    if (!isOwner) return;
    new NetPackets.BallMove(position, velocity).Send(client.driver, client.connection);
  }

  // Owner only
  public void PlayerFail(int racketID) {
    if (!isOwner) return;
    foreach (PlayerInfo player in players.Values) {
      if (player.racket.racketID == racketID) {
        new NetPackets.PlayerFail(player.id).Send(client.driver, client.connection);
        return;
      }
    }
  }

  bool destroyGame = false;
  private void OnGUI() {
    GUILayout.BeginArea(new Rect(0, 35, 300, 500));
    GUILayout.Label("GameID: " + game.id);
    GUILayout.Label("PlayerID: " + playerID);
    GUILayout.Label("OwnerID: " + game.owner);
    GUILayout.Label("RacketID: " + localPlayer.racket.racketID);
    GUILayout.Space(10);
    foreach(PlayerInfo player in players.Values) {
      GUILayout.Label("Player " + player.id + " score: " + player.score);
    }
    GUILayout.Space(10);
    if (destroyGame) GUILayout.Label("GAME DESTROYED!");
    GUILayout.EndArea();
  }
}
