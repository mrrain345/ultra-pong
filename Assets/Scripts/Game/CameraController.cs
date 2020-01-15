using UnityEngine;

public class CameraController : MonoBehaviour {
  
  GameInfo game;
  int racketID = 0;

  Vector2 gravity;

  private void Awake() {
    gravity = Physics2D.gravity;
  }

  public void Setup(GameInfo game, int racketID) {
    this.game = game;
    this.racketID = racketID;

    if (game.mode == GameInfo.Mode.PlayerVsPlayer) {
      if (racketID == 1) {
        transform.rotation = Quaternion.identity;
        Physics2D.gravity = gravity;
      } else if (racketID == 2) {
        transform.rotation = Quaternion.Euler(0, 0, 180);
        Physics2D.gravity = -gravity;
      } else {
        Debug.LogError("Bad RacketID: " + racketID);
      }
    } else {
      Debug.LogError("Bad game mode: " + game.mode.ToString());
    }
  }
}