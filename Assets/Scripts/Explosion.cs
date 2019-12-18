using UnityEngine;

public class Explosion : MonoBehaviour {
  
  public float multiplier = 5f;
  public float lifeTime = 3f;

  public void Explode(Vector3 velocity = new Vector3()) {
    foreach (Transform child in transform) {
      Rigidbody2D rbody = child.GetComponent<Rigidbody2D>();
      rbody.constraints = RigidbodyConstraints2D.None;
      rbody.velocity = child.localPosition * multiplier + velocity;
    }

    Destroy(gameObject, lifeTime);
  }
}
