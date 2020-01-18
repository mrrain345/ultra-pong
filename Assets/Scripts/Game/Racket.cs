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
  float velocity = 0;
  float time = 0;

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
      float velocity = position * (speed / maxPosition);
      float pos = velocity * Time.fixedDeltaTime;
      if (Mathf.Approximately(pos, 0f)) return;

      OnMove(this.position + pos, velocity);
      gameController.RacketMove(this.position, velocity);
    }
  }

  public void OnMove(float position, float velocity) {
    this.position = Mathf.Clamp(position, -1, 1);
    this.velocity = velocity;
    this.time = 0f;
    UpdatePosition();
  }

  public void UpdatePosition() {
    float position = Mathf.Clamp(this.position + this.velocity * this.time, -1, 1);
    if (gameController.game.mode != GameInfo.Mode.BattleRoyal) {
      transform.position = startPosition + transform.up * (position * maxPosition);
      return;
    }

    float radius = gameController.racketRadius;
    //float maxPos = radius * Mathf.PI / maxPosition;
    float startAngle = gameController.getFieldID(playerID) * (360f / gameController.playersAlive.Count);
    float angle = Remap(position, -1, 1, -180, 180);
    Quaternion rot = Quaternion.AngleAxis(startAngle - angle, Vector3.forward);
    transform.position = rot * Vector3.left * radius;
    transform.rotation = rot;
  }

  private void Update() {
    time += Time.deltaTime;
  }

  private void FixedUpdate() {
    if (!isLocalPlayer) UpdatePosition();
    else if (time > Time.fixedDeltaTime * 2) {
      if (!Mathf.Approximately(velocity, 0)) {
        time = 0f;
        velocity = 0f;
        gameController.RacketMove(position, 0);
      }
    }
  }
}
