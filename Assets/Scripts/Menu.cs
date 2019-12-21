using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour {

  [SerializeField] GameObject playersObj;
  [SerializeField] GameObject serverNameObj;
  [SerializeField] TextMeshProUGUI playersText;
  [SerializeField] Button button;
  [SerializeField] TextMeshProUGUI buttonText;

  int mode = 0;
  int players = 3;
  string serverName = "";

  private void Start() {
    playersObj.SetActive(false);
    serverNameObj.SetActive(false);
  }

  public void OnModeChanged(int mode) {
    this.mode = mode;
    serverNameObj.SetActive(mode > 0);
    playersObj.SetActive(mode > 1);
    buttonText.text = (mode == 0) ? "Play!" : "Create server!";
    button.interactable = (mode == 0 || serverName.Length >= 3);
  }

  public void OnServerName(string serverName) {
    this.serverName = serverName;
    button.interactable = (serverName.Length >= 3);
  }

  public void OnPlayersChanged(float players) {
    this.players = (int)(players+0.5f);
    playersText.text = this.players.ToString();
  }

  public void OnStart() {
    // TODO
  }
}
