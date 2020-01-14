using System;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Racket : MonoBehaviour, IComparable<Racket> {

  public int racketID = 0;
  public float speed = 7;

  float time = 0f;

  [SerializeField] Color playerColor;
  [SerializeField] GameController gameController;
  [SerializeField] SpriteRenderer border;

  [HideInInspector] public bool isLocalPlayer;
  [HideInInspector] public int playerID;

  new Rigidbody2D rigidbody;

  void Start() {
    if (racketID == 0) throw new Exception("RacketID is not set");
    rigidbody = GetComponent<Rigidbody2D>();

    if (isLocalPlayer) border.color = playerColor; 
  }

  void FixedUpdate () {
    if (isLocalPlayer) {
      float pos = Input.GetAxisRaw("Vertical") * speed * Time.fixedDeltaTime;
      if (Mathf.Approximately(pos, 0)) return;
      rigidbody.MovePosition(rigidbody.position + new Vector2(0, pos));
      gameController.RacketMove(transform.position.y);
    }
  }

  public int CompareTo(Racket racket) {
    return racket.racketID - racketID;
  }

  public void OnMove(float position) {
    Vector3 pos = transform.position;
    pos.y = position;
    transform.position = pos;
  }
}
