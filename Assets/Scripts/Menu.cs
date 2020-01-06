using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;

public class Menu : MonoBehaviour {

  [Header("General")]
  [SerializeField] Client client;
  
  [Header("Canvases")]
  [SerializeField] GameObject mainCanvas;
  [SerializeField] GameObject LobbyCanvas;

  [Header("Left Panel")]
  [SerializeField] GameObject playersObj;
  [SerializeField] TextMeshProUGUI playersText;
  [SerializeField] Button button;

  [Header("Right Panel")]
  [SerializeField] RectTransform lobby;
  [SerializeField] GameObject gameEntry;

  [Header("Lobby Panel")]
  [SerializeField] TextMeshProUGUI lobbyPlayers;

  List<GameLobby> gameLobby;

  public bool isLobby {
    get { return LobbyCanvas.activeInHierarchy; }
    set {
      mainCanvas.SetActive(!value);
      LobbyCanvas.SetActive(value);
    }
  }

  int mode = 0;
  int players = 3;
  string serverName = "";

  float time = 4f;

  private void Start() {
    playersObj.SetActive(false);
    button.interactable = false;
    gameLobby = new List<GameLobby>();
  }

  private void FixedUpdate() {
    time += Time.fixedDeltaTime;
    if (time > 5f) {
      time = 0f;
      RefreshGameList();
    }
  }

  public void OnModeChanged(int mode) {
    this.mode = mode;
    playersObj.SetActive(mode > 0);
    button.interactable = (serverName.Length >= 3);
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
    int players = (mode > 0) ? this.players : 2;

    new NetPackets.GameCreate((GameLobby.Mode)mode, players, serverName).Send(client.driver, client.connection);
  }

  public void RefreshGameList() {
    new NetPackets.GameList().Send(client.driver, client.connection);
  }

  public void UpdateGameList(NetPackets.GameListACK gameListACK) {
    List<GameLobby> gameLobby = gameListACK.gameLobby;
    
    foreach (RectTransform child in lobby) {
      Destroy(child.gameObject);
    }

    lobby.sizeDelta = new Vector2(0, 60 * gameLobby.Count);
    
    for (int i = 0; i < gameLobby.Count; i++) {
      GameObject entry = GameObject.Instantiate(gameEntry);
      entry.GetComponent<MenuLobby>().Initialize(gameLobby[i], this);
      entry.transform.SetParent(lobby);
      RectTransform rect = entry.GetComponent<RectTransform>();
      rect.anchorMin = new Vector2(0, 1);
      rect.anchorMax = new Vector2(1, 1);
      rect.pivot = new Vector2(0, 1);
      rect.localPosition = new Vector3(0, -60 * i, 0);
      rect.sizeDelta = new Vector2(0, 60);
    }
  }

  public void SelectGame(GameLobby game) {
    Debug.LogFormat("[MENU] Game selected: '{0}'", game.name);

    isLobby = true;
    lobbyPlayers.text = game.acceptedPlayers + " / " + game.playerCount;
  }

  public void OnLobbyCancel() {
    isLobby = false;
  }
}
