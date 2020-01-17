using UnityEngine;

public class Touchpad : MonoBehaviour {

  [SerializeField] GameController gameController;
  [SerializeField] float size = 0.333f;
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
    else if (!Mathf.Approximately(Input.GetAxis("Vertical"), 0f)) {
      gameController.localPlayer.racket.Move(Input.GetAxis("Vertical"));
    } else {
      if (Input.GetMouseButtonDown(0)) mousePressed = true;
      else if (Input.GetMouseButtonUp(0)) mousePressed = false;
      if (mousePressed) Move(Input.mousePosition.y);
    }
  }

  void Move(float position) {
    float height = Screen.height * size * 0.5f;
    float screen = Screen.height * 0.5f;
    float pos = Remap(position, screen - height, screen + height, -1f, 1f);
    pos = Mathf.Clamp(pos, -1, 1);
    gameController.localPlayer.racket.Move(pos);
  }
}