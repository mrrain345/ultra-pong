using UnityEngine;

public class Touchpad : MonoBehaviour {

  [SerializeField] GameController gameController;
  bool mousePressed;

  float Remap(float value, float from1, float to1, float from2, float to2) {
    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
  }

  void FixedUpdate() {
    if (Input.touchSupported) {
      if (Input.touchCount == 0) return;
      Touch touch = Input.GetTouch(0);
      Move(touch.position.y);
    }
    else if (!Mathf.Approximately(Input.GetAxisRaw("Vertical"), 0f)) {
      gameController.localPlayer.racket.Move(Input.GetAxisRaw("Vertical"));
    } else {
      if (Input.GetMouseButtonDown(0)) mousePressed = true;
      else if (Input.GetMouseButtonUp(0)) mousePressed = false;
      if (mousePressed) Move(Input.mousePosition.y);
    }
  }

  void Move(float position) {
    float pos = Remap(position, 0, Screen.height, -2f, 2f);
    pos = Mathf.Clamp(pos, -1, 1);
    gameController.localPlayer.racket.Move(pos);
  }
}