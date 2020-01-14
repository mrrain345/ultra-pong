public enum PacketType : int {
  None = 0,
  GameCreate,       // int mode, int playerCount, int nameLength, byte[] name
  GameCreateACK,    // int id, int mode, int playerCount, int nameLength, byte[] name
  GameJoin,         // int id
  GameJoinACK,      // int id, int playerCount, int acceptedPlayers
  GameJoinEVENT,    // int id, int playerCount, int acceptedPlayers
  GameCancel,       // int id
  GameCancelACK,    // int id
  GameCancelEVENT,  // int id, int playerCount, int acceptedPlayers
  GameDestroyEVENT, // int id
  GameStartEVENT,   // int id, int owner, int mode, int nameLength, byte[] name, int playerCount, [int id]
  LobbyChangedEVENT,//
  GameList,         //
  GameListACK,      // int length, [int id, int owner, int mode, int playerCount, int acceptedPlayers, int nameLength, byte[] name]
  GameFinish,
  PlayerMove,     
  RacketMove      
}