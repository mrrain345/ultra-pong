using UnityEngine;

public class Racket : MonoBehaviour {

  [SerializeField] Color playerColor;
  [SerializeField] SpriteRenderer border;
  [SerializeField] float speed = 7;
  [SerializeField] float maxPosition = 3.25f;

  [HideInInspector] public int racketID;
  [HideInInspector] public int playerID;

  GameController gameController;
  Vector3 startPosition;
  bool isLocalPlayer;
  float position = 0;
  float time = 0f;

  public void Setup(GameController gameController, int racketID, int playerID) {
    this.gameController = gameController;
    this.racketID = racketID;
    this.playerID = playerID;

    startPosition = transform.position;
    isLocalPlayer = (gameController.playerID == playerID);
    if (isLocalPlayer) border.color = playerColor;
  }

  private void Update() {
    Debug.DrawLine(startPosition - transform.up * (maxPosition+1.25f), startPosition + transform.up * (maxPosition+1.25f), Color.red);
  }

  void FixedUpdate () {
    if (isLocalPlayer) {
      float pos = Input.GetAxisRaw("Vertical") * (speed / maxPosition) * Time.fixedDeltaTime;
      time += Time.fixedDeltaTime;
      if (Mathf.Approximately(pos, 0f) && time < 2f) return;
      time = 0f;

      position = Mathf.Clamp(position + pos, -1f, 1f);
      transform.position = startPosition + transform.up * (position * maxPosition);
      gameController.RacketMove(position);
    }
  }

  public void OnMove(float position) {
    this.position = position;
    transform.position = startPosition + transform.up * (position * maxPosition);
  }
}
