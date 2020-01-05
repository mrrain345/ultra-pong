public enum PacketType : int {
  None = 0,
  GameCreate,     // int mode, int playerCount, int nameLength, byte[] name
  GameCreateACK,  // int id, int mode, int playerCount, int nameLength, byte[] name
  GameJoin,       // uint id
  GameList,
  GameListACK,    // int length, [int id, int owner, int mode, int playerCount, int nameLength, byte[] name]
  GameFinish,
  PlayerMove,     // float position
  RacketMove      // float position
}