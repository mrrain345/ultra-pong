using UnityEngine;

public class Ball : MonoBehaviour {
  
  public float speed = 10f;
  public float hitAcceleration = 0.15f;
  public GameObject explodeObj;

  GameController game;
  new Rigidbody2D rigidbody;
  int hits = 0;
  float time = 0f;
  float racketHeight = 1.25f;

  void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
    game = FindObjectOfType<GameController>();
    Vector3 dir = game.lastTouched.startPosition.normalized;
    rigidbody.velocity = dir * speed;
  }

  private void Update() {
    Debug.DrawRay(Vector3.zero, game.lastTouched.startPosition.normalized * 3, Color.red);
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
    if (time < 0.2f) return;
    time = 0f;
    game.BallMove(transform.position, rigidbody.velocity);
  }

  public void Explode() {
    GameObject obj = GameObject.Instantiate(explodeObj, transform.position + Vector3.back, transform.rotation);
    obj.GetComponent<Explosion>().Explode();
    Destroy(gameObject);
  }

  public void SetPosition(Vector2 position, Vector2 velocity) {
    transform.position = position;
    rigidbody.velocity = velocity;
  }
}
