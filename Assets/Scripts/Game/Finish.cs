using System;
using UnityEngine;

public class Finish : MonoBehaviour {
  public int racketID = 0;

  private void Start() {
    if (racketID == 0) throw new Exception("RacketID is not set");
  }
}