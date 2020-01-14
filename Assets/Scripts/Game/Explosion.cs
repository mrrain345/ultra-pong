using UnityEngine;

public class Explosion : MonoBehaviour {
  
  public GameObject ball;
  public float multiplier = 5f;
  public float lifeTime = 3f;
  
  float time = 0;
  bool created = false;

  public void Explode(Vector3 velocity = new Vector3()) {
    foreach (Transform child in transform) {
      Rigidbody2D rbody = child.GetComponent<Rigidbody2D>();
      rbody.constraints = RigidbodyConstraints2D.None;
      rbody.velocity = child.localPosition * multiplier + velocity;
    }

    Destroy(gameObject, lifeTime);
  }

  private void FixedUpdate() {
    time += Time.fixedDeltaTime;
    if (!created && time > 1f) {
      created = true;
      GameObject.Instantiate(ball, Vector3.zero, Quaternion.identity);
    }
  }
}
