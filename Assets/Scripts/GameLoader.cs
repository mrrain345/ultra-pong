using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour {
  [SerializeField] Server server;
  [SerializeField] Client client;
  [SerializeField] GameObject menu;
  [SerializeField] new GameObject camera;
  [Space]
  [SerializeField] bool editorClient = false;
  [SerializeField] bool serverBuild = false;
  
#if UNITY_EDITOR
  static bool instanceExists = false;

  private void Start() {
    if (!instanceExists) {
      server.enabled = true;
      instanceExists = true;
    }

    if (editorClient) {
      client.enabled = true;
      menu.SetActive(true);
      camera.SetActive(true);
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
    }
  }
#endif
}
