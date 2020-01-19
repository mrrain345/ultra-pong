using UnityEngine;

public class SmoothCamera : MonoBehaviour {

  [SerializeField] AnimationCurve curve;
  [SerializeField] float duration = 1f;

  Quaternion from;
  Quaternion to;
  float time = 2f;
  
  private void Start() {
    from = transform.rotation;
    to = transform.rotation;
  }

  public void RotateTo(Quaternion rot) {
    from = transform.rotation;
    to = rot;
    time = 0f;
  }

  private void Update() {
    if (time > 1f) return;
    time += Time.deltaTime;
    float val = curve.Evaluate(time / duration);

    transform.rotation = Quaternion.Slerp(from, to, val);
  }
}