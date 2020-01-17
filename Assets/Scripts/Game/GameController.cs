using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

  [Header("General")]
  [SerializeField] GameObject racketObj;
  [SerializeField] Ball ball;
  [SerializeField] Transform background;
  [SerializeField] new Camera camera;
  [SerializeField] Vector2 gravity;
  [SerializeField] [Range(0, 255)] int fieldAlpha = 32;

  [Header("Settings")]
  [SerializeField] GameObject[] maps;
  [Tooltip("x: cameraSize, y: racketRadius")]
  [SerializeField] Vector2[] settings;
  [SerializeField] Vector2 battleSettings;
  [SerializeField] Color[] racketColors;
  [SerializeField] GameObject[] fields;

  string[] testColors = new string[] { "CYAN", "RED", "GREEN", "YELLOW", "MAGENTA", "BLUE" };


  [HideInInspector] public Client client;
  [HideInInspector] public Racket lastTouched;
  [HideInInspector] public List<int> playersAlive;

  public GameInfo game => client.activeGame;
  public int playerID => client.playerID;
  public PlayerInfo localPlayer => players[playerID];
  public bool isOwner => playerID == game.owner;
  public float racketRadius => (game.mode == GameInfo.Mode.BattleRoyal) ? battleSettings.y : settings[game.playerCount-2].y;
  public float cameraSize => (game.mode == GameInfo.Mode.BattleRoyal) ? battleSettings.x : settings[game.playerCount-2].x;
  public Dictionary<int, PlayerInfo> players;

  private void Start() {
    client = FindObjectOfType<Client>();
    if (client == null) {
      SceneManager.LoadScene(0);
      return;
    }

    client.SetGameController(this);
    playersAlive = new List<int>(game.playerIDs);
    SpawnRackets();
    if (game.mode == GameInfo.Mode.BattleRoyal) SpawnFields();
    else maps[game.playerCount-2].SetActive(true);
    lastTouched = players[game.owner].racket;
  }

  public Color GetRacketColor(int racketID) {
    return racketColors[racketID];
  }

  void SpawnRackets() {
    players = new Dictionary<int, PlayerInfo>(game.playerCount);
    background.localScale = new Vector3(cameraSize, cameraSize, 1);
    camera.orthographicSize = cameraSize;

    for (int i = 0; i < game.playerCount; i++) {
      float angle = i * (360 / game.playerCount);
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      Vector3 pos = rot * Vector3.left * racketRadius;

      Racket racket = GameObject.Instantiate(racketObj, pos, rot).GetComponent<Racket>();
      int playerID = game.playerIDs[i];
      players[playerID] = new PlayerInfo(playerID, racket);
      racket.Setup(this, i, playerID);

      if (playerID == this.playerID) {
        camera.transform.rotation = rot;
        Physics2D.gravity = rot * gravity;
      }
    }
  }

  void SpawnFields() {
    GameObject[] oldFields = GameObject.FindGameObjectsWithTag("Field");
    for (int i = 0; i < oldFields.Length; i++) Destroy(oldFields[i]);

    for (int i = 0; i < playersAlive.Count; i++) {
      float angle = i * (360 / playersAlive.Count);
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      Color color = GetRacketColor(players[playersAlive[i]].racket.racketID);
      color.a = fieldAlpha;
      SpriteRenderer field = GameObject.Instantiate(fields[playersAlive.Count-2], Vector3.zero, rot).GetComponent<SpriteRenderer>();
      field.color = color;
    }
  }

  public void OnRacketMove(int playerID, float position) {
    players[playerID].racket.OnMove(position);
  }

  public void OnBallMove(Vector2 position, Vector2 velocity) {
    ball.SetPosition(position, velocity);
  }

  public void OnPlayerFail(int playerID, int lastTouched) {
    if (game.mode != GameInfo.Mode.BattleRoyal) {
      foreach (PlayerInfo player in players.Values) {
        if (playerID == player.id) continue;
        player.score++;
      }
    }
    else {
      playersAlive.Remove(playerID);
      if (playersAlive.Count > 1) {
        foreach (int id in playersAlive) players[id].racket.OnMove(0);
        players[playerID].racket.gameObject.SetActive(false);
      } else {
        players[playersAlive[0]].score++;
        playersAlive.Clear();
        playersAlive.AddRange(game.playerIDs);

        foreach (PlayerInfo player in players.Values) {
          player.racket.gameObject.SetActive(true);
          player.racket.OnMove(0);
          player.racket.startPosition = player.racket.transform.position;
        }
      }
      SpawnFields();
      SetBattleCamera();
    }
    this.lastTouched = players[lastTouched].racket;
    ball.Explode();
  }

  public int getFieldID(int playerID) {
    return playersAlive.IndexOf(playerID);
  }

  void SetBattleCamera() {
    Racket racket = localPlayer.racket;
    if (!playersAlive.Contains(playerID)) return;
    float radius = racketRadius;
    float maxPos = radius * Mathf.PI / racket.maxPosition;
    float startAngle = getFieldID(playerID) * (360 / playersAlive.Count);
    Quaternion rot = Quaternion.AngleAxis(startAngle, Vector3.forward);
    camera.transform.rotation = rot;
    Physics2D.gravity = rot * gravity;
  }

  public void RacketMove(float position) {
    new NetPackets.RacketMove(position).Send(client.driver, client.connection);
  }

  public void OnGameDestroy() {
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

  private void OnGUI() {
    GUILayout.BeginArea(new Rect(0, 35, 300, 500));
    GUILayout.Label("GameID: " + game.id);
    GUILayout.Label("PlayerID: " + playerID);
    GUILayout.Label("OwnerID: " + game.owner);
    GUILayout.Label("RacketID: " + localPlayer.racket.racketID);
    GUILayout.Space(10);
    foreach(PlayerInfo player in players.Values) {
      GUILayout.Label("Player " + player.id + ", racket: " + player.racket.racketID + ", color: " + testColors[player.racket.racketID]);
    }
    GUILayout.EndArea();
  }
}
