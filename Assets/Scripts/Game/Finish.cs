using System;
using UnityEngine;

public class Finish : MonoBehaviour {
  public int racketID = -1;

  private void Start() {
    if (racketID == -1) throw new Exception("RacketID is not set");
  }
}