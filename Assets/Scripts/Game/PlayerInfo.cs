public class PlayerInfo {

  public int id { get; private set; }
  public Racket racket;
  public int score;

  public PlayerInfo(int id, Racket racket) {
    this.id = id;
    this.racket = racket;
    this.score = 0;
  }
}