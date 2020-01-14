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
  
#if UNITY_EDITOR
  private void Start() {
    server.enabled = true;
    if (editorClient) {
      client.enabled = true;
      menu.SetActive(true);
      camera.SetActive(true);
    }
  }
#else
  private void Start() {
    client.enabled = true;
    menu.SetActive(true);
    camera.SetActive(true);
  }
#endif
}
