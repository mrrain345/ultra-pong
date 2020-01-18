using UnityEngine;

public class AudioManager : MonoBehaviour {

  [Header("Music")]
  [SerializeField] AudioSource music;
  [SerializeField] int loopTime = 100;

  [Header("Sounds")]
  [SerializeField] AudioSource point;
  [SerializeField] AudioSource racket;
  [SerializeField] AudioSource wall;

  private void FixedUpdate() {
    if (!music.isPlaying) {
      music.Play();
      music.time = loopTime;
    }
  }

  public void PlayPoint() {
    if (!point.isPlaying) point.Play();
  }

  public void PlayRacket() {
    if (!racket.isPlaying) racket.Play();
  }

  public void PlayWall() {
    if (!wall.isPlaying) wall.Play();
  }
}