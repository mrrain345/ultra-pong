using UnityEngine;

public class Ball : MonoBehaviour {

  [SerializeField] GameController game;  
  [SerializeField] float speed = 10f;
  [SerializeField] float hitAcceleration = 0.15f;
  [SerializeField] GameObject explodeObj;

  new Rigidbody2D rigidbody;
  new AudioManager audio;
  int hits = 0;
  float time = 0f;
  float racketHeight = 1.25f;
  float startSpeed;
  bool active;

  void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
    audio = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    startSpeed = speed;
    Spawn();
  }
  
  Vector3 HitFactor(Vector3 up, Vector3 ballPos, Vector3 racketPos) {
    return Vector3.Project(ballPos - racketPos, up) / racketHeight;
  }

  void OnCollisionEnter2D(Collision2D col) {
    if (!col.gameObject.CompareTag("Racket")) {
      if (!game.isOwner) return;
      audio.PlayWall();
      game.BallMove(transform.position, rigidbody.velocity, 2);
      return;
    }

    game.lastTouched = col.gameObject.GetComponent<Racket>();
    Vector3 hit = HitFactor(col.transform.up, transform.position, col.transform.position);
    Vector2 dir = (col.transform.right + hit).normalized;
    rigidbody.velocity = dir * (speed + hits * hitAcceleration);
    hits++;

    if (!game.isOwner) return;
    audio.PlayRacket();
    game.BallMove(transform.position, rigidbody.velocity, 1);
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
      game.BallMove(transform.position, rigidbody.velocity, 0);
    }

    if (game.game.mode == GameInfo.Mode.BattleRoyal && active) {
      float radius = game.racketRadius + 1.5f;
      if (transform.position.sqrMagnitude > radius*radius) {
        float angle = Vector2.SignedAngle(Vector2.right, transform.position.normalized) + 180f;
        int id = Mathf.RoundToInt(angle * game.playersAlive.Count / 360f);
        if (id >= game.playersAlive.Count) id -= game.playersAlive.Count;
        int playerID = game.playersAlive[id];
        int racketID = game.players[playerID].racket.racketID;
        game.PlayerFail(racketID);
      }
    }
  }

  public void Explode() {
    GameObject obj = GameObject.Instantiate(explodeObj, transform.position + Vector3.back, transform.rotation);
    obj.GetComponent<Explosion>().Explode(this);
    audio.PlayPoint();
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
      game.BallMove(transform.position, rigidbody.velocity, 0);
    }
  }

  public void SetPosition(Vector2 position, Vector2 velocity, int bounceMode) {
    transform.position = position;
    rigidbody.velocity = velocity;

    if (bounceMode == 1) audio.PlayRacket();
    if (bounceMode == 2) audio.PlayWall();
  }
}
