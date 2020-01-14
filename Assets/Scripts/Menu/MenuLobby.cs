using UnityEngine;
using TMPro;

public class MenuLobby : MonoBehaviour {
  [SerializeField] new TextMeshProUGUI name;
  [SerializeField] TextMeshProUGUI players;
  [SerializeField] TextMeshProUGUI mode;

  GameInfo game;
  Menu menu;

  public void Initialize(GameInfo game, Menu menu) {
    this.game = game;
    this.menu = menu;

    name.text = game.name;
    players.text = game.acceptedPlayers + " / " + game.playerCount;
    mode.text = game.mode.ToString();
  }

  public void OnClick() {
    if (menu != null) menu.SelectGame(game);
  }
}