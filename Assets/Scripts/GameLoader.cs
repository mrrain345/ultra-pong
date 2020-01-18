using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour {
  [SerializeField] Server server;
  [SerializeField] Client client;
  [SerializeField] GameObject menu;
  [SerializeField] new GameObject camera;
  [SerializeField] new GameObject audio;
  [Space]
  [SerializeField] bool editorClient = false;
  [SerializeField] bool serverBuild = false;
  [SerializeField] bool muteAudio = false;

#if UNITY_EDITOR
  private void Start() {
    server.enabled = true;

    if (editorClient) {
      client.enabled = true;
      menu.SetActive(true);
      camera.SetActive(true);
      audio.SetActive(true);
      if (muteAudio) audio.GetComponent<AudioListener>().enabled = false;
    }
  }
#else
  private void Start() {
    if (serverBuild) {
      server.enabled = true;
    } else {
      client.enabled = true;
      menu.SetActive(true);
      camera.SetActive(true);
      audio.SetActive(true);
      if (muteAudio) audio.GetComponent<AudioListener>().enabled = false;
    }
  }
#endif
}
