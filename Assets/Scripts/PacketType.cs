public enum PacketType : int {
  None = 0,
  GameCreate,     // int mode, int playerCount, int nameLength, byte[] name
  GameCreateACK,  // int id, int mode, int playerCount, int nameLength, byte[] name
  GameJoin,       // int id
  GameJoinACK,    // int id, int playerCount, int acceptedPlayers
  GamePlayerJoin, // int id, int player, int playerCount, int acceptedPlayers
  GameList,       //
  GameListACK,    // int length, [int id, int owner, int mode, int playerCount, int acceptedPlayers, int nameLength, byte[] name]
  GameFinish,
  PlayerMove,     
  RacketMove      
}