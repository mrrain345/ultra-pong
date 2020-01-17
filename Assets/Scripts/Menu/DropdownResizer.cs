using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownResizer : MonoBehaviour {

  [SerializeField] RectTransform template;
  [SerializeField] RectTransform content;
  [SerializeField] float ratio = 14.4f;
  [SerializeField] int items = 3;

  void Start() {
    template.sizeDelta = new Vector2(0, Screen.height / ratio * items);
    content.sizeDelta = new Vector2(0, Screen.height / ratio);
  }
}
