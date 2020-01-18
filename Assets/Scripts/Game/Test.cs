using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Test : MonoBehaviour {

  [Range(2, 6)] public int players = 3;
  public float cameraSize = 5;
  public float racketRadius = 7.5f;
  public float maxPosition = 3.25f;
  [Space]
  public new Camera camera;

  public Transform point;
  public float offset = 1.5f;

  [Space]
  public Color[] racketColors;
  public GameObject[] fields;
  [Range(2, 6)] public int playersCount = 6;

  //public List<int> playersAlive = new List<int>(new int[] { 0, 1, 2, 3, 4, 5});

  List<Vector3> positions = new List<Vector3>();
  List<Quaternion> rotations = new List<Quaternion>();

  int oldPlayers;
  int oldPlayersCount;

  public Vector2[] settings = new Vector2[] {
    new Vector2(5.0f, 7.5f),  // 2
    new Vector2(9.5f, 7.5f),  // 3
    new Vector2(6.0f, 5.0f),  // 4
    new Vector2(8.5f, 6.5f),  // 5
    new Vector2(10.0f, 8.2f)  // 6
  };

  bool refresh;

  private void OnValidate() {
    /*if (oldPlayers != players) {
      oldPlayers = players;
      cameraSize = settings[players-2].x;
      racketRadius = settings[players-2].y;
    }

    camera.orthographicSize = cameraSize;
    SpawnRackets();*/
    //PrintPositions();

    if (oldPlayersCount != playersCount) {
      oldPlayersCount = playersCount;
      refresh = true;
    }
  }

  void SpawnRackets() {
    positions.Clear();
    rotations.Clear();

    for (int i = 0; i < players; i++) {
      float angle = i * (360 / players);
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      Vector3 pos = rot * Vector3.left * racketRadius;

      positions.Add(pos);
      rotations.Add(rot);
      
      int playerID = players;
    }
  }

  /*void DebugFields() {
    float racketRadius = 8.2f + offset;
    string[] testColors = new string[] { "CYAN", "RED", "GREEN", "YELLOW", "MAGENTA", "BLUE" };

    if (point.position.sqrMagnitude > racketRadius*racketRadius) {
      float angle = Vector2.SignedAngle(Vector2.right, point.position.normalized) + 180f;
      int id = Mathf.RoundToInt(angle * playersAlive.Count / 360);
      
      if (id >= playersAlive.Count) id -= playersAlive.Count;
      Debug.LogFormat("Player Failed: fieldID: {0}, racketID: {1}, color: {2}, angle: {3}", id, playersAlive[id], testColors[playersAlive[id]], angle);
      //game.PlayerFail(game.playersAlive[id]);
    }
  }*/

  void SpawnFields() {
    GameObject[] oldFields = GameObject.FindGameObjectsWithTag("Field");
    for (int i = 0; i < oldFields.Length; i++) DestroyImmediate(oldFields[i]);

    for (int i = 0; i < playersCount; i++) {
      float angle = i * (360f / playersCount);
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      Color color = racketColors[i];
      color.a = 32 / 255f;
      SpriteRenderer field = GameObject.Instantiate(fields[playersCount-2], Vector3.zero, rot).GetComponent<SpriteRenderer>();
      field.color = color;
    }
  }

  void Update() {
    if (refresh) {
      refresh = false;
      SpawnFields();
    }
    /*for (int i = 0; i < positions.Count; i++) {
      Vector3 up = rotations[i] * Vector3.up;
      Debug.DrawLine(positions[i] - up * (maxPosition+1.25f), positions[i] + up * (maxPosition+1.25f), Color.red);
    }*/
  }

  void PrintPositions() {
    for (int i = 0; i < players; i++) {
      float angle = i * (360 / players) + (180 / players);
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      Vector3 pos = rot * Vector3.left * racketRadius;

      Debug.LogFormat("({0}) pos: {1}, rot: {2}", i+1, pos + (rot * (Vector3.left*7f)), rot.eulerAngles);
    }
  }
}