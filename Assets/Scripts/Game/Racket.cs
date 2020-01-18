using UnityEngine;

public class Racket : MonoBehaviour {

  [SerializeField] SpriteRenderer borderSprite;
  [SerializeField] SpriteRenderer racketSprite;
  [SerializeField] float speed = 7;
  public float maxPosition = 3.25f;

  [HideInInspector] public int racketID;
  [HideInInspector] public int playerID;
  [HideInInspector] public Vector3 startPosition;

  GameController gameController;
  bool isLocalPlayer;
  float position = 0;

  float Remap(float value, float from1, float to1, float from2, float to2) {
    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
  }

  public void Setup(GameController gameController, int racketID, int playerID) {
    this.gameController = gameController;
    this.racketID = racketID;
    this.playerID = playerID;

    startPosition = transform.position;
    isLocalPlayer = (gameController.playerID == playerID);
    borderSprite.color = gameController.GetRacketColor(racketID);
    if (isLocalPlayer) {
      borderSprite.sortingOrder = 2;
      racketSprite.sortingOrder = 3;
    }
  }

  public void Move(float position) {
    if (isLocalPlayer) {
      if (gameController.game.mode == GameInfo.Mode.BattleRoyal) {
        BattleMove(position);
        return;
      }

      float pos = position * (speed / maxPosition) * Time.fixedDeltaTime;
      if (Mathf.Approximately(pos, 0f)) return;

      this.position = Mathf.Clamp(this.position + pos, -1f, 1f);
      transform.position = startPosition + transform.up * (this.position * maxPosition);
      gameController.RacketMove(this.position);
    }
  }

  void BattleMove(float position) {
    float pos = position * (speed / maxPosition) * Time.fixedDeltaTime;
    if (Mathf.Approximately(pos, 0f)) return;

    OnMove(this.position + pos);
    gameController.RacketMove(this.position);
  }

  public void OnMove(float position) {
    if (gameController.game.mode != GameInfo.Mode.BattleRoyal) {
      this.position = position;
      transform.position = startPosition + transform.up * (position * maxPosition);
      return;
    }

    this.position = position;
    float radius = gameController.racketRadius;
    float maxPos = radius * Mathf.PI / maxPosition;
    float startAngle = gameController.getFieldID(playerID) * (360f / gameController.playersAlive.Count);
    float angle = Remap(position, -maxPos, maxPos, -180, 180);
    Quaternion rot = Quaternion.AngleAxis(startAngle - angle, Vector3.forward);
    transform.position = rot * Vector3.left * radius;
    transform.rotation = rot;
  }
}
