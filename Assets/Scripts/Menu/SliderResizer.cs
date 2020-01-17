using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderResizer : MonoBehaviour {

  [SerializeField] RectTransform rectTransform;
  [SerializeField] float ratio = 40;

  void Start() {
    rectTransform.sizeDelta = new Vector2(Screen.width / ratio, 0);
  }
}
