using UnityEngine;

public class Ball : MonoBehaviour {

  public GameController game;  
  public float speed = 10f;
  public float hitAcceleration = 0.15f;
  public GameObject explodeObj;

  new Rigidbody2D rigidbody;
  int hits = 0;
  float time = 0f;
  float racketHeight = 1.25f;
  float startSpeed;
  bool active;

  void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
    startSpeed = speed;
    Spawn();
  }
  
  Vector3 HitFactor(Vector3 up, Vector3 ballPos, Vector3 racketPos) {
    return Vector3.Project(ballPos - racketPos, up) / racketHeight;
  }

  void OnCollisionEnter2D(Collision2D col) {
    if (!col.gameObject.CompareTag("Racket")) {
      if (game.isOwner) game.BallMove(transform.position, rigidbody.velocity);
      return;
    }
    if (game.isOwner) game.lastTouched = col.gameObject.GetComponent<Racket>();
    Vector3 hit = HitFactor(col.transform.up, transform.position, col.transform.position);
    Vector2 dir = (col.transform.right + hit).normalized;
    rigidbody.velocity = dir * (speed + hits * hitAcceleration);
    hits++;

    if (game.isOwner) game.BallMove(transform.position, rigidbody.velocity);
  }

  void OnTriggerEnter2D(Collider2D col) {
    if (!game.isOwner) return;
    if (!col.gameObject.CompareTag("Finish")) return;
    int racketID = col.gameObject.GetComponent<Finish>().racketID;

    game.PlayerFail(racketID);
  }

  private void FixedUpdate() {
    if (!game.isOwner) return;

    time += Time.fixedDeltaTime;
    if (time > 0.2f) {
      time = 0f;
      game.BallMove(transform.position, rigidbody.velocity);
    }

    if (game.game.mode == GameInfo.Mode.BattleRoyal && active) {
      float radius = game.racketRadius + 1.5f;
      if (transform.position.sqrMagnitude > radius*radius) {
        float angle = Vector2.SignedAngle(Vector2.right, transform.position.normalized) + 180f;
        int id = Mathf.RoundToInt(angle * game.playersAlive.Count / 360);
        if (id >= game.playersAlive.Count) id -= game.playersAlive.Count;
        Debug.LogFormat("Player Failed: fieldID: {0}, playerID: {1}", id, game.playersAlive[id]);
        int playerID = game.playersAlive[id];
        int racketID = game.players[playerID].racket.racketID;
        game.PlayerFail(racketID);
      }
    }
  }

  public void Explode() {
    GameObject obj = GameObject.Instantiate(explodeObj, transform.position + Vector3.back, transform.rotation);
    obj.GetComponent<Explosion>().Explode(this);
    hits = 0;
    speed = startSpeed;
    transform.position = new Vector3(1000, 1000, 0);
    rigidbody.velocity = Vector3.zero;
    active = false;
  }

  public void Spawn() {
    active = true;
    if (game.isOwner) {
      transform.position = Vector3.zero;
      Vector3 dir = game.lastTouched.startPosition.normalized;
      rigidbody.velocity = dir * speed;
      game.BallMove(transform.position, rigidbody.velocity);
    }
  }

  public void SetPosition(Vector2 position, Vector2 velocity) {
    transform.position = position;
    rigidbody.velocity = velocity;
  }
}
