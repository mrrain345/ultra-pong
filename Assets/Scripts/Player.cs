using UnityEngine;

public class Player : MonoBehaviour {

  public float speed = 7;
  public string axis = "Vertical";

  new Rigidbody2D rigidbody;

  void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
  }

  void FixedUpdate () {
    float vel = Input.GetAxisRaw(axis) * speed * Time.fixedDeltaTime;
    rigidbody.velocity = new Vector2(0, vel);
  }
}
