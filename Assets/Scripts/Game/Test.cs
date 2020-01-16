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

  List<Vector3> positions = new List<Vector3>();
  List<Quaternion> rotations = new List<Quaternion>();

  int oldPlayers;

  public Vector2[] settings = new Vector2[] {
    new Vector2(5.0f, 7.5f),  // 2
    new Vector2(9.5f, 7.5f),  // 3
    new Vector2(6.0f, 5.0f),  // 4
    new Vector2(8.5f, 6.5f),  // 5
    new Vector2(10.0f, 8.2f)  // 6
  };

  private void OnValidate() {
    if (oldPlayers != players) {
      oldPlayers = players;
      cameraSize = settings[players-2].x;
      racketRadius = settings[players-2].y;
    }

    camera.orthographicSize = cameraSize;
    SpawnRackets();
    //PrintPositions();
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

  void Update() {
    for (int i = 0; i < positions.Count; i++) {
      Vector3 up = rotations[i] * Vector3.up;
      Debug.DrawLine(positions[i] - up * (maxPosition+1.25f), positions[i] + up * (maxPosition+1.25f), Color.red);
    }
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