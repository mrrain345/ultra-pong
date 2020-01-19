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
  [SerializeField] bool localServer = false;

#if UNITY_EDITOR
  private void Start() {
    if (localServer) server.enabled = true;

    if (editorClient) {
      if (localServer) client.ip = "127.0.0.1";
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
      if (localServer) client.ip = "127.0.0.1";
      client.enabled = true;
      menu.SetActive(true);
      camera.SetActive(true);
      audio.SetActive(true);
      if (muteAudio) audio.GetComponent<AudioListener>().enabled = false;
    }
  }
#endif
}
