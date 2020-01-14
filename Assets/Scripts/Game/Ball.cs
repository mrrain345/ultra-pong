using UnityEngine;

public class Ball : MonoBehaviour {
  
  public float speed = 10f;
  public float hitAcceleration = 0.1f;
  public GameObject explodeObj;

  new Rigidbody2D rigidbody;
  int hits = 0;

  void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
    rigidbody.velocity = Vector2.left * speed;
  }
  
  float hitFactor(Vector2 ballPos, Vector2 racketPos, float racketHeight) {
    return (ballPos.y - racketPos.y) / racketHeight;
  }

  void OnCollisionEnter2D(Collision2D col) {
    if (!col.gameObject.CompareTag("Racket")) return;
    float y = hitFactor(transform.position, col.transform.position, col.collider.bounds.size.y);
    Vector2 dir = new Vector2(col.transform.right.x, y).normalized;
    rigidbody.velocity = dir * (speed + hits * hitAcceleration);
    hits++;
  }

  void OnTriggerEnter2D(Collider2D col) {
    if (!col.gameObject.CompareTag("Finish")) return;
    GameObject obj = GameObject.Instantiate(explodeObj, transform.position + Vector3.back, transform.rotation);
    obj.GetComponent<Explosion>().Explode();
    Destroy(gameObject);
  }
}
