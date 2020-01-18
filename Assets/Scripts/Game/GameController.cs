using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

  [Header("General")]
  [SerializeField] GameObject racketObj;
  [SerializeField] Score score;
  [SerializeField] Ball ball;
  [SerializeField] Vector2 gravity;
  [SerializeField] [Range(0, 255)] int fieldAlpha = 32;

  [Header("Settings")]
  [SerializeField] GameObject[] maps;
  [Tooltip("x: cameraSize, y: racketRadius")]
  [SerializeField] Vector2[] settings;
  [SerializeField] Vector2 battleSettings;
  [SerializeField] Color[] racketColors;
  [SerializeField] GameObject[] fields;
  [SerializeField] int[] scoreSettings;

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

  Transform background;

  private void Start() {
    client = FindObjectOfType<Client>();
    if (client == null) {
      SceneManager.LoadScene(0);
      return;
    }

    background = GameObject.FindGameObjectWithTag("Background").transform;

    client.SetGameController(this);
    SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
    playersAlive = new List<int>(game.playerIDs);

    SpawnRackets();
    if (game.mode == GameInfo.Mode.BattleRoyal) SpawnFields();
    else maps[game.playerCount-2].SetActive(true);
    lastTouched = players[game.owner].racket;
    score.UpdateScore();
  }

  public Color GetRacketColor(int racketID) {
    return racketColors[racketID];
  }

  void SpawnRackets() {
    players = new Dictionary<int, PlayerInfo>(game.playerCount);
    background.localScale = new Vector3(cameraSize, cameraSize, 1);
    Camera.main.orthographicSize = cameraSize;

    for (int i = 0; i < game.playerCount; i++) {
      float angle = i * (360f / game.playerCount);
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      Vector3 pos = rot * Vector3.left * racketRadius;

      Racket racket = Instantiate(racketObj, pos, rot).GetComponent<Racket>();
      int playerID = game.playerIDs[i];
      players[playerID] = new PlayerInfo(playerID, racket);
      racket.Setup(this, i, playerID);

      if (playerID == this.playerID) {
        Camera.main.transform.rotation = rot;
        Physics2D.gravity = rot * gravity;
      }
    }
  }

  void SpawnFields() {
    GameObject[] oldFields = GameObject.FindGameObjectsWithTag("Field");
    for (int i = 0; i < oldFields.Length; i++) Destroy(oldFields[i]);

    for (int i = 0; i < playersAlive.Count; i++) {
      float angle = i * (360f / playersAlive.Count);
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      Color color = GetRacketColor(players[playersAlive[i]].racket.racketID);
      color.a = fieldAlpha / 255f;
      SpriteRenderer field = Instantiate(fields[playersAlive.Count-2], Vector3.zero, rot).GetComponent<SpriteRenderer>();
      field.color = color;
    }
  }

  public void OnRacketMove(int playerID, float position) {
    players[playerID].racket.OnMove(position);
  }

  public void OnBallMove(Vector2 position, Vector2 velocity, int bounceMode) {
    ball.SetPosition(position, velocity, bounceMode);
  }

  public void OnPlayerFail(int playerID, int lastTouched) {
    bool finish = false;

    if (game.mode != GameInfo.Mode.BattleRoyal) {
      foreach (PlayerInfo player in players.Values) {
        if (playerID != player.id) player.score++;
        if (player.score >= scoreSettings[(int) game.mode]) finish = true;
      }
      score.UpdateScore();
      if (finish) GameFinish();
    }
    else {
      playersAlive.Remove(playerID);
      if (playersAlive.Count > 1) {
        foreach (int id in playersAlive) players[id].racket.OnMove(0);
        players[playerID].racket.gameObject.SetActive(false);
      } else {
        players[playersAlive[0]].score++;
        if (players[playersAlive[0]].score >= scoreSettings[(int) game.mode]) finish = true;
        playersAlive.Clear();
        playersAlive.AddRange(game.playerIDs);

        foreach (PlayerInfo player in players.Values) {
          player.racket.gameObject.SetActive(true);
          player.racket.OnMove(0);
          player.racket.startPosition = player.racket.transform.position;
        }

        score.UpdateScore();
      }

      SpawnFields();
      SetBattleCamera();
      if (finish) GameFinish();
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
    float startAngle = getFieldID(playerID) * (360f / playersAlive.Count);
    Quaternion rot = Quaternion.AngleAxis(startAngle, Vector3.forward);
    Camera.main.transform.rotation = rot;
    Physics2D.gravity = rot * gravity;
  }

  public void RacketMove(float position) {
    new NetPackets.RacketMove(position).Send(client.driver, client.connection);
  }

  public void GameFinish() {
    if (isOwner) new NetPackets.GameFinish(game.id).Send(client.driver, client.connection);
  }

  public void OnGameFinish() {
    OnGameDestroy();
  }

  public void OnGameDestroy() {
    background.localScale = new Vector3(5, 5, 1);
    Camera.main.orthographicSize = 5;
    Physics2D.gravity = gravity;
    client.StopGame();
  }


  // Owner only
  public void BallMove(Vector2 position, Vector2 velocity, int bounceMode) {
    if (!isOwner) return;
    new NetPackets.BallMove(position, velocity, bounceMode).Send(client.driver, client.connection);
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
}
