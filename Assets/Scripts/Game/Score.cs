using UnityEngine;
using TMPro;

public class Score : MonoBehaviour {

  [SerializeField] GameController game;
  [SerializeField] RectTransform panel;
  [SerializeField] GameObject scoreObj;
  [SerializeField] TMP_FontAsset[] playerFonts;

  public void UpdateScore() {
    foreach (RectTransform child in panel) Destroy(child.gameObject);
    float size = 1f / game.game.playerCount;

    for (int i = 0; i < game.game.playerCount; i++) {
      int playerID = game.game.playerIDs[i];
      TextMeshProUGUI score = GameObject.Instantiate(scoreObj).GetComponent<TextMeshProUGUI>();
      score.font = playerFonts[i];
      score.text = game.players[playerID].score.ToString();
      RectTransform rect = score.rectTransform;
      rect.SetParent(panel);

      rect.anchorMin = new Vector2(i * size, 0);
      rect.anchorMax = new Vector2((i+1) * size, 1);
      rect.anchoredPosition = Vector2.zero;
      rect.sizeDelta = Vector2.zero;
    }
  }
}