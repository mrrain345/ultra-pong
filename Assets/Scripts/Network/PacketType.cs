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
  LobbyDestroyEVENT,// int id
  GameStartEVENT,   // int id, playerID, int owner, int mode, int nameLength, byte[] name, int playerCount, [int id]
  LobbyChangedEVENT,//
  GameList,         //
  GameListACK,      // int length, [int id, int owner, int mode, int playerCount, int acceptedPlayers, int nameLength, byte[] name]
  GameFinish,
  RacketMove,       // float position
  RacketMoveEVENT,  // int playerID, float position
  BallMove,         // float position.x, float position.y, float velocity.x, float velocity.y
  BallMoveEVENT,    // float position.x, float position.y, float velocity.x, float velocity.y
  PlayerFail,       // int id
  PlayerFailEVENT,  // int id
  Ping,             //
  PingACK,          //
}