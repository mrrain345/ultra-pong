﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

  [Header("General")]
  [SerializeField] GameObject racketObj;
  [SerializeField] Score score;
  [SerializeField] Ball ball;
  [SerializeField] Vector2 gravity;
  [SerializeField] [Range(0, 1)] float fieldAlpha = 0.3f;
  [SerializeField] [Range(0, 1)] float deadFields = 0.2f;
  [SerializeField] [Range(0, 1)] float deadAlpha = 0.8f;

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
  public bool isBattleRoyal => game.mode == GameInfo.Mode.BattleRoyal;
  public float racketRadius => isBattleRoyal ? battleSettings.y : settings[game.playerCount-2].y;
  public float cameraSize => isBattleRoyal ? battleSettings.x : settings[game.playerCount-2].x;
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
    if (isBattleRoyal) SpawnFields();
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
      
      if (!playersAlive.Contains(playerID)) {
        color.r *= deadFields;
        color.g *= deadFields;
        color.b *= deadFields;
        color.a = deadAlpha;
      }
      else color.a = fieldAlpha;

      SpriteRenderer field = Instantiate(fields[playersAlive.Count-2], Vector3.zero, rot).GetComponent<SpriteRenderer>();
      field.color = color;
    }
  }

  public void OnRacketMove(int playerID, float position, float velocity) {
    players[playerID].racket.OnMove(position, velocity);
  }

  public void OnBallMove(Vector2 position, Vector2 velocity, int bounceMode) {
    ball.SetPosition(position, velocity, bounceMode);
  }

  public void OnPlayerFail(int playerID, int lastTouched) {
    bool finish = false;

    if (!isBattleRoyal) {
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
        players[playerID].racket.gameObject.SetActive(false);

        foreach (int id in playersAlive) {
          Racket racket = players[id].racket;
          racket.OnMove(0, 0);
          racket.startPosition = racket.transform.position;
        }
      } else {
        players[playersAlive[0]].score++;
        if (players[playersAlive[0]].score >= scoreSettings[(int) game.mode]) finish = true;
        playersAlive.Clear();
        playersAlive.AddRange(game.playerIDs);

        foreach (PlayerInfo player in players.Values) {
          player.racket.gameObject.SetActive(true);
          player.racket.OnMove(0, 0);
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
    Camera.main.GetComponent<SmoothCamera>().RotateTo(rot);
    Physics2D.gravity = rot * gravity;
  }

  public void RacketMove(float position, float velocity) {
    new NetPackets.RacketMove(position, velocity).Send(client.driver, client.connection);
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

    if (isBattleRoyal && playersAlive.Count > 2) {
      if (!playersAlive.Contains(lastTouched.playerID)) {
        lastTouched = players[playersAlive[0]].racket;
      }
      else if (lastTouched.racketID == racketID) {
        int playerID = lastTouched.playerID;
        int index = playersAlive.IndexOf(playerID) + 1;
        if (index >= playersAlive.Count) index = 0;
        lastTouched = players[playersAlive[index]].racket;
      }
    }

    foreach (PlayerInfo player in players.Values) {
      if (player.racket.racketID == racketID) {
        new NetPackets.PlayerFail(player.id, lastTouched.playerID).Send(client.driver, client.connection);
        return;
      }
    }
  }
}
