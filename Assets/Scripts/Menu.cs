using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;

public class Menu : MonoBehaviour {

  [SerializeField] Client client;
  [SerializeField] GameObject playersObj;
  [SerializeField] TextMeshProUGUI playersText;
  [SerializeField] Button button;

  List<Game> gameLobby;

  int mode = 0;
  int players = 3;
  string serverName = "";

  float time = 4f;

  private void Start() {
    playersObj.SetActive(false);
    button.interactable = false;
    gameLobby = new List<Game>();
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
    byte[] name = Encoding.UTF8.GetBytes(serverName);
    int players = (mode > 0) ? this.players : 2;

    using (var writer = new DataStreamWriter(4*4 + name.Length, Allocator.Temp)) {
      writer.Write((int) PacketType.GameCreate);
      writer.Write(mode);
      writer.Write(players);
      writer.Write(name.Length);
      writer.Write(name, name.Length);
      client.SendData(writer);
    }
  }

  public void RefreshGameList() {
    using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
      writer.Write((int) PacketType.GameList);
      client.SendData(writer);
    }
  }

  public void UpdateGameList(DataStreamReader stream, ref DataStreamReader.Context context) {
    // int length, [int id, int owner, int mode, int playerCount, int nameLength, byte[] name]
    
    int length = stream.ReadInt(ref context);
    gameLobby = new List<Game>(length);

    Debug.LogFormat("[MENU] Lobby count: {0}", length);

    for (int i = 0; i < length; i++) {
      int id = stream.ReadInt(ref context);
      int owner = stream.ReadInt(ref context);
      int mode = stream.ReadInt(ref context);
      int playerCount = stream.ReadInt(ref context);
      int nameLength = stream.ReadInt(ref context);
      byte[] name = stream.ReadBytesAsArray(ref context, nameLength);

      gameLobby.Insert(i, new Game(id, owner, Encoding.UTF8.GetString(name), (Game.Mode)mode, playerCount));
      Debug.LogFormat("[MENU]  name: '{0}', id: {1}, owner: {2}, mode: {3}, players: {4}", Encoding.UTF8.GetString(name), id, owner, mode, players);
    }
  }
}
