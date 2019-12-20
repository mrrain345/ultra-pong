using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Racket : MonoBehaviour {

  public float speed = 7;
  public bool isLocalPlayer;

  Client client;
  new Rigidbody2D rigidbody;

  void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
    client = GameObject.FindGameObjectWithTag("GameController").GetComponent<Client>();
  }

  void FixedUpdate () {
    if (isLocalPlayer) {
      float pos = Input.GetAxisRaw("Vertical") * speed * Time.fixedDeltaTime;
      rigidbody.MovePosition(rigidbody.position + new Vector2(0, pos));
      SendPosition();
    }
  }

  void SendPosition() {
    using (var writer = new DataStreamWriter(16, Allocator.Temp)) {
      writer.Write((int) PacketType.PlayerMove);
      writer.Write(transform.position.y);
      client.SendData(writer);
    }
  }
}
