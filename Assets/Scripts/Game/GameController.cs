using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

  [SerializeField] GameObject racketObj;
  [SerializeField] GameObject ball;
  [SerializeField] Transform background;
  [SerializeField] new Camera camera;
  [SerializeField] Vector2 gravity;

  [Space]
  [SerializeField] GameObject[] maps;
  [Tooltip("x: cameraSize, y: racketRadius")]
  [SerializeField] Vector2[] settings;


  [HideInInspector] public Client client;
  [HideInInspector] public Racket lastTouched;

  public GameInfo game => client.activeGame;
  public int playerID => client.playerID;
  public PlayerInfo localPlayer => players[playerID];
  public bool isOwner => playerID == game.owner;
  Dictionary<int, PlayerInfo> players;

  private void Start() {
    client = FindObjectOfType<Client>();
    if (client == null) {
      SceneManager.LoadScene(0);
      return;
    }

    client.SetGameController(this);
    SpawnRackets();
    maps[game.playerCount-2].SetActive(true);
    GameObject.Instantiate(ball);
  }

  void SpawnRackets() {
    players = new Dictionary<int, PlayerInfo>(game.playerCount);
    float racketRadius = settings[game.playerCount-2].y;
    float cameraSize = settings[game.playerCount-2].x;
    background.localScale = new Vector3(cameraSize, cameraSize, 1);

    for (int i = 0; i < game.playerCount; i++) {
      float angle = i * (360 / game.playerCount);
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      Vector3 pos = rot * Vector3.left * racketRadius;

      Racket racket = GameObject.Instantiate(racketObj, pos, rot).GetComponent<Racket>();
      int playerID = game.playerIDs[i];
      racket.Setup(this, i, playerID);
      players[playerID] = new PlayerInfo(playerID, racket);

      if (playerID == this.playerID) {
        camera.transform.rotation = rot;
        camera.orthographicSize = cameraSize;
        Physics2D.gravity = rot * gravity;
        lastTouched = racket;
      }
    }
  }

  public void OnRacketMove(int playerID, float position) {
    players[playerID].racket.OnMove(position);
  }

  public void OnBallMove(Vector2 position, Vector2 velocity) {
    Ball ball = FindObjectOfType<Ball>();
    if (ball != null) ball.SetPosition(position, velocity);
  }

  public void OnPlayerFail(int playerID, int lastTouched) {
    foreach (PlayerInfo player in players.Values) {
      if (playerID == player.id) continue;
      player.score++;
    }
    this.lastTouched = players[lastTouched].racket;
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
        new NetPackets.PlayerFail(player.id, lastTouched.playerID).Send(client.driver, client.connection);
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
