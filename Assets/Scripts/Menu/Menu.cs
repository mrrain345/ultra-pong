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
  [SerializeField] GameObject disconnectError;

  [Header("Left Panel")]
  [SerializeField] GameObject playersObj;
  [SerializeField] TMP_InputField serverText;
  [SerializeField] Slider playersSlider;
  [SerializeField] TextMeshProUGUI playersText;
  [SerializeField] Button button;

  [Header("Right Panel")]
  [SerializeField] RectTransform lobby;
  [SerializeField] GameObject gameEntry;
  [SerializeField] float entryRatio = 0.15f;

  [Header("Lobby Panel")]
  [SerializeField] TextMeshProUGUI lobbyPlayers;

  List<GameInfo> gameLobby;

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

  float time = 0f;
  bool isYourGame = false;
  GameInfo selectedGame = null;

  private void Start() {
    playersObj.SetActive(false);
    button.interactable = false;
    gameLobby = new List<GameInfo>();
  }

  private void FixedUpdate() {
    time += Time.fixedDeltaTime;
    if (time > 5f) {
      time = 0;
      RefreshGameList();
    }
  }

  public void OnModeChanged(int mode) {
    this.mode = mode;
    playersObj.SetActive(mode > 0);
    if (mode == 1) playersSlider.minValue = 3;
    else if (mode == 2) playersSlider.minValue = 4;
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
    if (!client.IsConnected) return;
    int players = (mode > 0) ? this.players : 2;
    new NetPackets.GameCreate((GameInfo.Mode)mode, players, serverName).Send(client.driver, client.connection);
    OnServerName("");
    serverText.text = "";
  }

  public void RefreshGameList() {
    if (client.IsConnected) new NetPackets.GameList().Send(client.driver, client.connection);
  }

  public void SelectGame(GameInfo game) {
    if (!client.IsConnected) return;
    selectedGame = game;
    isYourGame = false;
    isLobby = true;
    lobbyPlayers.text = game.acceptedPlayers + " / " + game.playerCount;
    new NetPackets.GameJoin(game.id).Send(client.driver, client.connection);
    OnServerName("");
    serverText.text = "";
  }

  public void OnLobbyCancel() {
    if (client.IsConnected) new NetPackets.GameCancel(selectedGame.id).Send(client.driver, client.connection);
    isLobby = false;
    isYourGame = false;
    selectedGame = null;
  }

  public void OnDisconnect() {
    isLobby = false;
    isYourGame = false;
    selectedGame = null;
    disconnectError.SetActive(true);
    RenderGameList(new List<GameInfo>(0));
  }

  public void OnConnect() {
    disconnectError.SetActive(false);
  }



  public void GameListACK(NetPackets.GameListACK gameListACK) {
    RenderGameList(gameListACK.gameLobby);
  }

  void RenderGameList(List<GameInfo> gameLobby) {
    foreach (RectTransform child in lobby) Destroy(child.gameObject);
    float height = Screen.height * entryRatio;
    lobby.sizeDelta = new Vector2(0, height * gameLobby.Count);
    
    for (int i = 0; i < gameLobby.Count; i++) {
      GameObject entry = GameObject.Instantiate(gameEntry);
      entry.GetComponent<MenuLobby>().Initialize(gameLobby[i], this);
      entry.transform.SetParent(lobby);
      RectTransform rect = entry.GetComponent<RectTransform>();
      rect.anchorMin = new Vector2(0, 1);
      rect.anchorMax = new Vector2(0.97f, 1);
      rect.pivot = new Vector2(0, 1);
      rect.anchoredPosition = new Vector3(0, -height * i, 0);
      rect.sizeDelta = new Vector2(0, height);
    }
  }

  public void GameCreateACK(NetPackets.GameCreateACK gameCreateACK) {
    GameInfo game = gameCreateACK.GetGame();
    selectedGame = game;
    isYourGame = true;
    isLobby = true;
    lobbyPlayers.text = game.acceptedPlayers + " / " + game.playerCount;
  }

  public void GameJoinACK(NetPackets.GameJoinACK game) {
    lobbyPlayers.text = game.acceptedPlayers + " / " + game.playerCount;
  }

  public void GameJoinEVENT(NetPackets.GameJoinEVENT game) {
    lobbyPlayers.text = game.acceptedPlayers + " / " + game.playerCount;
  }

  public void GameCancelEVENT(NetPackets.GameCancelEVENT game) {
    lobbyPlayers.text = game.acceptedPlayers + " / " + game.playerCount;
  }

  public void LobbyDestroyEVENT(NetPackets.LobbyDestroyEVENT game) {
    
  }

  public void GameStartEVENT(NetPackets.GameStartEVENT game) {
    isLobby = false;
    client.StartGame(game.GetGame(), game.playerID);
    RenderGameList(new List<GameInfo>(0));
  }
}
