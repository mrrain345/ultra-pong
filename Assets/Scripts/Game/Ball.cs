using UnityEngine;

public class Ball : MonoBehaviour {
  
  public float speed = 10f;
  public float hitAcceleration = 0.1f;
  public GameObject explodeObj;

  GameController game;
  new Rigidbody2D rigidbody;
  int hits = 0;
  float time = 0f;

  void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
    rigidbody.velocity = Vector2.left * speed;
    game = FindObjectOfType<GameController>();
  }
  
  float hitFactor(Vector2 ballPos, Vector2 racketPos, float racketHeight) {
    return (ballPos.y - racketPos.y) / racketHeight;
  }

  void OnCollisionEnter2D(Collision2D col) {
    if (!col.gameObject.CompareTag("Racket")) {
      if (game.isOwner) game.BallMove(transform.position, rigidbody.velocity);
      return;
    }
    float y = hitFactor(transform.position, col.transform.position, col.collider.bounds.size.y);
    Vector2 dir = new Vector2(col.transform.right.x, y).normalized;
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
