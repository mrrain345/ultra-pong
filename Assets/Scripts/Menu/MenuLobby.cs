using UnityEngine;
using TMPro;

public class MenuLobby : MonoBehaviour {

  [SerializeField] new TextMeshProUGUI name;
  [SerializeField] TextMeshProUGUI players;
  [SerializeField] TextMeshProUGUI mode;

  GameInfo game;
  Menu menu;

  string[] modes = new string[] { "Player vs Player", "Octagon", "Battle Royal" };

  public void Initialize(GameInfo game, Menu menu) {
    this.game = game;
    this.menu = menu;

    name.text = game.name;
    players.text = game.acceptedPlayers + " / " + game.playerCount;
    mode.text = modes[(int)game.mode];
  }

  public void OnClick() {
    if (menu != null) menu.SelectGame(game);
  }
}